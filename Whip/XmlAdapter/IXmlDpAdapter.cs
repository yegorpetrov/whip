using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Whip.XmlAdapter
{
    interface IXmlDpAdapter
    {
        void SetDp(DependencyProperty dp, DependencyObject target, string xmlPropValue);
        string GetDp(DependencyProperty dp, DependencyObject target);
    }
}
