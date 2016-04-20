using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Whip.Runtime;

namespace Whip.Widgets
{
    class Container : Window, IXmlConfigurable
    {
        public static Container FromXml(XElement x, ElementStore store)
        {
            var result = new Container()
            {
                DataContext = new ElementStore(x, store.Root) { Next = store }
            };
            foreach (var a in x.Attributes())
            {
                result.SetXmlProperty(a.Name.LocalName, a.Value);
            }
            result.Content = GuiObject.FromXml(x.Element("layout"), store);
            return result;
        }

        public Container()
        {
            Background = Brushes.Transparent;
        }

        public string GetXmlProperty(string name)
        {
            var result = default(string);
            ProcessXmlProperty(name, ref result);
            return result;
        }

        public void SetXmlProperty(string name, string value)
        {
            ProcessXmlProperty(name, ref value);
        }

        /// <summary>
        /// Get or set specified property string
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Property value (null to get, non-null to set)</param>
        private void ProcessXmlProperty(string name, ref string value)
        {
            var get = value == null;
            switch (name)
            {
                case "id": value = Name = value ?? Name; break;
                case "name": value = Title = value ?? Title; break;
                case "default_x":
                case "default_y":
                case "default_w":
                case "default_h":
                    {
                        var dp = default(DependencyProperty);
                        switch (name.Last())
                        {
                            case 'x': dp = LeftProperty; break;
                            case 'y': dp = TopProperty; break;
                            case 'w': dp = WidthProperty; break;
                            case 'h': dp = HeightProperty; break;
                        }
                        if (get) value = Math.Round((double)GetValue(dp)).ToString(CultureInfo.InvariantCulture);
                        else SetValue(dp, double.Parse(value, CultureInfo.InvariantCulture));
                    }
                    break;
            }
        }
    }
}
