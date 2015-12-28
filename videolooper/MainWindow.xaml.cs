using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using MahApps.Metro.Controls;
using VideoLooper.Views;

namespace VideoLooper
{
    public partial class MainWindow : MetroWindow
    {   

        public MainWindow()
        {
            InitializeComponent();
            //SC/14.01.2015/ Display HomeView View 
            ContentArea.Content = new Home();
            Home h = (Home)ContentArea.Content;
            //Add custom events
            h.tileKinect.Click += tileKinect_Click;
            h.tileWebcam.Click += tileWebcam_Click;
            h.tileHelp.Click += tileHelp_Click;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //04.02.2015//Set the content area to show Home
            ContentArea.Content = new Home();
            Home vHome = (Home)ContentArea.Content;

            //SC/14.01.2015/Add custom events to the tiles object
            vHome.tileKinect.Click += tileKinect_Click;
            vHome.tileWebcam.Click += tileWebcam_Click;
            vHome.tileHelp.Click += tileHelp_Click;
        }

        void tileHelp_Click(object sender, RoutedEventArgs e)
        {
            //Show Help view
            ContentArea.Content = new Help();
        }

        void tileWebcam_Click(object sender, RoutedEventArgs e)
        {
            //Show GetWebcam view
            ContentArea.Content = new GetWebcam();
        }

        void tileKinect_Click(object sender, RoutedEventArgs e)
        {
            //Show GetKinect view
            ContentArea.Content = new GetKinect();
        }

    }
}
