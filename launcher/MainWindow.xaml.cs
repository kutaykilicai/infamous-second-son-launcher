using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;

namespace InfamousLauncher
{
    public partial class MainWindow : Window
    {
        // Bilinen inFAMOUS Second Son kimlikleri (bölgeye göre)
        static readonly string[] KnownIds = { "CUSA00004", "CUSA00223", "CUSA00264", "CUSA00502", "CUSA00616" };

        readonly string baseDir = AppContext.BaseDirectory;
        string EmuDir => Path.Combine(baseDir, "emulator");
        string EmuExe => Path.Combine(EmuDir, "shadPS4.exe");
        static string SettingsFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "InfamousLauncher", "ayarlar.json");

        string? gameDir; string? titleId;

        public MainWindow()
        {
            InitializeComponent();
            ApplyTheme();
            KeysText.Text =
                "W A S D   hareket          Fare      kamera\n" +
                "Sol tık   ateş (R2)        Sağ tık   nişan (L2)\n" +
                "Boşluk    zıpla (X)        Sol Shift kaçış (O)\n" +
                "C         kare             F         üçgen\n" +
                "Q / E     L1 / R1          R / G     dokunmatik sol/sağ\n" +
                "Enter     menü             Oklar     yön tuşları\n" +
                "Y  fare/kamera değiştir    F11 tam ekran   F9 duraklat\n" +
                "Ctrl + Shift + End  emülatörü kapat";
            SourceInitialized += (_, _) => ApplyBackdrop();
            Loaded += (_, _) => LoadSaved();
        }

