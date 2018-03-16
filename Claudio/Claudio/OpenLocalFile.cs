using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TagLib;

namespace Claudio
{ 
    class artistTitle<T1, T2> {
       public T1 artist { get; set; }
       public T2 title { get; set; }

        public artistTitle(T1 t1, T2 t2)
        {         
           artist = t1;
           title = t2;
        }
    }

    class OpenLocalFile
    {
        private OpenFileDialog openFileDialog;
        private BlockAlignReductionStream stream = null;
        public static WaveOut output = null;
        private WaveStream pcm = null;
        public delegate void DisposeWaveDelegate();
        public delegate void SetPositionDelegate();
        public delegate void StartPlayDelegate();
        private DisposeWaveDelegate disposeWaveDelegate;
        private SetPositionDelegate setPositionDelegate;
        private StartPlayDelegate startPlayDelegate;
        public List<string> trackList {get; set;}
        public List<artistTitle<string, string>> trackFullName;
        public CancellationTokenSource ts {get; set;}
        public static int trackNo = 0;
        private WaveForm waveForm;
        private TagLib.File track;
        private ControlTemplate playStateButtonTemplate;
        private ControlTemplate previousButtonTemplate;
        private ControlTemplate nextButtonTemplate;
        private Image playStateButtonImage;
        private Image previousButtonImage;
        private Image nextButtonImage;
        public static bool filesApproved {get; set;}

