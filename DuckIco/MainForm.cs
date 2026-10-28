using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuckIco
{
    public class MainForm : Form
    {
        // نام پوشه‌ی خروجی
        const string DUCK_FOLDER = "DuckIco";

        static readonly HashSet<string> Exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".jfif", ".bmp", ".gif", ".tif", ".tiff"
        };

        readonly DropPanel drop;
        readonly FlowLayoutPanel bar;
        readonly Button btnBrowse;
        readonly CheckBox[] sizeChecks;
        readonly CheckBox chkPerIconFolder;
        readonly Panel statusPanel;
        readonly Label statusLabel;
        readonly string[] startupFiles;
        string lastOutputDir;   // آخرین پوشه‌ی خروجی برای باز کردن با دابل‌کلیک
        bool busy;

        public MainForm(string[] args)
        {
            startupFiles = args ?? new string[0];

            Text = "DuckIco Image to .ico";
            RightToLeft = RightToLeft.No;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.ShowIcon = true;
            RightToLeftLayout = true;
            Font = new Font("Tahoma", 10F);
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.FromArgb(226, 232, 240);
            StartPosition = FormStartPosition.CenterScreen;
            MaximumSize = new Size(550, 450);
            MinimumSize = new Size(550, 450);
            AllowDrop = true;
            MaximizeBox = false;

            // ==========================================================
            // 1) کادر درگ (بالا)
            // ==========================================================
            drop = new DropPanel
            {
                Dock = DockStyle.Top,
                Height = 300,
                Caption = "عکس‌ها (یا پوشه) را اینجا بکشید و رها کنید\r\n" +
                          "خروجی در پوشه DuckIco ذخیره می‌شود"
            };

            // ==========================================================
            // 2) نوار اندازه‌ها، دکمه و چک‌باکس (پایین درگ)
            // ==========================================================
            bar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(10, 6, 10, 6),
                WrapContents = true,
                RightToLeft = RightToLeft.Yes
            };
            bar.Controls.Add(new Label { Text = "اندازه‌ها:", AutoSize = true, Margin = new Padding(6, 8, 6, 0) });

            sizeChecks = new CheckBox[IcoWriter.DefaultSizes.Length];
            for (int i = 0; i < sizeChecks.Length; i++)
            {
                int sz = IcoWriter.DefaultSizes[i];
                sizeChecks[i] = new CheckBox
                {
                    Text = sz.ToString(),
                    Tag = sz,
                    Checked = true,
                    AutoSize = true,
                    Margin = new Padding(6, 6, 6, 0)
                };
                bar.Controls.Add(sizeChecks[i]);
            }

            btnBrowse = MakeButton("انتخاب فایل...", Color.FromArgb(37, 99, 235));
            btnBrowse.Click += (s, e) => Browse();
            bar.Controls.Add(btnBrowse);
            bar.SetFlowBreak(sizeChecks[sizeChecks.Length - 1], true);

            chkPerIconFolder = new CheckBox
            {
                Text = "ایجاد پوشه برای هر آیکن",
                AutoSize = true,
                Checked = false,
                Margin = new Padding(8, 8, 6, 0)
            };
            bar.Controls.Add(chkPerIconFolder);

            // ==========================================================
            // 3) نوار وضعیت چسبیده به پایین (جای دیالوگ)
            // ==========================================================
            statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "آماده — عکس‌ها را رها کنید یا روی «انتخاب فایل» بزنید",
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.Transparent,
                Font = new Font("Tahoma", 9.5f),
                AutoEllipsis = true,
                Padding = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand
            };
            statusLabel.DoubleClick += (s, e) => OpenLastOutput();

            statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                BackColor = Color.FromArgb(10, 15, 30),
                Padding = new Padding(6, 0, 6, 0)
            };
            statusPanel.Controls.Add(statusLabel);

            // ==========================================================
            // ترتیب افزودن: اول لبه‌ها (Bottom و Top)، بعد Top بالایی
            // ==========================================================
            Controls.Add(statusPanel);   // پایین‌ترین
            Controls.Add(bar);           // وسط
            Controls.Add(drop);          // بالاترین

            // ---- رویدادهای درگ‌اندراپ (همان کد اصلی) ----
            foreach (Control c in new Control[] { this, drop, bar })
            {
                c.AllowDrop = true;
                c.DragEnter += OnDragEnter;
                c.DragOver += OnDragEnter;
                c.DragLeave += (s, e) => drop.Hover = false;
                c.DragDrop += OnDragDrop;
            }

            Shown += (s, e) =>
            {
                if (startupFiles.Length > 0) ProcessPaths(startupFiles);
            };
        }

        Button MakeButton(string text, Color color)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(120, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Margin = new Padding(8, 4, 8, 4),
                Padding = new Padding(8, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // ------------------------------------------------------------
        // لاگ روی نوار وضعیت (thread-safe)
        // ------------------------------------------------------------
        void SetStatus(string text, Color color)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetStatus(text, color)));
                return;
            }
            statusLabel.Text = text;
            statusLabel.ForeColor = color;
        }

        void OpenLastOutput()
        {
            if (!string.IsNullOrEmpty(lastOutputDir) && Directory.Exists(lastOutputDir))
            {
                try { Process.Start("explorer.exe", "\"" + lastOutputDir + "\""); }
                catch { }
            }
        }

        // ------------------------------------------------------------
        // درگ‌اندراپ
        // ------------------------------------------------------------
        void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                drop.Hover = true;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        void OnDragDrop(object sender, DragEventArgs e)
        {
            drop.Hover = false;
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null) ProcessPaths(files);
        }

        void Browse()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "انتخاب عکس";
                dlg.Multiselect = true;
                dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.jfif;*.bmp;*.gif;*.tif;*.tiff|All files|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK) ProcessPaths(dlg.FileNames);
            }
        }

        // ------------------------------------------------------------
        // پردازش
        // ------------------------------------------------------------
        int[] SelectedSizes()
        {
            var list = sizeChecks.Where(c => c.Checked).Select(c => (int)c.Tag).ToArray();
            return list.Length > 0 ? list : new[] { 256 };
        }

        async void ProcessPaths(IEnumerable<string> paths)
        {
            if (busy)
            {
                SetStatus("⏳ تبدیل قبلی هنوز تمام نشده؛ کمی بعد دوباره امتحان کنید",
                          Color.FromArgb(245, 158, 11));
                return;
            }
            busy = true;
            drop.Caption = "در حال تبدیل ...";
            drop.Invalidate();
            SetStatus("در حال آماده‌سازی فایل‌ها...", Color.FromArgb(96, 165, 250));

            int[] sizes = SelectedSizes();
            bool perIcon = chkPerIconFolder.Checked;

            try
            {
                var files = new List<string>();
                foreach (string p in paths)
                {
                    if (Directory.Exists(p))
                    {
                        try
                        {
                            files.AddRange(Directory.GetFiles(p)
                                .Where(f => Exts.Contains(Path.GetExtension(f)))
                                .Where(f => !string.Equals(
                                    Path.GetFileName(Path.GetDirectoryName(f)),
                                    DUCK_FOLDER, StringComparison.OrdinalIgnoreCase)));
                        }
                        catch (Exception ex)
                        {
                            SetStatus("✘ خطای خواندن پوشه: " + ex.Message,
                                      Color.FromArgb(239, 68, 68));
                        }
                    }
                    else if (File.Exists(p))
                    {
                        files.Add(p);
                    }
                }

                if (files.Count == 0)
                {
                    SetStatus("⚠ فایل تصویری معتبری پیدا نشد",
                              Color.FromArgb(245, 158, 11));
                    return;
                }

                int ok = 0, fail = 0, idx = 0;
                lastOutputDir = null;

                foreach (string f in files)
                {
                    idx++;
                    string ext = Path.GetExtension(f);
                    string name = Path.GetFileName(f);

                    if (string.Equals(ext, ".ico", StringComparison.OrdinalIgnoreCase))
                    {
                        fail++;
                        SetStatus($"⚠ ({idx}/{files.Count}) نادیده گرفته شد (قبلاً ICO است): {name}",
                                  Color.FromArgb(245, 158, 11));
                        continue;
                    }
                    if (!Exts.Contains(ext))
                    {
                        fail++;
                        SetStatus($"⚠ ({idx}/{files.Count}) فرمت پشتیبانی نمی‌شود: {name}",
                                  Color.FromArgb(245, 158, 11));
                        continue;
                    }

                    SetStatus($"⏳ ({idx}/{files.Count}) در حال تبدیل: {name}",
                              Color.FromArgb(96, 165, 250));

                    string srcFile = f;
                    try
                    {
                        string ico = await Task.Run(() => MakeIco(srcFile, sizes, perIcon));
                        lastOutputDir = Path.GetDirectoryName(ico);
                        ok++;
                    }
                    catch (Exception ex)
                    {
                        fail++;
                        SetStatus($"✘ ({idx}/{files.Count}) خطا در {name}: {ex.Message}",
                                  Color.FromArgb(239, 68, 68));
                    }
                }

                // --- خلاصه‌ی نهایی روی نوار وضعیت (بدون دیالوگ) ---
                if (ok > 0 && fail == 0)
                {
                    SetStatus($"✔ همه {ok} فایل با موفقیت در پوشه {DUCK_FOLDER} ذخیره شد " +
                              "— دابل‌کلیک روی همین نوار، پوشه را باز می‌کند",
                              Color.FromArgb(16, 185, 129));
                }
                else if (ok > 0)
                {
                    SetStatus($"✔ {ok} موفق | ✘ {fail} ناموفق — خروجی در {DUCK_FOLDER} " +
                              "(دابل‌کلیک برای باز کردن پوشه)",
                              Color.FromArgb(245, 158, 11));
                }
                else
                {
                    SetStatus($"✘ هیچ فایلی تبدیل نشد ({fail} ناموفق)",
                              Color.FromArgb(239, 68, 68));
                }
            }
            finally
            {
                busy = false;
                drop.Caption = "عکس‌ها (یا پوشه) را اینجا بکشید و رها کنید\r\n" +
                               "خروجی در پوشه DuckIco ذخیره می‌شود";
                drop.Invalidate();
            }
        }

        /// <summary>
        /// فایل ico را در پوشه DuckIco (کنار خود عکس) می‌سازد.
        /// اگر perIconFolder=true → داخل DuckIco زیرپوشه‌ی هم‌نام عکس هم ساخته می‌شود.
        /// </summary>
        static string MakeIco(string imagePath, int[] sizes, bool perIconFolder)
        {
            string dir = Path.GetDirectoryName(imagePath);
            string baseName = Path.GetFileNameWithoutExtension(imagePath);

            string duckDir = Path.Combine(dir, DUCK_FOLDER);
            Directory.CreateDirectory(duckDir);

            string outDir;
            if (perIconFolder)
            {
                outDir = Path.Combine(duckDir, baseName);
                Directory.CreateDirectory(outDir);
            }
            else
            {
                outDir = duckDir;
            }

            string icoPath = Path.Combine(outDir, baseName + ".ico");
            IcoWriter.ConvertFile(imagePath, icoPath, (int[])sizes.Clone());
            return icoPath;
        }
    }
}