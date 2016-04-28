using System.Linq;

namespace Whip.Widgets
{
    partial class Group
    {
        public string InstanceId
        {
            get;
            set;
        }

        public GuiObject GetObject(string id) => panel.Children
            .OfType<GuiObject>()
            .Where(go => go.Id == id || (go as Group)?.InstanceId == id)
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
