using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Whip.Runtime
{
    class BitmapFont
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ\"@@  0123456789….:()-'!_\\/[]^&%,=$#AOA?*";
        readonly IDictionary<char, BitmapSource> glyphs = new Dictionary<char, BitmapSource>();

        public BitmapFont(BitmapSource bmp, int cw, int ch)
        {
            int x = 0, y = 0;
            foreach (var c in chars)
            {
                glyphs[c] = new CroppedBitmap(bmp, new Int32Rect(x, y, cw, ch));
                if ((x += cw) > bmp.PixelWidth)
                {
                    x = 0;
                    y += ch;
                }
            }
        }

        public void Render(DrawingContext ctx)
        {
            //ctx.DrawImage()
        }
    }
}
