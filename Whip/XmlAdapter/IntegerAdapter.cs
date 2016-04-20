using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Whip.XmlAdapter
{
    class IntegerAdapter : IXmlDpAdapter
    {
        public string GetDp(DependencyProperty dp, DependencyObject target)
        {
            return ((int)target.GetValue(dp)).ToString(CultureInfo.InvariantCulture);
        }

        public void SetDp(DependencyProperty dp, DependencyObject target, string xmlPropValue)
        {
            target.SetValue(dp, int.Parse(xmlPropValue, CultureInfo.InvariantCulture));
        }
    }
}
