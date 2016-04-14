using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Whip.Widgets
{
    abstract partial class GuiObject : ContentControl, IXmlConfigurable
    {
        public GuiObject(XElement xml)
        {
            DataContextChanged += (s, e) =>
            {
                foreach (var a in xml.Attributes())
                {
                    SetXmlProperty(a.Name.LocalName, a.Value);
                }
            };
            Xml = xml.ToString();
        }

        protected ElementStore ElementStore
        {
            get { return DataContext as ElementStore; }
        }
    }
}
