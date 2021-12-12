using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;

namespace Utilitarios
{
    public class FaIcon
    {
        #region Constructors

        public FaIcon(IconType type) : this(type, 22, SystemColors.WindowFrame)
        {

        }

        public FaIcon(IconType type, int size, Color color)
        {
            IconFont = null;
            Width = size;
            Height = size;
            IconChar = char.ConvertFromUtf32((int)type);
            IconBrush = new SolidBrush(color);

        }

        #endregion

        public Bitmap Icon()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                if (IconFont == null)
                {
                    SetFontSize(g);
                }

                SizeF stringSize = g.MeasureString(IconChar, IconFont, Width);
                float w = stringSize.Width;
                float h = stringSize.Height;

                float left = (Width - w) / 2;
                float top = (Height - h) / 2;

                g.DrawString(IconChar, IconFont, IconBrush, new PointF(left, top));
            }
            return bmp;
        }


        #region Private

        #region Properties & Attributes

        private int Width { get; set; }
        private int Height { get; set; }
        private string IconChar { get; set; }
        private Font IconFont { get; set; }
        private Brush IconBrush { get; set; }
        #endregion


        #region Methods

        private void SetFontSize(Graphics g)
        {
            IconFont = GetAdjustedFont(g, IconChar, Width, Height, 4, true);
        }

        private Font GetIconFont(float size)
        {
            return new Font(Fonts.Families[0], size, GraphicsUnit.Point);
        }

        private Font GetAdjustedFont(Graphics g, string graphicString, int containerWidth, int maxFontSize, int minFontSize, bool smallestOnFail)
        {
            for (double adjustedSize = maxFontSize; adjustedSize >= minFontSize; adjustedSize = adjustedSize - 0.5)
            {
                Font testFont = GetIconFont((float)adjustedSize);

                SizeF adjustedSizeNew = g.MeasureString(graphicString, testFont);
                if (containerWidth > Convert.ToInt32(adjustedSizeNew.Width))
                {
                    return testFont;
                }
            }

            return GetIconFont(smallestOnFail ? minFontSize : maxFontSize);
        }

        #endregion

        #endregion

        #region Static

        static FaIcon()
        {
            InitialiseFont();
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
           IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        /// <summary>
        /// Store the icon font in a static variable to reuse between icons
        /// </summary>
        private static readonly PrivateFontCollection Fonts = new PrivateFontCollection();

        /// <summary>
        /// Loads the icon font from the resources.
        /// </summary>
        private static void InitialiseFont()
        {
            try
            {
                unsafe
                {
                    fixed (byte* pFontData =   Properties.Resources.fontawesome_webfont)
                    {
                        uint dummy = 0;
                        Fonts.AddMemoryFont((IntPtr)pFontData, Properties.Resources.fontawesome_webfont.Length);
                        AddFontMemResourceEx((IntPtr)pFontData, (uint)Properties.Resources.fontawesome_webfont.Length, IntPtr.Zero, ref dummy);
                    }
                }
            }
            catch (Exception)
            {
                // log?
            }
        }

        #endregion

    }
}
