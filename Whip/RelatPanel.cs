using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Whip
{
    class RelatPanel : Panel
    {
        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (VisualTreeHelper.GetParent(d) as RelatPanel)?.InvalidateArrange();
        }

        #region Attached properties
        public static int GetX(DependencyObject obj)
        {
            return (int)obj.GetValue(XProperty);
        }

        public static void SetX(DependencyObject obj, int value)
        {
            obj.SetValue(XProperty, value);
        }

        public static readonly DependencyProperty XProperty =
            DependencyProperty.RegisterAttached("X", typeof(int),
                typeof(RelatPanel), new PropertyMetadata(0, OnPositionChanged));



        public static int GetY(DependencyObject obj)
        {
            return (int)obj.GetValue(YProperty);
        }

        public static void SetY(DependencyObject obj, int value)
        {
            obj.SetValue(YProperty, value);
        }

        public static readonly DependencyProperty YProperty =
            DependencyProperty.RegisterAttached("Y", typeof(int),
                typeof(RelatPanel), new PropertyMetadata(0, OnPositionChanged));



        public static int GetW(DependencyObject obj)
        {
            return (int)obj.GetValue(WProperty);
        }

        public static void SetW(DependencyObject obj, int value)
        {
            obj.SetValue(WProperty, value);
        }

        public static readonly DependencyProperty WProperty =
            DependencyProperty.RegisterAttached("W", typeof(int),
                typeof(RelatPanel), new PropertyMetadata(0, OnPositionChanged));



        public static int GetH(DependencyObject obj)
        {
            return (int)obj.GetValue(HProperty);
        }

        public static void SetH(DependencyObject obj, int value)
        {
            obj.SetValue(HProperty, value);
        }

        public static readonly DependencyProperty HProperty =
            DependencyProperty.RegisterAttached("H", typeof(int),
                typeof(RelatPanel), new PropertyMetadata(0, OnPositionChanged));



        public static int GetRelatX(DependencyObject obj)
        {
            return (int)obj.GetValue(RelatXProperty);
        }

        public static void SetRelatX(DependencyObject obj, int value)
        {
            obj.SetValue(RelatXProperty, value);
        }

        public static readonly DependencyProperty RelatXProperty =
            DependencyProperty.RegisterAttached("RelatX", typeof(int),
                typeof(RelatPanel), new PropertyMetadata(0, OnPositionChanged));



        public static int GetRelatY(DependencyObject obj)
        {
            return (int)obj.GetValue(RelatYProperty);
        }

        public static void SetRelatY(DependencyObject obj, int value)
        {
            obj.SetValue(RelatYProperty, value);
        }

        public static readonly DependencyProperty RelatYProperty =
            DependencyProperty.RegisterAttached("RelatY", typeof(int),
                typeof(RelatPanel), new PropertyMetadata(0, OnPositionChanged));



        public static int GetRelatW(DependencyObject obj)
        {
            return (int)obj.GetValue(RelatWProperty);
        }

        public static void SetRelatW(DependencyObject obj, int value)
        {
            obj.SetValue(RelatWProperty, value);
        }

        public static readonly DependencyProperty RelatWProperty =
            DependencyProperty.RegisterAttached("RelatW", typeof(int),
                typeof(RelatPanel), new PropertyMetadata(0, OnPositionChanged));



        public static int GetRelatH(DependencyObject obj)
        {
            return (int)obj.GetValue(RelatHProperty);
        }

        public static void SetRelatH(DependencyObject obj, int value)
        {
            obj.SetValue(RelatHProperty, value);
        }

        public static readonly DependencyProperty RelatHProperty =
            DependencyProperty.RegisterAttached("RelatH", typeof(int),
                typeof(RelatPanel), new PropertyMetadata(0, OnPositionChanged));
        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            var childConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (var child in InternalChildren.OfType<UIElement>())
            {
                child.Measure(childConstraint);
            }
            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Func<int, int, int, double> calcRelat = (parent, child, rmode) =>
            {
                switch (rmode)
                {
                    case 1:
                        return parent + child;
                    case 2:
                        return parent * child / 100.0;
                    default:
                        return child;
                }
            };

            foreach (var child in InternalChildren.OfType<UIElement>())
            {
                int
                    x = GetX(child),
                    y = GetY(child),
                    w = GetW(child),
                    h = GetH(child),
                    rx = GetRelatX(child),
                    ry = GetRelatY(child),
                    rw = GetRelatW(child),
                    rh = GetRelatH(child),
                    pw = (int)Math.Round(finalSize.Width),
                    ph = (int)Math.Round(finalSize.Height);

                child.Arrange(new Rect(
                    calcRelat(pw, x, rx),
                    calcRelat(ph, y, ry),
                    Math.Max(0, calcRelat(pw, w, rw)),
                    Math.Max(0, calcRelat(ph, h, rh))
                    ));

            }
            return base.ArrangeOverride(finalSize);
        }
    }
}
