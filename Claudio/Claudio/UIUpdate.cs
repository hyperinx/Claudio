using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;


namespace Claudio
{
    public class UIUpdate
    {
        public void updateUI(BlockAlignReductionStream stream,
            WaveOut output, 
            Delegate dispose, 
            Delegate setPosition,
            Delegate startPlay,
            CancellationTokenSource ts, 
            List<string> trackList) {

            while (true) {

                if (ts.IsCancellationRequested)
                {              
                    break;
                }

                int t = 0;
                if (t >= 1000) t = 0;

                if (Application.Current != null)
                    Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Normal,
                      (Action)(() =>
                      {
                          if (stream != null && output != null && !ts.IsCancellationRequested)
                          {
                              if (stream.CurrentTime != stream.TotalTime)
                              {
                                  if (t % 1000 == 0)
                                  {
                                      MainWindow.instance.bottomTimeSlider.Value+= 10;
                                      MainWindow.instance.waveSlider.Value+= 10;
                                      MainWindow.instance.currentPosition.Text = TimeSpan.FromSeconds(MainWindow.instance.bottomTimeSlider.Value / 100).ToString(@"mm\:ss");
                                      MainWindow.instance.currentPosition.Text = TimeSpan.FromSeconds(MainWindow.instance.waveSlider.Value / 100).ToString(@"mm\:ss");
                                  }
                              }
                              else if (stream.CurrentTime == stream.TotalTime)
                              {                              
                                  MainWindow.instance.currentPosition.Text = "00:00";
                                  MainWindow.instance.bottomTimeSlider.Value = 0;
                                  MainWindow.instance.waveSlider.Value = 0;

                                  if (MainWindow.repeatTrack)
                                  {
                                      ts.Cancel();
                                      startPlay.DynamicInvoke();
                                  }
                                  else
                                  {

                                      if ((trackList.Count - 1) - OpenLocalFile.trackNo > 0)
                                      {
                                          ts.Cancel();
                                          OpenLocalFile.trackNo++;
                                          startPlay.DynamicInvoke();
                                      }
                                      else
                                      {
                                          
                                          MainWindow.instance.bottomTimeSlider.IsEnabled = false;
                                          MainWindow.instance.waveSlider.IsEnabled = false;
                                         
                                         
                                          ts.Cancel();
                                      }
                                  }
                              }
                          }
                      }));
               Thread.Sleep(100);
            }
        }       
    }
}
