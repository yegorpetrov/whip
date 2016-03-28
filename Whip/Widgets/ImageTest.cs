using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Whip.Widgets
{
    class ImageTest : GuiObject
    {
        public ImageTest(XElement xml) : base(xml)
        {

        }

        protected override void ProcessXmlProperty(string name, ref string value)
        {
            if (name == "image")
            {
                Content = new Image()
                {
                    Source = ElementStore.GetBitmap(value),
                    Stretch = Stretch.Fill
                };
                Name = new string(value.Where(char.IsLetter).ToArray());
            }
            base.ProcessXmlProperty(name, ref value);
        }
    }
}