        // ---- Görünüm ----
        bool IsDark()
        {
            try { using var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"); return (k?.GetValue("AppsUseLightTheme") as int?) == 0; }
            catch { return true; }
        }
        void ApplyTheme()
        {
            bool d = IsDark();
            Resources["Fg"] = new SolidColorBrush(d ? Colors.White : Color.FromRgb(0x1C, 0x1C, 0x1E));
            Resources["Fg2"] = new SolidColorBrush(d ? Color.FromRgb(0xAE, 0xAE, 0xB2) : Color.FromRgb(0x6C, 0x6C, 0x70));
            Resources["CardBg"] = new SolidColorBrush(d ? Color.FromArgb(0x40, 0x3A, 0x3A, 0x3C) : Color.FromArgb(0xB0, 0xFF, 0xFF, 0xFF));
            Resources["BtnBg"] = new SolidColorBrush(d ? Color.FromArgb(0x60, 0x63, 0x63, 0x66) : Color.FromArgb(0xC0, 0xE5, 0xE5, 0xEA));
        }
        [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr h, int a, ref int v, int s);
        [StructLayout(LayoutKind.Sequential)] struct MARGINS { public int l, r, t, b; }
        [DllImport("dwmapi.dll")] static extern int DwmExtendFrameIntoClientArea(IntPtr h, ref MARGINS m);
        void ApplyBackdrop()
        {
            var h = new WindowInteropHelper(this).Handle;
            try
            {
                int dark = IsDark() ? 1 : 0; DwmSetWindowAttribute(h, 20, ref dark, 4);
                int round = 2; DwmSetWindowAttribute(h, 33, ref round, 4);
                if (Environment.OSVersion.Version.Build >= 22000)
                {
                    HwndSource.FromHwnd(h).CompositionTarget.BackgroundColor = Colors.Transparent;
                    var m = new MARGINS { l = -1, r = -1, t = -1, b = -1 }; DwmExtendFrameIntoClientArea(h, ref m);
                    int mica = 2; DwmSetWindowAttribute(h, 38, ref mica, 4);
                    return;
                }
            }
            catch { }
            Background = new SolidColorBrush(IsDark() ? Color.FromRgb(0x1C, 0x1C, 0x1E) : Color.FromRgb(0xF2, 0xF2, 0xF7));
        }

        // ---- Ayar kaydı ----
        void LoadSaved()
        {
            if (!File.Exists(EmuExe)) { ShowStatus("emulator klasörü bulunamadı. Zip'i tamamen çıkardığından ve bu programı zip'in içinden değil, çıkardığın klasörden açtığından emin ol.", false); PickBtn.IsEnabled = false; return; }
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var p = JsonDocument.Parse(File.ReadAllText(SettingsFile)).RootElement.GetProperty("oyunKlasoru").GetString();
                    if (!string.IsNullOrEmpty(p) && Directory.Exists(p)) Validate(p);
                }
            }
            catch { }
        }
        static void Save(string p)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile)!);
            File.WriteAllText(SettingsFile, JsonSerializer.Serialize(new { oyunKlasoru = p }));
        }

        // ---- Klasör seçimi ve kontrol ----
        void Pick_Click(object s, RoutedEventArgs e)
        {
            var dlg = new OpenFolderDialog { Title = "inFAMOUS Second Son oyun klasörünü seç (içinde eboot.bin olan)" };
            if (dlg.ShowDialog(this) == true) Validate(dlg.FolderName);
        }

        void Validate(string dir)
        {
            gameDir = null; titleId = null; PlayBtn.IsEnabled = false;
            PathText.Text = dir;
            var r = GameCheck.Check(dir);
            // Kullanıcı bir üst klasörü seçtiyse içinde eboot.bin olan alt klasörü bul
            if (!r.Ok && r.Code == "noeboot")
            {
                try
                {
                    foreach (var sub in Directory.GetDirectories(dir))
                    {
                        var r2 = GameCheck.Check(sub);
                        if (r2.Ok) { dir = sub; r = r2; PathText.Text = dir; break; }
                    }
                }
                catch { }
            }
            if (!r.Ok) { ShowStatus(r.Message, false); return; }
            bool known = Array.IndexOf(KnownIds, r.TitleId) >= 0 || (r.Title ?? "").Contains("Second Son", StringComparison.OrdinalIgnoreCase);
            if (!known)
            {
                ShowStatus($"Bu klasördeki oyun inFAMOUS Second Son değil gibi görünüyor: {r.Title} ({r.TitleId}). Bu paket yalnızca Second Son için ayarlı.", false);
                return;
            }
            gameDir = dir; titleId = r.TitleId;
            ShowStatus($"Tamam: {r.Title} ({r.TitleId}, sürüm {r.Version}). Oyna'ya basabilirsin.", true);
            PlayBtn.IsEnabled = true;
            try { Save(dir); } catch { }
        }

        void ShowStatus(string msg, bool ok)
        {
            StatusText.Visibility = Visibility.Visible;
            StatusText.Text = msg;
            StatusText.Foreground = new SolidColorBrush(ok ? Color.FromRgb(0x30, 0xD1, 0x58) : Color.FromRgb(0xFF, 0x45, 0x3A));
        }

        // ---- Oyna ----
        void Play_Click(object s, RoutedEventArgs e)
        {
            if (gameDir == null || titleId == null) return;
            try
            {
                // Bölge farklıysa Kutay'ın CUSA00004 ayarını o kimliğe kopyala
                var cc = Path.Combine(EmuDir, "user", "custom_configs");
                var target = Path.Combine(cc, titleId + ".json");
                var src = Path.Combine(cc, "CUSA00004.json");
                if (!File.Exists(target) && File.Exists(src)) File.Copy(src, target);

                var psi = new ProcessStartInfo(EmuExe) { WorkingDirectory = EmuDir, UseShellExecute = false };
                psi.ArgumentList.Add("--game");
                psi.ArgumentList.Add(gameDir);
                Process.Start(psi);
                WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {
                ShowStatus("Emülatör başlatılamadı: " + ex.Message + " (Microsoft Visual C++ 2015-2022 x64 paketini kurmayı dene.)", false);
            }
        }
    }

    public record CheckResult(bool Ok, string Code, string Message, string? TitleId = null, string? Title = null, string? Version = null);

    public static class GameCheck
    {
        public static CheckResult Check(string dir)
        {
            if (!Directory.Exists(dir)) return new(false, "nodir", "Klasör bulunamadı.");
            if (!File.Exists(Path.Combine(dir, "eboot.bin")))
            {
                if (Directory.GetFiles(dir, "*.pkg").Length > 0)
                    return new(false, "pkg", "Bu klasörde .pkg dosyası var ama oyun açılmamış. Önce pkg'yi çıkarman gerekiyor; seçilecek klasörün içinde eboot.bin ve sce_sys olmalı.");
                return new(false, "noeboot", "Bu klasörde eboot.bin yok. Oyunun ana klasörünü seç (içinde eboot.bin ve sce_sys klasörü olan).");
            }
            var sfo = Path.Combine(dir, "sce_sys", "param.sfo");
            if (!File.Exists(sfo)) return new(false, "nosfo", "eboot.bin var ama sce_sys\\param.sfo yok. Oyun eksik kopyalanmış olabilir, sce_sys klasörüyle birlikte kopyala.");
            try
            {
                var p = ParamSfo.Read(File.ReadAllBytes(sfo));
                p.TryGetValue("TITLE_ID", out var id); p.TryGetValue("TITLE", out var t);
                p.TryGetValue("APP_VER", out var v); if (string.IsNullOrEmpty(v)) p.TryGetValue("VERSION", out v);
                if (string.IsNullOrEmpty(id)) return new(false, "badsfo", "param.sfo okunamadı (oyun kimliği yok).");
                return new(true, "ok", "", id, t ?? "?", v ?? "?");
            }
            catch { return new(false, "badsfo", "param.sfo bozuk ya da okunamadı."); }
        }
    }

    public static class ParamSfo
    {
        public static System.Collections.Generic.Dictionary<string, string> Read(byte[] b)
        {
            var d = new System.Collections.Generic.Dictionary<string, string>();
            if (b.Length < 20 || BitConverter.ToUInt32(b, 0) != 0x46535000) throw new InvalidDataException();
            int keyT = BitConverter.ToInt32(b, 8), dataT = BitConverter.ToInt32(b, 12), n = BitConverter.ToInt32(b, 16);
            for (int i = 0; i < n; i++)
            {
                int e = 20 + i * 16;
                int ko = BitConverter.ToUInt16(b, e), fmt = BitConverter.ToUInt16(b, e + 2);
                int len = BitConverter.ToInt32(b, e + 4), off = BitConverter.ToInt32(b, e + 12);
                int ks = keyT + ko, ke = Array.IndexOf(b, (byte)0, ks);
                string key = Encoding.ASCII.GetString(b, ks, ke - ks);
                int ds = dataT + off;
                string val = fmt == 0x0404 ? BitConverter.ToUInt32(b, ds).ToString() : Encoding.UTF8.GetString(b, ds, len).TrimEnd('\0');
                d[key] = val;
            }
            return d;
        }
    }
}
