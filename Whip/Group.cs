using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Whip
{
    class Group : GuiObject
    {
        readonly RelatPanel panel = new RelatPanel();

        public Group()
        {
            Content = panel;
        }

        public static new Group FromXml(XElement xml, ElementStore store)
        {
            var xdef = xml.Name.LocalName == "layout" ?
                xml :
                store
                    .Root
                    .Elements()
                    .First()
                    .Elements("groupdef")
                    .Where(d => d.Attribute("id").Value == xml.Attribute("id").Value)
                    .FirstOrDefault();

            if (xdef == null) return null;

            var group = new Group();
            foreach (var e in xdef
                .Elements()
                .Select(x => GuiObject.FromXml(x, store))
                .Where(g => g != null))
            {
                group.panel.Children.Add(e);
            }
            return group;
        }
    }
}
