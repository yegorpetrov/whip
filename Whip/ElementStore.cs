using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Whip
{
    class ElementStore
    {
        readonly XDocument xml;
        readonly string root;
        readonly IDictionary<string, BitmapSource> bitmaps
            = new Dictionary<string, BitmapSource>();
        readonly IDictionary<string, BitmapSource> files
            = new Dictionary<string, BitmapSource>();

        public ElementStore(XDocument xml, string root)
        {
            this.xml = xml;
            this.root = root;
        }

        public void Preload()
        {
            foreach (var entry in xml.Descendants("bitmap"))
            {
                bitmaps[entry.Attribute("id").Value] = LoadBitmap(entry);
            }
        }

        public BitmapSource GetBitmap(string id)
        {
            var result = default(BitmapSource);
            if (!bitmaps.TryGetValue(id, out result))
            {
                bitmaps[id] = result = LoadBitmap(xml.Descendants("bitmap")
                    .First(b => b.Attribute("id").Value == id));
            }
            return result;
        }

        private BitmapSource LoadBitmap(XElement entry)
        {
            var path = Path.Combine(root, entry.Attribute("file").Value);
            var file = default(BitmapSource);
            if (!files.TryGetValue(path, out file))
            {
                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(path);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                files[path] = file = img;
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
                rect.Width = Math.Min(file.PixelWidth - rect.X, rect.X + rect.Width);
                rect.Height = Math.Min(file.PixelHeight - rect.Y, rect.Y + rect.Height);
                return new CroppedBitmap(file, rect);
            }
        }
    }
}
