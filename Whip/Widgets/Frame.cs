using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Whip.Widgets
{
    class Frame : GuiObject
    {
        Grid grid = new Grid();

        public Frame(XElement xml) : base(xml)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            Content = grid;
        }

        protected override void ProcessXmlProperty(string name, ref string value)
        {
            switch (name)
            {
                case "left":
                case "right":
                    var content = FromXml(ElementStore.GetGroupDef(value), ElementStore);
                    Grid.SetColumn(content, name == "left" ? 0 : 1);
                    break;
            }
            base.ProcessXmlProperty(name, ref value);
        }
    }
}
