using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Widgets
{
    partial class Group
    {
        public GuiObject GetObject(string id) => panel.Children
            .OfType<GuiObject>()
            .Where(go => go.Id == id)
            .FirstOrDefault();

        public new GuiObject FindObject(string id) => GetObject(id) ??
            panel
            .Children
            .OfType<Group>()
            .Select(g => g.FindObject(id))
            .FirstOrDefault(g => g != null);

        public int GetNumObjects() => panel.Children
            .OfType<GuiObject>()
            .Count();

        public GuiObject EnumObject(int i) => panel.Children
            .OfType<GuiObject>()
            .ElementAt(i);
    }
}
