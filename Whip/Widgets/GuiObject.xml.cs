using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        protected virtual void ProcessXmlProperty(string name, ref string value)
        {
            var adapter = default(IXmlDpAdapter);
            var dp = default(DependencyProperty);
            switch (name)
            {
                case "alpha":
                    adapter = byteToDouble01;
                    dp = OpacityProperty;
                    break;
                case "visible":
                    adapter = bool2vis;
                    dp = VisibilityProperty;
                    break;
                case "x":
                    adapter = simpleInt;
                    dp = RelatPanel.XProperty;
                    break;
                case "y":
                    adapter = simpleInt;
                    dp = RelatPanel.YProperty;
                    break;
                case "w":
                    adapter = simpleInt;
                    dp = RelatPanel.WProperty;
                    break;
                case "h":
                    adapter = simpleInt;
                    dp = RelatPanel.HProperty;
                    break;
                case "relatx":
                    adapter = simpleInt;
                    dp = RelatPanel.RelatXProperty;
                    break;
                case "relaty":
                    adapter = simpleInt;
                    dp = RelatPanel.RelatYProperty;
                    break;
                case "relatw":
                    adapter = simpleInt;
                    dp = RelatPanel.RelatWProperty;
                    break;
                case "relath":
                    adapter = simpleInt;
                    dp = RelatPanel.RelatHProperty;
                    break;
                case "fitparent":
                    adapter = simpleInt;
                    dp = RelatPanel.FitParentProperty;
                    break;
            }
            if (value == null)
            {
                value = adapter?.GetDp(dp, this);
            }
            else
            {
                adapter?.SetDp(dp, this, value);
            }
        }

        public static GuiObject FromXml(XElement xml, ElementStore store)
        {
            var type = xml.Name.LocalName;
            var xui = default(XElement);
            switch (type.ToLower())
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
