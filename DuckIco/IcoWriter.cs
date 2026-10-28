using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace DuckIco
{
    /// <summary>ساخت فایل ICO چندسایزه بدون هیچ کتابخانه‌ی خارجی.</summary>
    public static class IcoWriter
    {
        public static readonly int[] DefaultSizes = { 16, 24, 32, 48, 64, 128, 256 };

        /// <summary>تصویر را می‌خواند (بدون قفل‌کردن فایل) و به ICO تبدیل می‌کند.</summary>
        public static void ConvertFile(string imagePath, string icoPath, int[] sizes)
        {
            byte[] bytes = File.ReadAllBytes(imagePath);
            using (var ms = new MemoryStream(bytes))
            using (var src = Image.FromStream(ms, true, false))
            {
                Save(src, icoPath, sizes);
            }
        }

        public static void Save(Image src, string icoPath, int[] sizes)
        {
            if (sizes == null || sizes.Length == 0) sizes = DefaultSizes;
            Array.Sort(sizes);

            var blobs = new List<byte[]>();
            foreach (int size in sizes)
            {
                using (Bitmap bmp = Resize(src, size))
                    blobs.Add(size >= 256 ? ToPng(bmp) : ToDib(bmp));
            }

            using (var fs = new FileStream(icoPath, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write((short)0);              // reserved
                bw.Write((short)1);              // type = icon
                bw.Write((short)blobs.Count);

                int offset = 6 + 16 * blobs.Count;
                for (int i = 0; i < blobs.Count; i++)
                {
                    byte dim = (byte)(sizes[i] >= 256 ? 0 : sizes[i]);
                    bw.Write(dim);               // width
                    bw.Write(dim);               // height
                    bw.Write((byte)0);           // palette
                    bw.Write((byte)0);           // reserved
                    bw.Write((short)1);          // planes
                    bw.Write((short)32);         // bpp
                    bw.Write(blobs[i].Length);   // size
                    bw.Write(offset);            // offset
                    offset += blobs[i].Length;
                }
                foreach (byte[] b in blobs) bw.Write(b);
            }
        }

        /// <summary>تصویر را بدون کشیده‌شدن، وسط یک بوم مربع شفاف قرار می‌دهد.</summary>
        static Bitmap Resize(Image src, int size)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                double scale = Math.Min((double)size / src.Width, (double)size / src.Height);
                int w = Math.Max(1, (int)Math.Round(src.Width * scale));
                int h = Math.Max(1, (int)Math.Round(src.Height * scale));
                int x = (size - w) / 2;
                int y = (size - h) / 2;

                using (var ia = new ImageAttributes())
                {
                    ia.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(src, new Rectangle(x, y, w, h), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
                }
            }
            return bmp;
        }

        static byte[] ToPng(Bitmap bmp)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        /// <summary>ساخت داده‌ی BMP/DIB 32 بیتی (سازگار با همه‌ی نسخه‌های ویندوز برای سایزهای کوچک).</summary>
        static byte[] ToDib(Bitmap bmp)
        {
            int s = bmp.Width;
            var rect = new Rectangle(0, 0, s, s);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] px = new byte[s * s * 4];
            try
            {
                for (int row = 0; row < s; row++)
                    Marshal.Copy(IntPtr.Add(data.Scan0, row * data.Stride), px, row * s * 4, s * 4);
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            int maskRow = ((s + 31) / 32) * 4;
            int maskSize = maskRow * s;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(40);                    // BITMAPINFOHEADER size
                bw.Write(s);                     // width
                bw.Write(s * 2);                 // height (XOR + AND)
                bw.Write((short)1);              // planes
                bw.Write((short)32);             // bpp
                bw.Write(0);                     // BI_RGB
                bw.Write(px.Length + maskSize);  // image size
                bw.Write(0); bw.Write(0);        // resolution
                bw.Write(0); bw.Write(0);        // colors

                for (int row = s - 1; row >= 0; row--)   // پایین به بالا
                    bw.Write(px, row * s * 4, s * 4);

                bw.Write(new byte[maskSize]);    // AND mask (شفافیت از کانال آلفا)
                return ms.ToArray();
            }
        }
    }
}
