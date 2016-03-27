using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Whip
{
    class SkinBootstrapper
    {
        const string root = @"e:\testdata\winamp\Winamp Modern\";
        
        public SkinBootstrapper()
        {
            var reader = new WinampXmlReader(Path.Combine(root, "skin.xml"));
            var skin = XDocument.Load(reader);

            var store = new ElementStore(skin, root);
            store.Preload();
            var bmp = store.GetBitmap("player.main.right");

            var containers =
                skin
                .Elements()
                .First()
                .Elements("container");

            var main =
                containers
                .Where(c => c.Attribute("id").Value == "main")
                .First();

            Container.FromXml(main).Show();
        }
    }
}
