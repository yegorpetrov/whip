using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whip
{
    class WinampXmlReader : XmlReader
    {
        readonly Stack<XmlReader> includeStack = new Stack<XmlReader>();
        XmlReader current;

        public WinampXmlReader(string skin)
        {
            includeStack.Push(current = MakeReader(skin));
        }

        static XmlReader MakeReader(string path, bool fragment = false)
        {
            path = new Uri(path).LocalPath;
            XmlTextReader result;
            if (fragment)
            {
                var ctx = new XmlParserContext(null, null, null, null,
                    null, null, path, null, XmlSpace.Default);
                result = new XmlTextReader(File.OpenRead(path),
                    XmlNodeType.Element, ctx);
            }
            else
            {
                result = new XmlTextReader(path);
            }
            result.Namespaces = false;
            result.WhitespaceHandling = WhitespaceHandling.None;
            return result;
        }

        public override bool Read()
        {
            if (current.Read())
            {
                if (current.LocalName == "include")
                {
                    includeStack.Push(current);
                    current = MakeReader(
                        Path.Combine(
                            Path.GetDirectoryName(current.BaseURI),
                            current["file"]
                            ),
                        true);
                    return Read();
                }
                else return true;
            }
            else
            {
                if (includeStack.Count > 0)
                {
                    current = includeStack.Pop();
                    return current.NodeType != XmlNodeType.None;
                }
                else return false;
            }
        }

        public override string LocalName
        {
            get
            {
                // Remove fake namespaces
                return current.LocalName.Replace(":", "");
            }
        }

        #region Overrides
        public override int AttributeCount { get { return current.AttributeCount; } }
        public override string BaseURI { get { return current.BaseURI; } }
        public override int Depth { get { return current.Depth + includeStack.Count - 1; } }
        public override bool EOF { get { return current.EOF; } }
        public override bool IsEmptyElement { get { return current.IsEmptyElement; } }
        public override string NamespaceURI { get { return current.NamespaceURI; } }
        public override XmlNameTable NameTable { get { return current.NameTable; } }
        public override XmlNodeType NodeType { get { return current.NodeType; } }
        public override string Prefix { get { return current.Prefix; } }
        public override ReadState ReadState { get { return current.ReadState; } }
        public override string Value { get { return current.Value; } }
        public override string GetAttribute(int i) { return current.GetAttribute(i); }
        public override string GetAttribute(string name) { return current.GetAttribute(name); }
        public override string GetAttribute(string name, string namespaceURI) { return current.GetAttribute(name, namespaceURI); }
        public override string LookupNamespace(string prefix) { return current.LookupNamespace(prefix); }
        public override bool MoveToAttribute(string name) { return current.MoveToAttribute(name); }
        public override bool MoveToAttribute(string name, string ns) { return current.MoveToAttribute(name, ns); }
        public override bool MoveToElement() { return current.MoveToElement(); }
        public override bool MoveToFirstAttribute() { return current.MoveToFirstAttribute(); }
        public override bool MoveToNextAttribute() { return current.MoveToNextAttribute(); }
        public override bool ReadAttributeValue() { return current.ReadAttributeValue(); }
        public override void ResolveEntity() { current.ResolveEntity(); }
        #endregion
    }
}
