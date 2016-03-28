using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Whip.XmlAdapter;

namespace Whip.Widgets
{
    abstract class GuiObject : ContentControl, IXmlConfigurable
    {
        readonly static protected IXmlDpAdapter
            byteToDouble01 = new ByteTo01Adapter(),
            simpleInt = new IntegerAdapter(),
            bool2vis = new BooleanVisibilityAdapter();

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
            var result = default(GuiObject);
            switch (type.ToLower())
            {
                case "group":
                    result = Group.FromGroupdef(
                        store.GetGroupDef(xml.Attribute("id").Value), store);
                    break;
                case "layout":
                    result = Group.FromGroupdef(xml, store);
                    break;
                case "grid":
                    result = WGrid.FromXml(xml, store);
                    break;
                default:
                    if ((xui = store.GetGroupDef(type)) != null)
                    {
                        result = Group.FromGroupdef(
                        store.GetGroupDef(type), store);
                    }
                    else
                    {
                        result = ImageTest.FromXml(xml, store);
                    }
                    break;
            }
            if (result != null)
            {
                foreach (var a in xml.Attributes())
                {
                    result.SetXmlProperty(a.Name.LocalName, a.Value);
                }
            }
            return result;
        }
    }
}
