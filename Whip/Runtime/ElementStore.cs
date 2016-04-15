using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Whip.Runtime
{
    class ElementStore
    {
        readonly XElement xml;
        readonly IDictionary<string, BitmapSource> bitmaps
            = new Dictionary<string, BitmapSource>();
        readonly IDictionary<string, BitmapSource> files
            = new Dictionary<string, BitmapSource>();
        readonly IDictionary<string, XElement> groupdefs
            = new Dictionary<string, XElement>();
        bool preloaded;

        public ElementStore(XElement xml, string root)
        {
            this.xml = xml;
            Root = root;
            Preload();
        }

        public string Root
        {
            get;
            private set;
        }

        public ElementStore Next
        {
            get;
            set;
        }

        public System0 System
        {
            get
            {
                return System0.Instance;
            }
        }

        public byte[] FindScript(string file)
        {
            var path = Path.Combine(Root, file);
            return File.Exists(path) ?
                File.ReadAllBytes(path) : Next?.FindScript(file);
        }

        public void Preload()
        {
            if (preloaded) return;
            foreach (var entry in xml.Elements("elements").Elements("bitmap"))
            {
                bitmaps[entry.Attribute("id").Value] = LoadBitmap(entry);
            }
            foreach (var entry in xml.Elements("groupdef"))
            {
                groupdefs[entry.Attribute("id").Value] = entry;
                if (entry.Attribute("xuitag") != null)
                {
                    groupdefs[entry.Attribute("xuitag").Value.Replace(":", "")] = entry;
                }
            }
            preloaded = true;
        }

        public XElement GetGroupDef(string id)
        {
            var result = default(XElement);
            if (!groupdefs.TryGetValue(id, out result) && !preloaded)
            {
                var entry = xml.Elements("groupdef")
                    .FirstOrDefault(b => b.Attribute("id").Value == id);
                if (entry != null)
                {
                    groupdefs[id] = result = entry;
                }
            }
            return result ?? Next?.GetGroupDef(id);
        }

        public BitmapSource GetBitmap(string id)
        {
            var result = default(BitmapSource);
            if (!bitmaps.TryGetValue(id, out result) && !preloaded)
            {
                var entry = xml.Elements("elements").Elements("bitmap")
                    .FirstOrDefault(b => b.Attribute("id").Value == id);
                if (entry != null)
                {
                    bitmaps[id] = result = LoadBitmap(entry);
                }
            }
            return result ?? Next?.GetBitmap(id);
        }

        private BitmapSource LoadBitmap(XElement entry)
        {
            var path = Path.Combine(Root, entry.Attribute("file").Value);
            var file = default(BitmapSource);
            if (!files.TryGetValue(path, out file))
            {
                try
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(path);
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    files[path] = file = ConvertBitmapTo96DPI(img);
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
                catch (DirectoryNotFoundException)
                {
                    return null;
                }
            }
            if ((entry.Attribute("x") ??
                entry.Attribute("y") ??
                entry.Attribute("w") ??
                entry.Attribute("h")) == null)
            {
                return file;
            }
            else
            {
                var rect = new Int32Rect()
                {
                    X = int.Parse(entry.Attribute("x").Value),
                    Y = int.Parse(entry.Attribute("y").Value),
                    Width = int.Parse(entry.Attribute("w").Value),
                    Height = int.Parse(entry.Attribute("h").Value)
                };
                rect.Width = rect.Width + Math.Min(0, file.PixelWidth - rect.Width - rect.X);
                rect.Height = rect.Height + Math.Min(0, file.PixelHeight - rect.Height - rect.Y);
                return new CroppedBitmap(file, rect);
            }
        }

        // http://stackoverflow.com/questions/3745824/loading-image-into-imagesource-incorrect-width-and-height
        static BitmapSource ConvertBitmapTo96DPI(BitmapImage bitmapImage)
        {
            if ((int)Math.Round(bitmapImage.DpiX) == 96) return bitmapImage;
            double dpi = 96;
            int width = bitmapImage.PixelWidth;
            int height = bitmapImage.PixelHeight;

            int stride = width * bitmapImage.Format.BitsPerPixel;
            byte[] pixelData = new byte[stride * height];
            bitmapImage.CopyPixels(pixelData, stride, 0);
            
            return BitmapSource.Create(width, height, dpi, dpi, bitmapImage.Format, bitmapImage.Palette, pixelData, stride);
        }
    }
}
