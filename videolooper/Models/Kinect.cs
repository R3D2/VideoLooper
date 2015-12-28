using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Linq;

namespace VideoLooper.Models
{
    public class Kinect
    {
        #region VARS
        private RecognizerInfo _recognizerInfo;
        private SpeechRecognitionEngine _speechRecognitionEngine;
        #endregion

        #region GET/SET
        public RecognizerInfo RecognizerInfo
        {
            get { return _recognizerInfo; }
            set { _recognizerInfo = value; }
        }
        public SpeechRecognitionEngine SpeechRecognitionEngine
        {
            get { return _speechRecognitionEngine; }
            set { _speechRecognitionEngine = value; }
        }
        #endregion

        #region KinectHandler
        /// <summary>
        /// Kinect State Method Handler
        /// Source : http://dotneteers.net/blogs/vbandi/archive/2013/03/25/kinect-interactions-with-wpf-part-i-getting-started.aspx
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool ChooseKinect(KinectChangedEventArgs args)
        {
            bool error = false;
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }

                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    error = true;
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    //args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    //args.NewSensor.SkeletonStream.Enable();
                    try
                    {
                        //args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        //args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }

                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                        error = true;
                    }
                }

                catch (InvalidOperationException)
                {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
            return error;
        }
        #endregion

        #region SpeechEngineRecognizer
        /// <summary>
        /// Get the Speech recognition function available on the kinect and get the
        /// English language, set it on our SpeechRecognitionEngine.
        /// </summary>
        public void GetKinectSpeechRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase)
                    && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };

            RecognizerInfo ri = SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();

            if( ri != null)
            {
                RecognizerInfo = ri;
            }
        }

        /// <summary>
        /// Load our Grammar Dictionnary to our SpeechRecognitionEngine
        /// </summary>
        /// <param name="ri"></param>
        public void LoadGrammar(RecognizerInfo ri)
        {
            //Init
            SpeechRecognitionEngine = new SpeechRecognitionEngine(ri.Id);
            var directions = new Choices();
            
            //Add Words To the dictioinnry
            directions.Add(new SemanticResultValue("three", "three"));
            directions.Add(new SemanticResultValue("five", "five"));
            directions.Add(new SemanticResultValue("ten", "ten"));
            directions.Add(new SemanticResultValue("loop", "loop"));
            directions.Add(new SemanticResultValue("stop", "stop"));
            directions.Add(new SemanticResultValue("fullscreen", "fullscreen"));
  
            //Instansiate new GrammarBuilder and add the dictionnary
            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(directions);
            
            //Load the grammar to the SpeechRecognitionEngine of the kinect
            var g = new Grammar(gb);
            SpeechRecognitionEngine.LoadGrammar(g);
        }

        /// <summary>
        /// Start the audio monitoring of the Kinect
        /// </summary>
        /// <param name="sensorChooser"></param>
        public void StartAudio(KinectSensorChooser sensorChooser)
        {
            SpeechRecognitionEngine.SetInputToAudioStream(sensorChooser.Kinect.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            SpeechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }
        #endregion
    }
}
