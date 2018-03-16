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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Claudio
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static MainWindow instance;
        public static bool repeatTrack = false;
        private OpenLocalFile openLocalFile;
        private ControlTemplate repeatButtonTemplate;
        private Image repeatButtonImage;
        public const uint DEFAULT_VOLUME = (uint)(UInt16.MaxValue * 0.7);

        public MainWindow()
        {
            InitializeComponent();
            instance = this;
            openLocalFile = new OpenLocalFile();
            volumeSlider.Minimum = 0;
            volumeSlider.Maximum = 65535;
            volumeSlider.SmallChange = 256;
            volumeSlider.LargeChange = 2048;
            volumeSlider.TickFrequency = 2048;
            volumeSlider.Value = DEFAULT_VOLUME;       

        }
        private void closeMainWindow(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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

        private void openLocalFileClick(object sender, RoutedEventArgs e)
        {
            openLocalFile.openLocalFiles(null, false);  
            if(OpenLocalFile.filesApproved)
            openLocalFile.StartPlay();
          
        }

        private void timeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            openLocalFile.setPos();
            currentPosition.Text = TimeSpan.FromSeconds(bottomTimeSlider.Value/100).ToString(@"mm\:ss");
        }

        private void loginClick(object sender, RoutedEventArgs e)
        {
            new Login().Show();
        }

        private void waveSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            openLocalFile.setPos();
           
            currentPosition.Text = TimeSpan.FromSeconds(waveSlider.Value/100).ToString(@"mm\:ss");
        }

        private void playStateButtonClick(object sender, RoutedEventArgs e)
        {
            openLocalFile.setPlaybackState();
        }

        private void previousPlayButtonClick(object sender, RoutedEventArgs e)
        {
            openLocalFile.playPrevious();

        }

        private void nextPlayButtonClick(object sender, RoutedEventArgs e)
        {
            openLocalFile.playNext();

        }

        private void dropFieldDrop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                 openLocalFile.openLocalFiles((string[])e.Data.GetData(DataFormats.FileDrop), true);
                 openLocalFile.StartPlay();
            }

        }

        private void AudioConfigClick(object sender, RoutedEventArgs e)
        {
            new WindowConfig().Show();
        }

        private void volumeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OpenLocalFile.output != null)
            {

                OpenLocalFile.waveOutSetVolume((uint)IntPtr.Zero, (uint)volumeSlider.Value + ((uint)volumeSlider.Value << 16));
                    
            }
        }

        private void repeatButtonClick(object sender, RoutedEventArgs e)
        {
            repeatButtonTemplate = repeatButton.Template;
            repeatButtonImage = (Image)repeatButtonTemplate.FindName("repeatButtonImage", repeatButton);

            repeatTrack = !repeatTrack;
            if (repeatTrack)
                repeatButtonImage.Source = new BitmapImage(new Uri("Resources/repeat_button.png", UriKind.RelativeOrAbsolute));
            else {
                repeatButtonImage.Source = new BitmapImage(new Uri("Resources/repeat_buttonOff.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            string about = "Made for ZJO\nE-mails:\nm.morrison8996@gmail.com\nandrey.lodz.pl@gmail.com";
            MessageBox.Show(about, "Credits", MessageBoxButton.OK);

        }
    }
}
