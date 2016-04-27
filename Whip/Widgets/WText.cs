using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace Whip.Widgets
{
    class WText : GuiObject
    {
        TextBlock txt;

        public WText(XElement xml) : base(xml)
        {
            Content = txt = new TextBlock() { Foreground = Brushes.White };
        }

        protected override void ProcessXmlProperty(string name, string value)
        {
            base.ProcessXmlProperty(name, value);
            switch (name)
            {
                case "default":
                    txt.Text = value;
                    break;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }
    }
}
