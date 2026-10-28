using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DuckIco
{
    /// <summary>کادر خط‌چین برای رها کردن فایل‌ها.</summary>
    public class DropPanel : Panel
    {
        bool hover;
        public string Caption { get; set; }

        public DropPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AllowDrop = true;
            Caption = "";
        }

        public bool Hover
        {
            get { return hover; }
            set { if (hover != value) { hover = value; Invalidate(); } }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : BackColor);

            Rectangle r = new Rectangle(8, 8, Width - 17, Height - 17);
            Color fill = hover ? Color.FromArgb(30, 64, 120) : Color.FromArgb(30, 41, 59);
            Color line = hover ? Color.FromArgb(96, 165, 250) : Color.FromArgb(100, 116, 139);

            using (GraphicsPath path = Round(r, 18))
            using (var b = new SolidBrush(fill))
            using (var p = new Pen(line, 2.5f) { DashStyle = DashStyle.Dash })
            {
                g.FillPath(b, path);
                g.DrawPath(p, path);
            }

            // آیکون ساده‌ی فلش به پایین
            int cx = Width / 2, cy = Height / 2 - 26;
            using (var p = new Pen(line, 4f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                g.DrawLine(p, cx, cy - 22, cx, cy + 16);
                g.DrawLines(p, new[] { new Point(cx - 16, cy), new Point(cx, cy + 18), new Point(cx + 16, cy) });
                g.DrawLines(p, new[] { new Point(cx - 26, cy + 14), new Point(cx - 26, cy + 30), new Point(cx + 26, cy + 30), new Point(cx + 26, cy + 14) });
            }

            using (var f = new Font("Tahoma", 12f, FontStyle.Bold))
            using (var tb = new SolidBrush(Color.FromArgb(226, 232, 240)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near })
            {
                g.DrawString(Caption, f, tb, new RectangleF(10, cy + 44, Width - 20, 60), sf);
            }
        }

        static GraphicsPath Round(Rectangle r, int radius)
        {
            int d = radius * 2;
            var gp = new GraphicsPath();
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}
