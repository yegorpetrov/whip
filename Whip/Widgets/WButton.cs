using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Whip.Widgets
{
    class WButton : GuiObject
    {
        BitmapSource neutral, hover, down;
        readonly Image img;

        public WButton(XElement xml) : base(xml)
        {
            Content = img = new Image();
        }

        protected override void ProcessXmlProperty(string name, string value)
        {
            switch (name.ToLower(CultureInfo.InvariantCulture))
            {
                case "image":
                    img.Source = neutral = ElementStore.GetBitmap(value);
                    break;
                case "hoverimage":
                    hover = ElementStore.GetBitmap(value);
                    break;
                case "downimage":
                    down = ElementStore.GetBitmap(value);
                    break;
                default:
                    base.ProcessXmlProperty(name, value);
                    break;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e) => img.Source = hover;
        protected override void OnMouseLeave(MouseEventArgs e) => img.Source = neutral;
        protected override void OnMouseDown(MouseButtonEventArgs e) => img.Source = down;
        protected override void OnMouseUp(MouseButtonEventArgs e) => img.Source = hover;
    }
}
