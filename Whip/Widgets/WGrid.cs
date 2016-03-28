using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace Whip.Widgets
{
    class WGrid : GuiObject
    {
        readonly Image
            topleft = new Image(),
            top = new Image() { Stretch = Stretch.Fill },
            topright = new Image(),
            left = new Image() { Stretch = Stretch.Fill },
            middle = new Image() { Stretch = Stretch.Fill },
            right = new Image() { Stretch = Stretch.Fill },
            bottomleft = new Image(),
            bottom = new Image() { Stretch = Stretch.Fill },
            bottomright = new Image();

        public WGrid()
        {
            var grid = new Grid();
            foreach (var row in new[]
                {
                    new RowDefinition() { Height = GridLength.Auto },
                    new RowDefinition(),
                    new RowDefinition() { Height = GridLength.Auto }
                })
            {
                grid.RowDefinitions.Add(row);
            }
            
            foreach (var col in new[]
                {
                    new ColumnDefinition() { Width = GridLength.Auto },
                    new ColumnDefinition(),
                    new ColumnDefinition() { Width = GridLength.Auto }
                })
            {
                grid.ColumnDefinitions.Add(col);
            }
            
            var images = new[]
            {
                new[] { topleft, top, topright },
                new[] { left, middle, right },
                new[] { bottomleft, bottom, bottomright }
            };
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    var i = images[row][col];
                    grid.Children.Add(i);
                    Grid.SetRow(i, row);
                    Grid.SetColumn(i, col);
                }
            }
            Content = grid;

        }

        public static new WGrid FromXml(XElement xml, ElementStore store)
        {
            Action<Image, string> setBitmap = (i, b) =>
            {
                if (xml.Attribute(b) != null)
                {
                    i.Source = store.GetBitmap(xml.Attribute(b).Value);
                }
            };
            var grid = new WGrid();
            setBitmap(grid.topleft,     "topleft");
            setBitmap(grid.top,         "top");
            setBitmap(grid.topright,    "topright");
            setBitmap(grid.left,        "left");
            setBitmap(grid.middle,      "middle");
            setBitmap(grid.right,       "right");
            setBitmap(grid.bottomleft,  "bottomleft");
            setBitmap(grid.bottom,      "bottom");
            setBitmap(grid.bottomright, "bottomright");
            return grid;

        }
    }
}
