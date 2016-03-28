using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Whip.Widgets
{
    class Group : GuiObject
    {
        readonly RelatPanel panel = new RelatPanel();
        readonly bool isLayout;
        string groupId;
        
        public Group(XElement xml) : base(xml)
        {
            Content = panel;
            if (xml.Name.LocalName == "layout")
            {
                isLayout = true;
                DataContextChanged += (s, e) =>
                {
                    InstantiateGroupDef(xml);
                };
            }
        }

        protected override void ProcessXmlProperty(string name, ref string value)
        {
            bool get = value == null;
            switch (name)
            {
                case "id":
                    if (get) value = groupId;
                    else
                    {
                        groupId = value;
                        if (!isLayout)
                        {
                            panel.Children.Clear();
                            InstantiateGroupDef(ElementStore.GetGroupDef(value));
                        }
                    }
                    break;
            }
            base.ProcessXmlProperty(name, ref value);
        }

        protected void InstantiateGroupDef(XElement groupdef)
        {
            if (groupdef == null) return;
            foreach (var go in groupdef.Elements().Select(x => FromXml(x, ElementStore)))
            {
                panel.Children.Add(go);
            }
        }
    }
}
