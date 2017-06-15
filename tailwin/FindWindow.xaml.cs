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

namespace tailwin
{
    /// <summary>
    /// Interaction logic for FindWindow.xaml
    /// </summary>
    public partial class FindWindow : Window
    {
        public delegate void SearchEventHandler();
        public event SearchEventHandler Search;

        public bool AllowClose = false;

        private void OnSearch()
        {
            if (Search != null)
            {
                Search();
            }
        }

        public FindWindow()
        {
            InitializeComponent();
            Closing += FindWindow_Closing;
            Activated += FindWindow_Activated;
        }

        private void FindWindow_Activated(object sender, EventArgs e)
        {
            txtFind.Focus();
        }

        private void FindWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!AllowClose)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void txtFind_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void btnFind_Submit_Click(object sender, RoutedEventArgs e)
        {
            OnSearch();
        }

        private void btnFind_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
