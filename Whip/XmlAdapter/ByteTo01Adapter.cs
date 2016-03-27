using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Whip.XmlAdapter
{
    class ByteTo01Adapter : IXmlDpAdapter
    {
        public string GetDp(DependencyProperty dp, DependencyObject target)
        {
            return ((int)Math.Round((double)target.GetValue(dp) * 255)).ToString();
        }

        public void SetDp(DependencyProperty dp, DependencyObject target, string xmlPropValue)
        {
            target.SetValue(dp, byte.Parse(xmlPropValue) / 255.0);
        }
    }
}
