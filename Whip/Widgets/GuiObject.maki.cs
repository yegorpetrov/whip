using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Whip.Widgets
{
    [DebuggerDisplay("{Id}")]
    partial class GuiObject
    {
        public string Id
        {
            get;
            set;
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }

        public GuiObject FindObject(string id)
        {
            return
                FindVisualChildren<GuiObject>(this)
                .Where(g => g.Id == id)
                .FirstOrDefault();
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject d) where T : DependencyObject
        {
            if (d != null)
            {
                for (int i = 0, _i = VisualTreeHelper.GetChildrenCount(d); i < _i; i++)
                {
                    var child = VisualTreeHelper.GetChild(d, i);
                    if (child != null && child is T)
                    {
                        yield return child as T;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
