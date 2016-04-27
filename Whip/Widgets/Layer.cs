using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Whip.Widgets
{
    class Layer : GuiObject
    {
        readonly Image img;

        public Layer(XElement xml) : base(xml)
        {
            Content = img = new Image() { Stretch = System.Windows.Media.Stretch.Fill };
        }

        protected override void ProcessXmlProperty(string name, string value)
        {
            base.ProcessXmlProperty(name, value);
            switch (name)
            {
                case "image":
                    img.Source = ElementStore.GetBitmap(value);
                    break;
            }
        }
    }
}
