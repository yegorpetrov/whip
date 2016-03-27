using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Whip.XmlAdapter
{
    class BooleanVisibilityAdapter : IXmlDpAdapter
    {
        public string GetDp(DependencyProperty dp, DependencyObject target)
        {
            return (Visibility)target.GetValue(dp) == Visibility.Visible ? "1" : "0";
        }

        public void SetDp(DependencyProperty dp, DependencyObject target, string xmlPropValue)
        {
            target.SetValue(dp, xmlPropValue == "1" ? Visibility.Visible : Visibility.Collapsed);
        }
    }
}
