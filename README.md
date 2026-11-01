# inFAMOUS Second Son Başlatıcı

inFAMOUS Second Son'u PC'de shadPS4 ile açmanın en kolay yolu. Zip'i indirip çıkarıyorsun,
başlatıcıyı açıyorsun, oyun klasörünü bir kez seçiyorsun ve Oyna'ya basıyorsun. Ayarlarla
uğraşmana gerek yok, hepsi hazır.

İçinde benim kendi bilgisayarımda kullandığım kurulum var: AxylPup'ın Second Son için
hazırladığı shadPS4 1.1.1, FPS sorununu çözen ayar (present mode Immediate), klavye ve fare
tuşlarım ve Paper Trail bölümünün kilidini açan küçük yama.

## Kurulum

1. [Sürümler](../../releases/latest) sayfasından `inFAMOUS-SecondSon-Kutay.zip` dosyasını indir.
2. Zip'i istediğin bir klasöre çıkar (zip'in içinden çalıştırma).
3. `inFAMOUS Baslatici.exe` dosyasını aç, "Klasör seç" ile oyun klasörünü göster.
   Doğru klasör, içinde `eboot.bin` ve `sce_sys` olan klasördür. Başlatıcı oyunu tanır ve
   seçimini hatırlar.
4. Oyna'ya bas.

Oyunun kendisi bu pakette yok. Kendi PS4 kopyandan alınmış, açılmış (pkg'den çıkarılmış)
oyun klasörünü kullanman gerekiyor.

## Gerekenler

- Windows 10 veya 11, 64 bit
- Vulkan destekleyen bir ekran kartı ve güncel sürücü
- Emülatör açılmazsa: [Microsoft Visual C++ 2015-2022 x64](https://aka.ms/vs/17/release/vc_redist.x64.exe)
- Oyun açılış ekranında kalırsa kendi PS4 firmware'inden `libSceLibcInternal.sprx` ve
  `libSceRtc.sprx` dosyalarını `emulator\user\sys_modules` klasörüne koy. Firmware
  dosyaları bu pakette yoktur ve olmayacak.

## Tuşlar

| Tuş | Oyunda |
|---|---|
| W A S D | Hareket |
| Fare | Kamera |
| Sol tık / sağ tık | Ateş (R2) / nişan (L2) |
| Boşluk | Zıpla (X) |
| Sol Shift | Kaçış (O) |
| C / F | Kare / üçgen |
| Q / E | L1 / R1 |
| R / G | Dokunmatik yüzey sol / sağ |
| Enter | Menü |
| Y | Fare ile kamera arasında geçiş |
| F11 / F9 | Tam ekran / duraklat |
| Ctrl + Shift + End | Emülatörü kapat |

Kol takılıysa doğrudan kolla da oynayabilirsin. Tuşları değiştirmek istersen
`emulator\user\input_config\default.ini` dosyasını düzenle.

## Sorun çıkarsa

- Başlatıcı "eboot.bin yok" diyorsa yanlış klasörü seçmişsindir; bir alt klasöre in.
- AMD kartta shader hazırlanırken oyun kapanıyorsa `emulator\shadps4-flags-user.txt`
  adında bir dosya aç ve içine şu iki satırı yaz:
  `SHADPS4_NATIVE_COLOUR=0` ve `SHADPS4_NATIVE_COLOUR_UNTEXTURED=0`
- Diğer ayarlar için `emulator\README.md` (AxylPup'ın notları, İngilizce) dosyasına bak.

## Lisans ve teşekkür

- Emülatör: [shadPS4](https://github.com/shadps4-emu/shadPS4) ekibi ve
  [AxylPup](https://github.com/axylpup/shadPS4/tree/second-son-v1.1.1). GPL-2.0-or-later.
  Kaynak kodu yukarıdaki bağlantıda. Benim exe'ye yaptığım değişiklikler `YAMA-NOTLARI.txt`
  dosyasında bayt bayt yazılı.
- Başlatıcı (`launcher/`): MIT, Kutay Kılıç.

## English

A one-click launcher for inFAMOUS Second Son on shadPS4 (AxylPup's 1.1.1 fork) with my
settings. Download the release zip, extract, run `inFAMOUS Baslatici.exe`, pick your own
dumped game folder (the one with `eboot.bin` and `sce_sys`) and press Oyna. No game files,
firmware or keys are included. The emulator is GPL-2.0-or-later (source linked above, binary
patch documented in `YAMA-NOTLARI.txt`); the launcher is MIT.
