using AForge.Video;
using AForge.Video.DirectShow;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VideoLooper.Models;

namespace VideoLooper.Views
{
    /// <summary>
    /// View of the mWebcam processing
    /// </summary>
    public partial class PrcWebcam : UserControl
    {
        #region VARS
        private static VideoCaptureDevice _usrWebcam; //User mWebcam
        private DateTime TimerStart { get; set; } // Chrono
        DispatcherTimer dt = new DispatcherTimer();
        #endregion

        #region LINKS
        Webcam Webcam = new Webcam(); //Model Webcam
        Video Video = new Video(); //Model Video
        #endregion

        #region GET/SET
        public static VideoCaptureDevice UsrWebcam
        {
            get { return _usrWebcam; }
            set { _usrWebcam = value; }
        }
        #endregion

        public PrcWebcam()
        {
            InitializeComponent();
            //SC Add a new event to process the Webcam frames and start the video
            _usrWebcam.NewFrame += new NewFrameEventHandler(usrWebcam_NewFrame);
            Video.IFrameRate = Webcam.StartVideo();
            //SC UI CLEAN
            btnLoop.IsEnabled = false;
            btnLoop.Click += OnLooping;
            rb3seconds.Checked += LoopSequenceChoosed;
            rb5seconds.Checked += LoopSequenceChoosed;
            rb10seconds.Checked += LoopSequenceChoosed;
        }

        void LoopSequenceChoosed(object sender, RoutedEventArgs e)
        {
            if ((Convert.ToBoolean(rb3seconds.IsChecked)) || (Convert.ToBoolean(rb5seconds.IsChecked)) || (Convert.ToBoolean(rb10seconds.IsChecked)))
            {
                btnLoop.IsEnabled = true;
            }
            else
            {
                btnLoop.IsEnabled = false;
            }
        }

        private void usrWebcam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //00.12.2014//SC Create a clone of the frame
            System.Drawing.Image usrWebcamFrame = (Bitmap)eventArgs.Frame.Clone();
            
            //13.01.2014//SC If the video is looping
            if (Video.IsLoop)
            {
                //13.01.2014//SC If we are processing the initial sequence
                if (Video.IsInitSequence)
                {
                    Video.CatchInitSequence((Bitmap)usrWebcamFrame);
                }
                else
                {
                    usrWebcamFrame = Video.Merge((Bitmap)usrWebcamFrame);
                }
            }

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();

            MemoryStream ms = new MemoryStream();
            usrWebcamFrame.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);

            bi.StreamSource = ms;
            bi.EndInit();

            //Using the freeze function to avoid cross thread operations 
            bi.Freeze();

            //Calling the UI thread using the Dispatcher to update the 'Image' WPF control         
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                pbWebcamOutput.Source = bi;
            }));
        }

        private async void OnLooping(object sender, RoutedEventArgs e)
        {
            if (!Video.IsLoop)
            {
                //26.01.2015//SC Start the loop
                Video.IsLoop = true;
                //SC Set the length sequence
                if (Convert.ToBoolean(rb3seconds.IsChecked))
                {
                    Video.ISequenceLength = 3;
                }
                if (Convert.ToBoolean(rb5seconds.IsChecked))
                {
                    Video.ISequenceLength = 5;
                }
                if (Convert.ToBoolean(rb10seconds.IsChecked))
                {
                    Video.ISequenceLength = 10;
                }
                //SC UI CLEAN
                btnLoop.Content = "STOP";
                lblChrono.Visibility = Visibility.Visible;
                rb3seconds.Visibility = Visibility.Hidden;
                rb5seconds.Visibility = Visibility.Hidden;
                rb10seconds.Visibility = Visibility.Hidden;
                //SC Start the timer for the countdown
                TimerStart = DateTime.Now;
                dt.Tick += new EventHandler(OnDispatcherTimer_Tick);
                dt.Interval = new TimeSpan(0, 0, 1);
                dt.Start();
            }
            else
            {
                //26.01.2015//SC Stop the looping, and reset the length of the sequence
                Video.Clean();
                //26.01.2015//SC Stop the looping, and reset the length of the sequence
                Video.IsLoop = false;
                Video.ISequenceLength = 0;
                //SC UI CLEAN
                btnLoop.Content = "LOOP";
                lblChrono.Visibility = Visibility.Hidden;
                rb3seconds.Visibility = Visibility.Visible;
                rb5seconds.Visibility = Visibility.Visible;
                rb10seconds.Visibility = Visibility.Visible;
                //SC Start the timer 
                dt.Stop();

                //26.01.2015//SC Throw a message to the user
                MessageDialogResult result = await ShowMessage().ConfigureAwait(false);
                
                //26.01.2015//SC Try to save the video
                if (result == MessageDialogResult.Affirmative)
                {
                    SaveFileDialog sfDialog = new SaveFileDialog();
                    sfDialog.InitialDirectory = Convert.ToString(Environment.SpecialFolder.MyDocuments);
                    sfDialog.Filter = ".avi (*.avi)|*.avi|All Files (*.*)|*.*";
                    //SC if the user gave a name to the file, save the sequence
                    if (sfDialog.ShowDialog() != null)
                    {
                        Video.SaveSequence(sfDialog.FileName);
                    }
                }

                //26.01.2015//SC Waiting for the thread to finish before changing the label of the button
                //=> Calling the UI thread using the Dispatcher to update the button  
                await Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    btnLoop.Content = "LOOP";
                }));
            }
        }

        private static async Task<MessageDialogResult> ShowMessage()
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            return await metroWindow.ShowMessageAsync("Save", "Do you want to save the sequence ?", MessageDialogStyle.AffirmativeAndNegative);
        }

        private void OnDispatcherTimer_Tick(object sender, EventArgs e)
        {
            var currentValue = DateTime.Now - TimerStart;
            lblChrono.Content = currentValue.Seconds.ToString();
        }
    }
}
