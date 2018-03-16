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
    /// Логика взаимодействия для WindowConfig.xaml
    /// </summary>
    public partial class WindowConfig : Window
    {
       

        public WindowConfig()
        {
            InitializeComponent();
            
        }

        

        private void closeLoginWindow(object sender, RoutedEventArgs e)
        {
            Close();

        }


        private void borderClick(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();

            }
        }

        private void saveConfigButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
         

        }
    }
}
