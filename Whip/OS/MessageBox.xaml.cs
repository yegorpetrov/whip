using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Whip.OS
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        [Flags]
        public enum MsgButtons
        {
            OK, Cancel, Yes, No, All, Next, Previous
        }

        public MessageBox()
        {
            InitializeComponent();
        }

        public static MsgButtons Show(string msg, string title, MsgButtons buttons, string suppress)
        {
            var box = new MessageBox();
            box.msg.Text = msg;
            box.Title = title;
            box.ok.Visibility = buttons.HasFlag(MsgButtons.OK) ? Visibility.Visible : Visibility.Collapsed;
            box.cancel.Visibility = buttons.HasFlag(MsgButtons.Cancel) ? Visibility.Visible : Visibility.Collapsed;
            box.yes.Visibility = buttons.HasFlag(MsgButtons.Yes) ? Visibility.Visible : Visibility.Collapsed;
            box.no.Visibility = buttons.HasFlag(MsgButtons.No) ? Visibility.Visible : Visibility.Collapsed;
            box.all.Visibility = buttons.HasFlag(MsgButtons.All) ? Visibility.Visible : Visibility.Collapsed;
            box.next.Visibility = buttons.HasFlag(MsgButtons.Next) ? Visibility.Visible : Visibility.Collapsed;
            box.prev.Visibility = buttons.HasFlag(MsgButtons.Previous) ? Visibility.Visible : Visibility.Collapsed;
            box.ShowDialog();
            return box.Result;
        }

        public MsgButtons Result
        {
            get;
            set;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            Result = MsgButtons.OK;
            DialogResult = true;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MsgButtons.Cancel;
            DialogResult = false;
        }

        private void yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MsgButtons.Yes;
            DialogResult = true;
        }

        private void no_Click(object sender, RoutedEventArgs e)
        {
            Result = MsgButtons.No;
            DialogResult = false;
        }

        private void all_Click(object sender, RoutedEventArgs e)
        {
            Result = MsgButtons.All;
            DialogResult = true;
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            Result = MsgButtons.Previous;
            DialogResult = true;
        }

        private void prev_Click(object sender, RoutedEventArgs e)
        {
            Result = MsgButtons.Previous;
            DialogResult = true;
        }
    }
}
