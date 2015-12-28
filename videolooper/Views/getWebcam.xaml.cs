using AForge.Video.DirectShow;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VideoLooper.Models;

namespace VideoLooper.Views
{
    /// <summary>
    /// Interaction logic for the webcam processing
    /// </summary>
    public partial class GetWebcam : UserControl
    {
        #region VARS
            private VideoCaptureDevice _videoSource;
            private readonly BackgroundWorker _worker = new BackgroundWorker();
        #endregion

        #region LINKS
            Webcam Webcam = new Webcam();
        #endregion

        public GetWebcam()
        {
            //INIT
            InitializeComponent();
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            _worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
             //17.01.2015//SC Loop until a webcam is found
            do
            {
                try
                {
                    //17.01.2015//SC Get the user webcam
                    _videoSource = Webcam.GetConnectedWebcam();
                }
                catch (Exception )
                {
                    break;
                }
            } while (_videoSource == null);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PrcWebcam.UsrWebcam = _videoSource;
            (Application.Current.MainWindow.FindName("ContentArea") as ContentControl).Content = new PrcWebcam();
        }
    }
}
