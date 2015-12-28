using AForge.Video.FFMPEG;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VideoLooper.Models
{
    public class Video
    {
        #region VARS
        private static List<Bitmap> lstInitSequence = new List<Bitmap>();  // Init Sequence
        private static List<Bitmap> lstMainSequence = new List<Bitmap>();  // Main Sequence 
        private static List<Bitmap> lstFinalMovie = new List<Bitmap>();    // Output
        private static int _iActualFrame = 0;                            // Id of the actual frame
        private bool _isLoop = false;                                    // Are we looping some shit ?
        private bool _isInitSequence = true;
        private int _iSequenceLength = 0;                                 // Frame Total Per Sequence
        private int _iFrameRate = 0;
        #endregion

        #region GET/SET
        public int IFrameRate
        {
            get { return _iFrameRate; }
            set { _iFrameRate = value; }
        }
        public bool IsInitSequence
        {
            get { return _isInitSequence; }
            set { _isInitSequence = value; }
        }
        public int ISequenceLength
        {
            get { return _iSequenceLength; }
            set { _iSequenceLength = value; }
        }
        public bool IsLoop
        {
            get { return _isLoop; }
            set { _isLoop = value; }
        }
        #endregion

        /// <summary>
        /// Convert a Array of byte into a Bitmap
        /// </summary>
        /// <param name="data">Array of Byte</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="ch">Channel - 1/Gray8 - 2/Gray16 - 3/RGB - 4/RGB+Alpha</param>
        /// <returns></returns>
        public List<Bitmap> ArrayToBmp(List<byte[]> data, int width, int height, int ch)
        {
            System.Windows.Media.PixelFormat format = PixelFormats.Default;
            List<Bitmap> lstBmp = new List<Bitmap>();

            if (ch == 1) format = PixelFormats.Gray8;
            if (ch == 2) format = PixelFormats.Gray16;
            if (ch == 3) format = PixelFormats.Bgr24;
            if (ch == 4) format = PixelFormats.Bgr32;
            
            Bitmap bmp;

            for (int i = 0; i < data.Count; i++)
		    {
                WriteableBitmap wbm = new WriteableBitmap(width, height, 96, 96, format, null);
			    wbm.WritePixels(new Int32Rect(0, 0, width, height), data[i], ch * width, 0);
                BitmapEncoder enc = new BmpBitmapEncoder();

                using (MemoryStream outStream = new MemoryStream())
                {
                    enc.Frames.Add(BitmapFrame.Create(wbm));
                    enc.Save(outStream);
                    bmp = new Bitmap(outStream);
                    lstBmp.Add(bmp);
			    }
            }
            return lstBmp;
        }

        /// <summary>
        /// Convert Kinect image format to Bitmap
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Bitmap ImageToBitmap(ColorImageFrame image)
        {
            byte[] pixeldata = new byte[image.PixelDataLength];
            image.CopyPixelDataTo(pixeldata);
            Bitmap bmp = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            BitmapData bmpdata = bmp.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.WriteOnly,
                bmp.PixelFormat);
            IntPtr ptr = bmpdata.Scan0;
            Marshal.Copy(pixeldata, 0, ptr, image.PixelDataLength);
            bmp.UnlockBits(bmpdata);
            return bmp;
        }

        /// <summary>
        /// Save the Sequence into an mp4 file
        /// </summary>
        /// <param name="path"></param>
        public void SaveSequence(string path)
        {
               // instantiate AVI writer, use WMV3 codec
                VideoFileWriter writer = new VideoFileWriter();
                // create new AVI file and open it
                writer.Open(path, 640, 480, IFrameRate, VideoCodec.MPEG4);

                foreach (var item in lstFinalMovie)
                {
                    writer.WriteVideoFrame(item);
                }
                writer.Close();          
        }

        /// <summary>
        /// Merge two bitmap
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public Bitmap Merge(Bitmap bmp)
        {
            Bitmap bmpimg = new Bitmap(lstInitSequence[_iActualFrame]);

            BitmapData bmpData = bmpimg.LockBits(new Rectangle(0, 0, bmpimg.Width, bmpimg.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            BitmapData bmpData2 = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            int width = bmpData.Width;
            int height = bmpData.Height;

            if (bmpData2.Width > width)
                width = bmpData2.Width;
            if (bmpData2.Height > height)
                height = bmpData2.Height;

            bmpimg.UnlockBits(bmpData);
            bmp.UnlockBits(bmpData2);

            Bitmap bit1 = new Bitmap(bmpimg, width, height);
            Bitmap bit2 = new Bitmap(bmp, width, height);

            Bitmap bmpresult = new Bitmap(width, height);

            BitmapData data1 = bit1.LockBits(new Rectangle(0, 0, bit1.Width, bit1.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            BitmapData data2 = bit2.LockBits(new Rectangle(0, 0, bit2.Width, bit2.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            BitmapData data3 = bmpresult.LockBits(new Rectangle(0, 0, bmpresult.Width, bmpresult.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            unsafe
            {
                int remain1 = data1.Stride - data1.Width * 4;
                int remain2 = data2.Stride - data2.Width * 4;
                int remain3 = data3.Stride - data3.Width * 4;

                byte* ptr1 = (byte*)data1.Scan0;
                byte* ptr2 = (byte*)data2.Scan0;
                byte* ptr3 = (byte*)data3.Scan0;

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width * 4; j++)
                    {
                        ptr3[0] = (byte)(ptr1[0] & ptr2[0]);
                        ptr1++;
                        ptr2++;
                        ptr3++;
                    }

                    ptr1 += remain1;
                    ptr2 += remain2;
                    ptr3 += remain3;
                }
            }

            bit1.UnlockBits(data1);
            bit2.UnlockBits(data2);
            bmpresult.UnlockBits(data3);

            //Add the bitmap to the InitSequence(OnlyFirst30frame) and MainSequence(fullMovie)
            lstInitSequence[_iActualFrame] = bmpresult;
            lstMainSequence.Add(bmpresult);

            //Increment the frame Number
            _iActualFrame += 1;

            if ((_iActualFrame == (ISequenceLength * IFrameRate)))
            {
                AddSequenceToFinalMovie(lstMainSequence);
                _iActualFrame = 0;
                lstMainSequence.Clear();
            }

            return bmpresult;
        }

        /// <summary>
        /// Add the main sequence to the FinalMovie
        /// </summary>
        /// <param name="lstSequence"></param>
        public void AddSequenceToFinalMovie(List<Bitmap> lstSequence)
        {
            //As we can't store all the frames due to memory getting filled
            //We clear the lstFinalMovie each time
            //to save only the sequence
            lstFinalMovie.Clear();

            //Add each frame to the lstFinalMovie
            foreach (var item in lstSequence)
            {
                lstFinalMovie.Add(item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameSequence"></param>
        public void CatchInitSequence(Bitmap frameSequence)
        {
            if ((IsLoop) & (lstInitSequence.Count < ISequenceLength * IFrameRate) & (IsInitSequence))
            {
                lstInitSequence.Add(frameSequence);
                switch (ISequenceLength)
                {
                    case 3:
                        if (lstInitSequence.Count == ISequenceLength * IFrameRate)
                        {
                            AddSequenceToFinalMovie(lstInitSequence);
                            IsInitSequence = false;
                        }
                        break;
                    case 5:
                        if (lstInitSequence.Count == ISequenceLength * IFrameRate)
                        {
                            IsInitSequence = false;
                        }
                        break;
                    case 10:
                        if (lstInitSequence.Count == ISequenceLength * IFrameRate)
                        {
                            IsInitSequence = false;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Clean Environnement, goes to the original state
        /// </summary>
        public void Clean()
        {
            lstFinalMovie.Clear();
            lstInitSequence.Clear();
            lstMainSequence.Clear();
            IsLoop = false;
            _isInitSequence = true;
            _iActualFrame = 0;
        }
    }
}