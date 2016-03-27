using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Whip.Widgets;

namespace Whip
{
    class SkinBootstrapper
    {
        const string root = @"e:\testdata\winamp\Winamp3\";
        const string root2 = @"c:\Program Files (x86)\Winamp\Plugins\freeform\xml\";
        static readonly ElementStore environment;

        static SkinBootstrapper()
        {
            var env = new[]
            {
                @"about\about.xml",
                @"checkbox\checkbox.xml",
                @"combobox\combobox.xml",
                @"dropdownlist\dropdownlist.xml",
                @"guiobjects.xml",
                @"historyeditbox\historyeditbox.xml",
                @"menubutton\menubutton.xml",
                @"msgbox\msgbox.xml",
                @"pathpicker\pathpicker.xml",
                @"popupmenu\popupmenu.xml",
                @"statusbar\statusbar.xml",
                @"tabsheet\tabsheet.xml",
                @"titlebox\titlebox.xml",
                @"tooltips\tooltips.xml",
                @"wasabi\wasabi.xml",
                @"winamp\cover\cover.xml",
                @"winamp\thinger\thinger.xml"
            };

            foreach (var e in env)
            {
                var path = Path.Combine(root2, e);
                var wxr = new WinampXmlReader(path);
                var es = new ElementStore(
                    XDocument.Load(wxr),
                    Path.GetDirectoryName(path))
                {
                    Next = environment
                };
                es.Preload();
                environment = es;
            }
        }
        
        public SkinBootstrapper()
        {
            var reader = new WinampXmlReader(Path.Combine(root, "skin.xml"));
            var skin = XDocument.Load(reader);

            var store = new ElementStore(skin, root)
            {
                Next = environment
            };
            store.Preload();
            
            var containers =
                skin
                .Elements()
                .First()
                .Elements("container");

            var main =
                containers
                .Where(c => c.Attribute("id").Value == "main")
                .First();

            Container.FromXml(main, store).Show();
        }
    }
}
