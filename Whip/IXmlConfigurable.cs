using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip
{
    interface IXmlConfigurable
    {
        void SetXmlProperty(string name, string value);
        string GetXmlProperty(string name);
    }
}
