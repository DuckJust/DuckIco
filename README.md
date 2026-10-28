# 🦆 DuckuIco

<p align="center">
  <img src="logo.png" alt="DuckuIco Logo" width="250"/>
</p>

<p align="center">
  <b>تبدیل سریع و آسان عکس به آیکون (ICO) بدون نیاز به نرم‌افزارهای جانبی</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#"/>
  <img src="https://img.shields.io/badge/.NET_Framework-4.7.2+-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET"/>
  <img src="https://img.shields.io/badge/Windows-7_SP1_to_11-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Windows"/>
</p>

---

## 📌 درباره پروژه
**DuckuIco** یک برنامه سبک، سریع و کاربرپسند (WinForms) است که به شما امکان می‌دهد تصاویر خود را با کشیدن و رها کردن (Drag & Drop)، به فایل‌های آیکون چندسایزه (`.ico`) تبدیل کنید. این برنامه به هیچ کتابخانه خارجی نیاز ندارد و تمام فرآیند ساخت فرمت ICO را به صورت داخلی مدیریت می‌کند.

## ✨ امکانات
* 🚀 **پشتیبانی از درگ و دراپ (Drag & Drop):** عکس‌ها یا پوشه‌های حاوی عکس را مستقیماً داخل برنامه رها کنید.
* 📐 **خروجی چندسایزه (Multi-Resolution):** ساخت آیکون با سایزهای استاندارد (16, 24, 32, 48, 64, 128, 256) به صورت یکجا و با بهترین کیفیت.
* 📁 **مدیریت هوشمند خروجی:** ذخیره خودکار فایل‌های نهایی در پوشه اختصاصی `DuckIco` (با قابلیت ساخت زیرپوشه برای هر آیکون).
* 🖼️ **پشتیبانی از فرمت‌های متنوع:** ورودی‌های PNG, JPG, JPEG, BMP, GIF, TIFF.
* 🎨 **رابط کاربری تیره (Dark Mode):** طراحی مدرن، ساده و چشم‌نواز.

---

## 🛠️ راهنمای ساخت (Build)
برای کامپایل کردن پروژه، مراحل زیر را دنبال کنید:

1. فایل `DuckuIco.sln` را در **Visual Studio 2019** (یا نسخه‌های بالاتر) باز کنید.
2. حالت خروجی را روی **Release** قرار دهید.
3. از منوی بالا گزینه **Build Solution** (یا کلیدهای `Ctrl+Shift+B`) را بزنید.
4. فایل اجرایی نهایی در مسیر زیر ساخته می‌شود:
   ```text
   DuckuIco\bin\Release\X2Ico.exe