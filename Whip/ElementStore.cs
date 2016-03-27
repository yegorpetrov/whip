﻿using System;
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

        public XDocument Root
        {
            get { return xml; }
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
                var entry = xml.Descendants("bitmap")
                    .FirstOrDefault(b => b.Attribute("id").Value == id);
                if (entry != null)
                {
                    bitmaps[id] = result = LoadBitmap(entry);
                }
            }
            return result;
        }

        private BitmapSource LoadBitmap(XElement entry)
        {
            var path = Path.Combine(root, entry.Attribute("file").Value);
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