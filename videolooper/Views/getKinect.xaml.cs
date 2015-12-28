using Microsoft.Kinect.Toolkit;
using System;
using System.Windows;
using System.Windows.Controls;
using VideoLooper.Models;

namespace VideoLooper.Views
{
    /// <summary>
    /// Interaction logic for Kinect.xaml
    /// </summary>
    public partial class GetKinect : UserControl
    {
        #region Links
        Kinect Kinect = new Kinect();
        #endregion

        #region Vars
        private static KinectSensorChooser _sensorChooser;
        #endregion

        public GetKinect()
        {
            InitializeComponent();

            //28.01.2015//SC Create an async worker to found a kinect
            _sensorChooser = new KinectSensorChooser();
            sensorChooserUi.KinectSensorChooser = _sensorChooser;
            _sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            _sensorChooser.Start();
        }

        public void sensorChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            if (!Kinect.ChooseKinect(e))
            {
                //Check to see if the sensor exist
                if (e.NewSensor != null)
                {
                    switch (Convert.ToString(e.NewSensor.Status))
                    {
                        case "Connected":
                            PrcKinect.SensorChooser = _sensorChooser;
                            (Application.Current.MainWindow.FindName("ContentArea") as ContentControl).Content = new PrcKinect();
                            break;
                    }
                }
                else
                {
                    //Set the Sensor to NULL
                    PrcKinect.SensorChooser = null;
                    //Call the HomeView, set the error to true
                    Home h = new Home(true);
                    //Change View
                    (Application.Current.MainWindow.FindName("ContentArea") as ContentControl).Content = h;
                }
            }
        }
    }
}
