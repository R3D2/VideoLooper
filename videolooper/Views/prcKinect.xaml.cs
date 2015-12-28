using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Drawing;
using Microsoft.Speech.Recognition;
using VideoLooper.Models;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.Threading;
using Microsoft.Win32;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media;

namespace VideoLooper.Views
{
    /// <summary>
    /// Interaction logic for the kinect processing
    /// </summary>
    public partial class PrcKinect : UserControl
    {
        #region VARS
        private static KinectSensorChooser _sensorChooser; //Kinect of the user
        private byte[] _arrFrameMap; //Array of Bytes - FrameMap
        private DateTime TimerStart { get; set; }
        DispatcherTimer dt = new DispatcherTimer(); // Chrono
        double ConfidenceThreshold = 0.4;
        #endregion 

        #region GET/SET
        public static KinectSensorChooser SensorChooser
        {
            get { return _sensorChooser; }
            set { _sensorChooser = value; }
        }
        #endregion

        #region LINKS
        Video Video = new Video();
        Kinect Kinect = new Kinect();
        #endregion

        public PrcKinect()
        {
            InitializeComponent();
            //SC/28.01.2015/ Initiate the Kinect
            _sensorChooser.Kinect.ColorFrameReady += Kinect_ColorFrameReady;
            Video.IFrameRate = 30;
            //SC/28.01.2015/ Initiate the Kinect speech recognition
            Kinect.GetKinectSpeechRecognizer();
            Kinect.LoadGrammar(Kinect.RecognizerInfo);
            Kinect.SpeechRecognitionEngine.SpeechRecognized += SpeechRecognitionEngine_SpeechRecognized;
            Kinect.StartAudio(SensorChooser);
            //SC//05.02.2015/ UI Control and add events
            btnLoop.IsEnabled = false;
            btnLoop.Click += OnLooping;
            rb3seconds.Checked += LoopSequenceChoosed;
            rb5seconds.Checked += LoopSequenceChoosed;
            rb10seconds.Checked += LoopSequenceChoosed;
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
                //SC Start the timer
                TimerStart = DateTime.Now;
                dt.Tick += new EventHandler(OnDispatcherTimer_Tick);
                dt.Interval = new TimeSpan(0, 0, 1);
                dt.Start();
            }
            else
            {
                //26.01.2015//SC Stop the looping, and reset the length of the sequence
                Video.Clean();
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

        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    //??.12.2014//01.01.2015//SC/ Initialize the array of byte to contain the msap of the frame.
                    _arrFrameMap = new byte[frame.PixelDataLength];

                    //??.12.2014//01.01.2015//SC/ Copy the frames pixel into a byte array.
                    frame.CopyPixelDataTo(_arrFrameMap);
                    
                    //05.02.2015//SC/ Convert ColorImageFrame to bitmap
                    System.Drawing.Image bmp = Video.ImageToBitmap(frame);

                    //13.01.2014//SC If the video is looping
                    if (Video.IsLoop)
                    {
                        //13.01.2014//SC If we are processing the initial sequence
                        if (Video.IsInitSequence)
                        {
                            Video.CatchInitSequence((Bitmap)bmp);
                        }
                        else
                        {
                            bmp = Video.Merge((Bitmap)bmp);
                        }
                    }

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();

                    MemoryStream ms = new MemoryStream();
                    bmp.Save(ms, ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);

                    bi.StreamSource = ms;
                    bi.EndInit();

                    //Using the freeze function to avoid cross thread operations 
                    bi.Freeze();

                    //Calling the UI thread using the Dispatcher to update the 'Image' WPF control         
                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                    {
                        this.kinectColorImage.Source = bi;
                    }));
                }
            }
        }

        private void SpeechRecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //If we are not sure that the command is good
            if (e.Result.Confidence < ConfidenceThreshold)
            {
                lblVoiceCommand.Foreground = new SolidColorBrush(Colors.Red);
                lblVoiceCommand.Content = Convert.ToString(e.Result.Confidence);
            }
            else
            {
                //Get the command said by the user
                string semantic = e.Result.Text.ToString();
                lblVoiceCommand.Foreground = new SolidColorBrush(Colors.Green);
                lblVoiceCommand.Content = char.ToString(semantic[0]) + semantic.Substring(1);

                //Do the action corresponding to the user vocal command
                switch (semantic)
                {
                    case "three":
                        rb3seconds.IsChecked = true;
                        break;
                    case "five":
                        rb5seconds.IsChecked = true;
                        break;
                    case "ten":
                        rb10seconds.IsChecked = true;
                        break;
                    case "loop":
                        if (((bool)rb10seconds.IsChecked) || (bool)rb5seconds.IsChecked || (bool)rb3seconds.IsChecked)
                        {
                            OnLooping(this, new RoutedEventArgs());
                        }
                        break;
                    case "stop":
                        OnLooping(this, new RoutedEventArgs());
                        break;
                    case "fullscreen":
                        Application.Current.MainWindow.WindowState = WindowState.Maximized;
                        break;
                    default:
                        lblVoiceCommand.Content = "NO COMPRENDO...";
                        break;
                }
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
