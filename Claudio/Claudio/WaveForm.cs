using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WaveFormRendererLib;

namespace Claudio
{
    class WaveForm
    {
        public string selectedFile { get; set; }
        public int waveWidth { get; set; }
        private readonly WaveFormRenderer waveFormRenderer;

        public WaveForm()
        {
            waveFormRenderer = new WaveFormRenderer();

        }

        private WaveFormRendererSettings GetRendererSettings()
        {
            var topSpacerColor = Color.FromArgb(255, 102, 0, 0);

            var soundCloudOrangeTransparentBlocks = new SoundCloudBlockWaveFormSettings(Color.FromArgb(196, 197, 53, 0), topSpacerColor, Color.FromArgb(196, 79, 26, 0),
                Color.FromArgb(64, 79, 79, 79))
            {
                Name = "SoundCloud Orange Transparent Blocks",
                PixelsPerPeak = 2,
                SpacerPixels = 1,
                TopSpacerGradientStartColor = topSpacerColor,
                BackgroundColor = Color.Transparent
            };

            var settings = (WaveFormRendererSettings)soundCloudOrangeTransparentBlocks;
            settings.TopHeight = 75;
            settings.BottomHeight = 35;
            settings.Width = waveWidth;

            return settings;
        }


        public void RenderWaveform()
        {
            if (selectedFile == null) return;
            var settings = GetRendererSettings();
            var peakProvider = new SamplingPeakProvider(200);
            Task.Factory.StartNew(() => RenderThreadFunc(peakProvider, settings));
        }

        private void RenderThreadFunc(IPeakProvider peakProvider, WaveFormRendererSettings settings)
        {
            Image image = null;
            try
            {
                image = waveFormRenderer.Render(selectedFile, peakProvider, settings);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            FinishedRender(image);
        }

        [DllImport("gdi32")]

        static extern int DeleteObject(IntPtr o);

        public static BitmapSource LoadBitmap(Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }
        private void FinishedRender(Image image)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(
               DispatcherPriority.Normal,
               (Action)(() =>
               {
                   MainWindow.instance.waveForm.Source = LoadBitmap(new Bitmap(image));
               }));
        }

    }
}
