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

        public WGrid(XElement xml) : base(xml)
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

        protected override void ProcessXmlProperty(string name, string value)
        {
            // TODO handle get
            switch (name)
            {
                case "topleft": topleft.Source = ElementStore.GetBitmap(value); break;
                case "top": top.Source = ElementStore.GetBitmap(value); break;
                case "topright": topright.Source = ElementStore.GetBitmap(value); break;
                case "left": left.Source = ElementStore.GetBitmap(value); break;
                case "middle": middle.Source = ElementStore.GetBitmap(value); break;
                case "right": right.Source = ElementStore.GetBitmap(value); break;
                case "bottomleft": bottomleft.Source = ElementStore.GetBitmap(value); break;
                case "bottom": bottom.Source = ElementStore.GetBitmap(value); break;
                case "bottomright": bottomright.Source = ElementStore.GetBitmap(value); break;
            }
            base.ProcessXmlProperty(name, value);
        }
    }
}
