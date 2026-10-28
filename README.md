DuckuIco - تبدیل عکس به ICO (Visual Studio 2019 / C# / WinForms)

ساخت:
1) DuckuIco.sln را در Visual Studio 2019 باز کنید.
2) حالت Release > Build Solution
3) خروجی: DuckuIco\bin\Release\X2Ico.exe  (فقط همین یک فایل را کپی کنید)

نیازمندی: Windows 7 SP1 تا 11 با .NET Framework 4.7.2 یا بالاتر.

طرز کار:
- عکس (یا چند عکس، یا یک پوشه) را داخل پنجره بکشید؛ کنار هر عکس یک پوشه هم‌نام ساخته می‌شود
  و فایل .ico داخلش قرار می‌گیرد.  مثال:  C:\pics\logo.png  ->  C:\pics\logo\logo.ico
- عکس را روی آیکون DuckuIco.exe هم می‌توانید رها کنید.
- سایزهای داخل ICO: 16, 24, 32, 48, 64, 128, 256 (قابل انتخاب).
- فرمت‌های ورودی: PNG, JPG, BMP, GIF, TIFF.

فایل‌ها:
DuckuIco.sln
DuckuIco\X2Ico.csproj
DuckuIco\app.manifest        (بدون ادمین تا درگ‌اندراپ کار کند)
DuckuIco\Program.cs
DuckuIco\MainForm.cs         (رابط کاربری و درگ‌اندراپ)
DuckuIco\DropPanel.cs        (کادر خط‌چین)
DuckuIco\IcoWriter.cs        (ساخت ICO چندسایزه)
DuckuIco\Properties\AssemblyInfo.cs
