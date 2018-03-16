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

namespace Claudio
{
    /// <summary>
    /// Логика взаимодействия для Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        public Welcome()
        {
            InitializeComponent();
        }

        private void closeWelcomeWindow(object sender, RoutedEventArgs e)
        {
            Close();

        }

        public void show()
        {

            show();

        }

        private void borderClick(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();

            }

            if (e.ClickCount == 2)
            {

                if (Application.Current.MainWindow.WindowState is WindowState.Maximized)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;


                }
                else if (Application.Current.MainWindow.WindowState is WindowState.Normal)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;


                }

            }
        }
    }
}
