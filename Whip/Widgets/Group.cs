﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using WhipMaki;

namespace Whip.Widgets
{
    partial class Group : GuiObject
    {
        readonly RelatPanel panel = new RelatPanel();
        readonly bool isGroupDef; // layouts are groupdefs too
        
        public Group(XElement xml) : base(xml)
        {
            var type = xml.Name.LocalName;
            switch(type.ToLower(CultureInfo.InvariantCulture))
            {
                case "layout":
                case "groupdef":
                    isGroupDef = true;
                    DataContextChanged += (s, e) =>
                    {
                        InstantiateGroupDef(xml);
                    };
                    break;
                case "group":
                    break;
                default:
                    isGroupDef = true;
                    DataContextChanged += (s, e) =>
                    {
                        InstantiateGroupDef(ElementStore.GetGroupDef(type));
                    };
                    break;
            }
            Content = panel;
        }

        protected override void ProcessXmlProperty(string name, string value)
        {
            base.ProcessXmlProperty(name, value);
            switch (name)
            {
                case "id":
                    if (!isGroupDef)
                    {
                        panel.Children.Clear();
                        InstantiateGroupDef(ElementStore.GetGroupDef(value));
                    }
                    break;
                case "instanceid":
                    InstanceId = value;
                    break;
            }
        }

        protected void InstantiateGroupDef(XElement groupdef)
        {
            if (groupdef == null) return;
            foreach (var go in groupdef.Elements().Select(x => FromXml(x, ElementStore)))
            {
                panel.Children.Add(go);
            }
            foreach (var xscript in groupdef.Elements("script"))
            {
                var file = xscript.Attribute("file")?.Value ?? string.Empty;
                var param = xscript.Attribute("param")?.Value ?? string.Empty;
                Debug.WriteLine("Loading script " + file);
                var script = ElementStore.System.LoadScript(ElementStore.FindScript(file), this, param, file);
            }
        }        
    }
}