        public void setCoverImage(string f) {
            
            IPicture pic = null;
            BitmapImage bitmap = null;
            TagLib.File file = TagLib.File.Create(f);

            if (file.Tag.Pictures.Length != 0)
            {
                pic = file.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                // ImageSource for System.Windows.Controls.Image
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                if (Application.Current != null)
                    Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Normal,
                   (Action)(() =>
                   {
                       MainWindow.instance.coverImage.Source = bitmap;
                   }));
            }
            else
            {
                if (Application.Current != null)
                    Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Normal,
                   (Action)(() =>
                   {
                       MainWindow.instance.coverImage.Source = new BitmapImage(new Uri("Resources/default_cover.png", UriKind.RelativeOrAbsolute));
                   }));
              }
        }

        public void openLocalFiles(string[] tracks, bool dropped)
        {
            trackList = new List<string>();
            trackFullName = new List<artistTitle<string, string>>();

            if (!dropped)
            {
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
                openFileDialog.Multiselect = true;

                if ((bool)openFileDialog.ShowDialog())
                {
                    filesApproved = true;

                    foreach (String file in openFileDialog.FileNames)
                    {
                        trackList.Add(file);
                        track = TagLib.File.Create(file);

                        if (trackList[trackNo].EndsWith("mp3"))
                        {
                            if (track.Tag.Performers.Length > 0 || track.Tag.Title != null)
                            {
                                trackFullName.Add(new artistTitle<string, string>(track.Tag.Performers[0], track.Tag.Title));
                            }
                            else
                            {
                                trackFullName.Add(new artistTitle<string, string>("", Path.GetFileName(file)));
                            }
                        }
                        else trackFullName.Add(new artistTitle<string, string>("", Path.GetFileName(file)));
                    }
                }
                else filesApproved = false;
            }
            else if (tracks != null)
            {
                filesApproved = true;

                foreach (String file in tracks)
                {
                    trackList.Add(file);
                    track = TagLib.File.Create(file);

                    if (track.Tag.Performers.Length > 0 || track.Tag.Title != null)
                    {
                        trackFullName.Add(new artistTitle<string, string>(track.Tag.Performers[0], track.Tag.Title));
                    }
                    else
                    {
                        trackFullName.Add(new artistTitle<string, string>("", Path.GetFileName(file)));
                    }
                }
            }
            if (filesApproved) {
                playStateButtonTemplate = MainWindow.instance.playStateButton.Template;
                previousButtonTemplate = MainWindow.instance.previousPlayButton.Template;
                nextButtonTemplate = MainWindow.instance.nextPlayButton.Template;
                playStateButtonImage = (Image)playStateButtonTemplate.FindName("playStateButtonImage", MainWindow.instance.playStateButton);
                previousButtonImage = (Image)previousButtonTemplate.FindName("previousPlayButtonImage", MainWindow.instance.previousPlayButton);
                nextButtonImage = (Image)nextButtonTemplate.FindName("nextPlayButtonImage", MainWindow.instance.nextPlayButton);

                playStateButtonImage.Source = new BitmapImage(new Uri("Resources/pause_button.png", UriKind.RelativeOrAbsolute));

                waveForm = new WaveForm();

                disposeWaveDelegate = DisposeWave;
                setPositionDelegate = setPos;
                startPlayDelegate = StartPlay;
                trackNo = 0;
                MainWindow.instance.playStateButton.IsEnabled = true;
            }
        }

        public void StartPlay() {

            if (!trackFullName[trackNo].artist.Equals("") && !trackFullName[trackNo].title.Equals(""))
            {             
                MainWindow.instance.artistLabel.Content = trackFullName[trackNo].artist;
                MainWindow.instance.trackNameLabel.Content = trackFullName[trackNo].title;

                if (trackNo == 0)
                {

                    MainWindow.instance.previousTrack.Content = "";
                    MainWindow.instance.currentTrack.Content = trackFullName[trackNo].artist + " - " + trackFullName[trackNo].title;
                    previousButtonImage.Source = new BitmapImage(new Uri("Resources/previous_noactive.png", UriKind.RelativeOrAbsolute));
                    if (trackList.Count == 1) { 
                            nextButtonImage.Source = new BitmapImage(new Uri("Resources/next_noactive.png", UriKind.RelativeOrAbsolute));
                            MainWindow.instance.previousTrack.Content = "";
                            MainWindow.instance.nextTrack.Content = "";
                            MainWindow.instance.currentTrack.Content = "";
                        }
                else nextButtonImage.Source = new BitmapImage(new Uri("Resources/next_active.png", UriKind.RelativeOrAbsolute));
                    if (trackNo + 1 < trackList.Count)
                        MainWindow.instance.nextTrack.Content = trackFullName[trackNo + 1].artist + " - " + trackFullName[trackNo + 1].title;

                }
                else 
                {
                    MainWindow.instance.currentTrack.Content = trackFullName[trackNo].artist + " - " + trackFullName[trackNo].title;

                    if (trackNo - 1 >= 0)
                    {
                        MainWindow.instance.previousTrack.Content = trackFullName[trackNo - 1].artist + " - " + trackFullName[trackNo - 1].title;
                        previousButtonImage.Source = new BitmapImage(new Uri("Resources/previous_active.png", UriKind.RelativeOrAbsolute));
                    }

                    if (trackNo + 1 < trackList.Count)
                    {
                        MainWindow.instance.nextTrack.Content = trackFullName[trackNo + 1].artist + " - " + trackFullName[trackNo + 1].title;
                        nextButtonImage.Source = new BitmapImage(new Uri("Resources/next_active.png", UriKind.RelativeOrAbsolute));
                    }
                    if (trackNo + 1 == trackList.Count)
                    {
                        MainWindow.instance.nextTrack.Content = "";
                        nextButtonImage.Source = new BitmapImage(new Uri("Resources/next_noactive.png", UriKind.RelativeOrAbsolute));

                    }
                }
            }
            else
            {
                MainWindow.instance.artistLabel.Content = trackFullName[trackNo].title;
                MainWindow.instance.trackNameLabel.Content = "";

                if (trackNo == 0)
                {
                    MainWindow.instance.previousTrack.Content = "";
                    MainWindow.instance.currentTrack.Content = trackFullName[trackNo].title;
                    previousButtonImage.Source = new BitmapImage(new Uri("Resources/previous_noactive.png", UriKind.RelativeOrAbsolute));
                    if (trackList.Count == 1)
                    {
                        nextButtonImage.Source = new BitmapImage(new Uri("Resources/next_noactive.png", UriKind.RelativeOrAbsolute));
                        MainWindow.instance.previousTrack.Content = "";
                        MainWindow.instance.nextTrack.Content = "";
                        MainWindow.instance.currentTrack.Content = "";
                    }
                    else nextButtonImage.Source = new BitmapImage(new Uri("Resources/next_active.png", UriKind.RelativeOrAbsolute));
                    if (trackNo + 1 < trackList.Count)
                        MainWindow.instance.nextTrack.Content = trackFullName[trackNo + 1].title;
                }
                else 
                {
                    MainWindow.instance.currentTrack.Content = trackFullName[trackNo].title;

                    if (trackNo - 1 >= 0)
                    {
                       
                        MainWindow.instance.previousTrack.Content = trackFullName[trackNo - 1].title;
                        previousButtonImage.Source = new BitmapImage(new Uri("Resources/previous_active.png", UriKind.RelativeOrAbsolute));
                    }

                    if (trackNo+1 < trackList.Count)
                    {
                        MainWindow.instance.nextTrack.Content = trackFullName[trackNo + 1].title;
                        nextButtonImage.Source = new BitmapImage(new Uri("Resources/next_active.png", UriKind.RelativeOrAbsolute));
                    }
                    if (trackNo + 1 == trackList.Count)
                    {
                        MainWindow.instance.nextTrack.Content = "";
                        nextButtonImage.Source = new BitmapImage(new Uri("Resources/next_noactive.png", UriKind.RelativeOrAbsolute));
                    }
                }
            }

            if (trackNo + 1 < trackList.Count)
            {
                playStateButtonImage.Source = new BitmapImage(new Uri("Resources/pause_button.png", UriKind.RelativeOrAbsolute));
                MainWindow.instance.nextPlayButton.IsEnabled = true;

            }
            if (trackNo > 0) MainWindow.instance.previousPlayButton.IsEnabled = true;


            if (ts != null)
            {
                ts.Cancel();
                DisposeWave();
            }
                   
            Console.WriteLine(trackNo);

            if (trackList[trackNo].EndsWith("mp3"))
            {
                pcm = WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(trackList[trackNo]));
               
                stream = new BlockAlignReductionStream(pcm);
            }
            else if (trackList[trackNo].EndsWith("wav"))
            {

                WaveStream pcm = new WaveChannel32(new WaveFileReader(trackList[trackNo]));
               
                stream = new BlockAlignReductionStream(pcm);
            }
            else throw new InvalidOperationException("Not a correct audio file type.");

            playStateButtonImage.Source = new BitmapImage(new Uri("Resources/pause_button.png", UriKind.RelativeOrAbsolute));

            setCoverImage(trackList[trackNo]);
            output = new WaveOut();

            output.Init(stream);
            output.Play();

            waveForm.selectedFile = trackList[trackNo];
            waveForm.waveWidth = (int)MainWindow.instance.Width;
            waveForm.RenderWaveform();

            MainWindow.instance.bottomTimeSlider.Minimum = 0;
            MainWindow.instance.bottomTimeSlider.Maximum = (stream.TotalTime).TotalSeconds * 100;
            MainWindow.instance.bottomTimeSlider.Value = 0;
            MainWindow.instance.bottomTimeSlider.IsEnabled = true;
            MainWindow.instance.waveSlider.IsEnabled = true;
            MainWindow.instance.waveSlider.Minimum = 0;
            MainWindow.instance.waveSlider.Maximum = (stream.TotalTime).TotalSeconds * 100;
            MainWindow.instance.waveSlider.Value = 0;
            MainWindow.instance.totalTime.Text = (stream.TotalTime).ToString(@"mm\:ss");
            MainWindow.instance.currentPosition.Text = "00:00";
           
            startUpdateUI();
        }

        public void DisposeWave()
        {
            if (ts.IsCancellationRequested && !MainWindow.instance.bottomTimeSlider.IsMouseOver)
            {
                try
                {
                    if (output != null)
                    {
                        if (output.PlaybackState == PlaybackState.Playing) output.Stop();
                        output.Dispose();
                        output = null;
                    }
                    if (stream != null)
                    {
                        stream.Dispose();
                        stream = null;
                    }
                }
                catch (NAudio.MmException mmex) {
                    Console.Error.WriteLine(mmex.Message);
                }
            }
        }

        [DllImport("winmm.dll")]
        public static extern long waveOutSetVolume(UInt32 deviceID, UInt32 Volume);

        public void playPrevious()
        {
            if (TimeSpan.FromSeconds(MainWindow.instance.bottomTimeSlider.Value / 100).TotalSeconds <= 10)
            {
                if (trackNo - 1 >= 0)
                {
                    ts.Cancel();
                    DisposeWave();
                    trackNo--;
                    StartPlay();
                }
                else
                {
                    MainWindow.instance.previousPlayButton.IsEnabled = false;
                }
            }
            else {

                ts.Cancel();
                StartPlay();

            }
        }

        public void playNext() {

            if (trackNo + 1 < trackList.Count)
            {
                ts.Cancel();
                DisposeWave();
                trackNo++;
                StartPlay();
            }
            else {
                MainWindow.instance.nextPlayButton.IsEnabled = false;
            }
        }

        public void setPlaybackState()
        {    
            if (output.PlaybackState == PlaybackState.Playing)
            {
                output.Pause();
                ts.Cancel();
                playStateButtonImage.Source = new BitmapImage(new Uri("Resources/start_play_nolight.png", UriKind.RelativeOrAbsolute));
            }
            else if (output.PlaybackState == PlaybackState.Paused)
            {
                output.Play();
                startUpdateUI();
                playStateButtonImage.Source = new BitmapImage(new Uri("Resources/pause_button.png", UriKind.RelativeOrAbsolute));
            }
        }

        public void setPos()
        {
            if (stream != null && output != null)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    if (MainWindow.instance.bottomTimeSlider.IsMouseOver)
                    {
                        output.Pause();
                    
                        WaveStreamExtensions.SetPosition(stream, MainWindow.instance.bottomTimeSlider.Value/100);
                        MainWindow.instance.waveSlider.Value = MainWindow.instance.bottomTimeSlider.Value;
                    }
                    else if (MainWindow.instance.waveSlider.IsMouseOver)
                    {
                        output.Pause();
                    
                        WaveStreamExtensions.SetPosition(stream, MainWindow.instance.waveSlider.Value/100);
                        MainWindow.instance.bottomTimeSlider.Value = MainWindow.instance.waveSlider.Value;
                    }
                }
                else if (output.PlaybackState == PlaybackState.Paused && !MainWindow.instance.playStateButton.IsMouseOver ) {

                    output.Play();                                
                }
            }
        }
        public void startUpdateUI()
        {
            ts = new CancellationTokenSource();
            CancellationToken ct = ts.Token;

            Task.Factory.StartNew(() =>
                {
                    new UIUpdate().updateUI(stream, output, disposeWaveDelegate, setPositionDelegate, startPlayDelegate, ts, trackList);
                }, ct);
        }
    }
}

