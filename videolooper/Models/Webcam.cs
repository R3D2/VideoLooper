using System.Collections.Generic;
using AForge.Video.DirectShow;
using System.Drawing;

namespace VideoLooper.Models
{
    class Webcam
    {
        #region VARS
        public static VideoCaptureDevice VideoSource;
        public static List<Bitmap> LstBmp = new List<Bitmap>();
        #endregion

        public VideoCaptureDevice GetConnectedWebcam()
        {
            //17.01.2015//SC Enumerate webcam/video devices
            var videoDevices = new FilterInfoCollection( FilterCategory.VideoInputDevice );
            if (videoDevices.Count > 0)
            { 
                //17.01.2015//SC Create a new videosource with the first founded devices
                VideoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            }
            return VideoSource;
        }

        /// <summary>
        /// Start the video capture, set the right resolution
        /// </summary>
        /// <returns>returns the average framerate of the camera</returns>
        public int StartVideo()
        {
            int iFrameRate = 0;
            VideoSource.VideoResolution = VideoSource.VideoCapabilities[1];
            iFrameRate = VideoSource.VideoResolution.AverageFrameRate;
            VideoSource.Start();
            return iFrameRate;
        }
    }
}