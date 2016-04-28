using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Whip.Runtime;
using Whip.XmlAdapter;

namespace Whip.Widgets
{
    partial class GuiObject
    {
        private readonly IDictionary<string, string> props =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        readonly static protected IXmlDpAdapter
            byteToDouble01 = new ByteTo01Adapter(),
            simpleInt = new IntegerAdapter(),
            bool2vis = new BooleanVisibilityAdapter();

        public string Xml
        {
            get;
            private set;
        }

        public string GetXmlProperty(string name)
        {
            string result;
            props.TryGetValue(name, out result);
            return result ?? string.Empty;
        }

        public void SetXmlProperty(string name, string value)
        {
            ProcessXmlProperty(name, props[name] = value);
        }

        /// <summary>
        /// Get or set specified property string
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Property value (null to get, non-null to set)</param>
        protected virtual void ProcessXmlProperty(string name, string value)
        {
            switch (name.ToLower())
            {
                case "alpha":
                    SetValue(OpacityProperty, int.Parse(value) / 255.0);
                    break;
                case "visible":
                    SetValue(VisibilityProperty, int.Parse(value) == 1 ? Visibility.Visible : Visibility.Hidden);
                    break;
                case "x":
                    SetValue(RelatPanel.XProperty, int.Parse(value));
                    break;
                case "y":
                    SetValue(RelatPanel.YProperty, int.Parse(value));
                    break;
                case "w":
                    SetValue(RelatPanel.WProperty, int.Parse(value));
                    break;
                case "h":
                    SetValue(RelatPanel.HProperty, int.Parse(value));
                    break;
                case "relatx":
                    SetValue(RelatPanel.RelatXProperty, int.Parse(value));
                    break;
                case "relaty":
                    SetValue(RelatPanel.RelatYProperty, int.Parse(value));
                    break;
                case "relatw":
                    SetValue(RelatPanel.RelatWProperty, int.Parse(value));
                    break;
                case "relath":
                    SetValue(RelatPanel.RelatHProperty, int.Parse(value));
                    break;
                case "fitparent":
                    SetValue(RelatPanel.FitParentProperty, int.Parse(value));
                    break;
                case "id":
                    Id = value;
                    break;
            }
        }

        public static GuiObject FromXml(XElement xml, ElementStore store)
        {
            var type = xml.Name.LocalName;
            var xui = default(XElement);
            switch (type.ToLower(CultureInfo.InvariantCulture))
            {
                case "group":
                case "layout":
                    return new Group(xml);
                case "grid":
                    return new WGrid(xml);
                case "wasabiframe":
                    return new Frame(xml);
                case "button":
                    return new WButton(xml);
                case "layer":
                    return new Layer(xml);
                case "text":
                    return new WText(xml);
                default:
                    if ((xui = store.GetGroupDef(type)) != null)
                    {
                        return new Group(xml);
                    }
                    else
                    {
                        return new ImageTest(xml);
                    }
            }
        }
    }
}
