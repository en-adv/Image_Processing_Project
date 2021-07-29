using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Collections;

namespace UAS_CItra
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoDevice;
        private ArrayList listCamera = new ArrayList();
     
        Bitmap sourceImage = null;
        Bitmap detectedImage = null;
        Bitmap RGBImage, HSLImage, YCbCrImage;
        Bitmap track;


        int Hmin, Hmax, Rmin, Rmax, Gmin, Gmax, Bmin, Bmax;
        float Smin, Smax, Lmin, Lmax, Ymin, Ymax, Cbmin, Cbmax, Crmin, Crmax;
        int TRACK_SPACE = 2;
        int pilChannel = 1;

        bool cameraUsed = false;

        public Form1()
        {
            InitializeComponent();

            trackBarInit(pilChannel);
            trackBarReset(pilChannel);
            trackBarEnable(false);
            labelReset();

            // picture box border
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
        }

        private void trackBarEnable(bool isEnable = true)
        {
            trackBar1.Enabled = isEnable;
            trackBar3.Enabled = isEnable;
            trackBar5.Enabled = isEnable;
            trackBar2.Enabled = isEnable;
            trackBar4.Enabled = isEnable;
            trackBar6.Enabled = isEnable;
        }
        private void trackBarInit(int channel)
        {
            if (channel == 1)
            {
                trackBar1.Maximum = 255;
                trackBar3.Maximum = 255;
                trackBar5.Maximum = 255;
                trackBar2.Maximum = 255;
                trackBar4.Maximum = 255;
                trackBar6.Maximum = 255;
            }
            else if (channel == 2)
            {
                trackBar1.Maximum = 360;
                trackBar3.Maximum = 100;
                trackBar5.Maximum = 100;
                trackBar2.Maximum = 360;
                trackBar4.Maximum = 100;
                trackBar6.Maximum = 100;
            }
            else if (channel == 3)
            {

                trackBar1.Maximum = 100;
                trackBar3.Minimum = -50;
                trackBar3.Maximum = 50;
                trackBar5.Minimum = -50;
                trackBar5.Maximum = 50;
                trackBar2.Maximum = 100;
                trackBar4.Minimum = -50;
                trackBar4.Maximum = 50;
                trackBar6.Minimum = -50;
                trackBar6.Maximum = 50;
            }

        }
        private void trackBarReset(int filterChannel)
        {
            if (filterChannel == 1)
            {
                // -------- RGB ---------
                // RGB channel value
                trackBar1.Value = 0;
                trackBar3.Value = 0;
                trackBar5.Value = 0;
                trackBar2.Value = 255;
                trackBar4.Value = 255;
                trackBar6.Value = 255;

                // RGB value reset
                Rmin = 0; Rmax = 255;
                Gmin = 0; Gmax = 255;
                Bmin = 0; Bmax = 255;
            }
            else if (filterChannel == 2)
            {
                // -------- HSL ---------
                // HSL Channel Value
                trackBar1.Value = 0;
                trackBar3.Value = 0;
                trackBar5.Value = 0;
                trackBar2.Value = 360;
                trackBar4.Value = 100;
                trackBar6.Value = 100;

                // HSL channel value
                Hmin = trackBar1.Value;
                Hmax = trackBar2.Value;
                Smin = (float)trackBar3.Value / 100;
                Smax = (float)trackBar4.Value / 100;
                Lmin = (float)trackBar5.Value / 100;
                Lmax = (float)trackBar6.Value / 100;
            }
            else if (filterChannel == 3)
            {
                // -------- YCbCr ---------
                // YCbCr channel value
                trackBar1.Value = 0;
                trackBar3.Value = -50;
                trackBar5.Value = -50;
                trackBar2.Value = 100;
                trackBar4.Value = 50;
                trackBar6.Value = 50;

                // YCbCr value reset
                Ymin = (float)trackBar1.Value / 100;
                Ymax = (float)trackBar2.Value / 100;
                Cbmin = (float)trackBar3.Value / 100;
                Cbmax = (float)trackBar4.Value / 100;
                Crmin = (float)trackBar5.Value / 100;
                Crmax = (float)trackBar6.Value / 100;
            }
        }
        private void labelReset(int filterChannel)
        {
            if (filterChannel == 1)
            {
                label1.Text = string.Format("RMin : {0}", trackBar1.Value);
                label3.Text = string.Format("GMin : {0}", trackBar3.Value);
                label5.Text = string.Format("BMin : {0}", trackBar5.Value);
                label2.Text = string.Format("RMax : {0}", trackBar2.Value);
                label4.Text = string.Format("GMax : {0}", trackBar4.Value);
                label6.Text = string.Format("BMax : {0}", trackBar6.Value);
            }
            else if (filterChannel == 2)
            {
                label1.Text = string.Format("HueMin : {0}", trackBar1.Value);
                label3.Text = string.Format("SMin : {0}", (float)trackBar3.Value / 100);
                label5.Text = string.Format("LMin : {0}", (float)trackBar5.Value / 100);
                label2.Text = string.Format("HueMax : {0}", trackBar2.Value);
                label4.Text = string.Format("SMax : {0}", (float)trackBar5.Value / 100);
                label6.Text = string.Format("LMax : {0}", (float)trackBar6.Value / 100);
            }
            else if (filterChannel == 3)
            {
                label2.Text = string.Format("Ymax : {0}", (float)trackBar2.Value / 100);
                label1.Text = string.Format("Ymin : {0}", (float)trackBar1.Value / 100);
                label4.Text = string.Format("Cbmax : {0}", (float)trackBar4.Value / 100);
                label3.Text = string.Format("Cbmin : {0}", (float)trackBar3.Value / 100);
                label6.Text = string.Format("Crmax : {0}", (float)trackBar6.Value / 100);
                label5.Text = string.Format("Crmin : {0}", (float)trackBar5.Value / 100);
            }

        }

        private void videoSourcePlayer1_NewFrame(object sender, ref Bitmap image)
        {

            try
            {
                Bitmap histogramImage = image.Clone() as Bitmap;

                sourceImage = image.Clone() as Bitmap;
                pictureBox3.Image = (Bitmap)image.Clone();

                if (pilChannel == 1)
                {
                    //detect the image
                    RGBFiltering(image.Clone() as Bitmap);
                    //traking the image
                    objectTracking(image.Clone() as Bitmap, RGBImage);
                }
                else if (pilChannel == 2)
                {
                    HSLFiltering(image.Clone() as Bitmap);
                    //traking the image
                    objectTracking(image.Clone() as Bitmap, HSLImage);
                }
                else if (pilChannel == 3)
                {
                    YCbCrFiltering(image.Clone() as Bitmap);
                    //traking the image
                    objectTracking(image.Clone() as Bitmap, YCbCrImage);
                }

                hitungHistogram(histogramImage);
            }
            catch
            {

            }
        }
        private void labelReset()
        {
            label1.Text = "";
            label3.Text = "";
            label5.Text = "";
            label2.Text = "";
            label4.Text = "";
            label6.Text = "";
        }

        private void OpenCamera()
        {
            try
            {
                usbcamera = comboBox1.SelectedIndex.ToString();
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count != 0)
                {
                    foreach (FilterInfo device in videoDevices)
                    {
                        listCamera.Add(device.Name);
                    }
                }
                else
                {
                    MessageBox.Show("Camera devices found");
                }
                videoDevice = new
               VideoCaptureDevice(videoDevices[Convert.ToInt32(usbcamera)].MonikerString);

                OpenVideoSource(videoDevice);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count != 0)
            {
          
                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);
                }
            }
            else
            {
                comboBox1.Items.Add("No DirectShow devices found");
            }
            comboBox1.SelectedIndex = 0;

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (videoDevice != null && videoDevice.IsRunning)
                videoDevice.Stop();
        }

        private static string _usbcamera;
        public string usbcamera
        {
            get { return _usbcamera; }
            set { _usbcamera = value; }
        }

        

        private void OpenVideoSource(IVideoSource source)
        {
            try
            {
           
                this.Cursor = Cursors.WaitCursor;
           
                CloseCurrentVideoSource();
              
                videoSourcePlayer1.VideoSource = source;
                videoSourcePlayer1.Start();

                this.Cursor = Cursors.Default;
            }
            catch { }
        }

        public void CloseCurrentVideoSource()
        {
            try
            {
                if (videoSourcePlayer1.VideoSource != null)
                {
                    videoSourcePlayer1.SignalToStop();
                 
                    for (int i = 0; i < 30; i++)
                    {
                        if (!videoSourcePlayer1.IsRunning)
                            break;
                        System.Threading.Thread.Sleep(100);
                    }
                    if (videoSourcePlayer1.IsRunning)
                    {
                        videoSourcePlayer1.Stop();
                    }
                    videoSourcePlayer1.VideoSource = null;

                }
            }
            catch { }
        }

        private void HSLFiltering(Bitmap srcImage)
        {
            // create filter
            HSLFiltering filter = new HSLFiltering();
            filter.Hue = new IntRange(Hmin, Hmax);
            filter.Saturation = new Range(Smin, Smax);
            filter.Luminance = new Range(Lmin, Lmax);
            // apply the filter
            HSLImage = filter.Apply(srcImage);
            pictureBox1.Image = HSLImage;
            
        }

        private void RGBFiltering(Bitmap srcImage)
        {
            // create filter
            ColorFiltering filter = new ColorFiltering();
            // set color ranges to keep
            filter.Red = new IntRange(Rmin, Rmax);
            filter.Green = new IntRange(Gmin, Gmax);
            filter.Blue = new IntRange(Bmin, Bmax);
            // apply the filter
            RGBImage = filter.Apply(srcImage);
            
            pictureBox1.Image = RGBImage;
        }

        private void YCbCrFiltering(Bitmap srcImage)
        {
            // create filter
            YCbCrFiltering filter = new YCbCrFiltering();
            // set color ranges to keep
            filter.Y = new Range(Ymin, Ymax);
            filter.Cb = new Range(Cbmin, Cbmax);
            filter.Cr = new Range(Crmin, Crmax);
            YCbCrImage = filter.Apply(srcImage);
            //draw the picture
            pictureBox1.Image = YCbCrImage;
            
        }

        private void trackObject(Bitmap image)
        {

            Bitmap tempImage = new Bitmap(sourceImage);
            Bitmap newImage =  (Bitmap) detectedImage.Clone();
            
            BlobCounter bc = new BlobCounter();
            bc.MinHeight = 5;
            bc.MinWidth = 5;
            bc.FilterBlobs = true;
            bc.ObjectsOrder = ObjectsOrder.Area;
            bc.ProcessImage(newImage);
            Rectangle[] rects = bc.GetObjectsRectangles();
            foreach (Rectangle recs in rects)
                if (rects.Length > 0)
                {
                    Rectangle objectRect = rects[0]; //= recs;
                    Graphics graph = Graphics.FromImage(tempImage);
                    using (Pen pen = new Pen(Color.FromArgb(255, 0, 0), 2))
                    {
                        graph.DrawRectangle(pen, objectRect);
                       
                    }
                    graph.Dispose();
                    pictureBox2.Image = tempImage;
                }
           

        }


        private void objectTracking(Bitmap srcImage, Bitmap filterImage)
        {
            detectedImage = filterImage;

            if (srcImage == null || detectedImage == null) return;
            //copy detected image to the new one
            Bitmap newImage = (Bitmap)detectedImage.Clone();
            //blob counter on the detected image
            BlobCounter bc = new BlobCounter();
            bc.MinHeight = 20;
            bc.MinWidth = 20;
            bc.FilterBlobs = true;
            bc.ObjectsOrder = ObjectsOrder.Area;
            bc.ProcessImage(newImage);
            Rectangle[] rects = bc.GetObjectsRectangles();
            foreach (Rectangle recs in rects)
            {
                if (rects.Length > 0)
                {
                    Rectangle objectRect = rects[0];
                    Graphics graph = Graphics.FromImage(srcImage);
                    using (Pen pen = new Pen(Color.FromArgb(0, 255, 0), 10))
                    {
                        graph.DrawRectangle(pen, objectRect);
                    }
                    graph.Dispose();
                }
            }
            //draw tracked object on picture box
            pictureBox2.Image = srcImage;
            
        }
        private void trackObject()
        {
            if (!cameraUsed)
            {
                if (pilChannel == 1)
                {
                    if (RGBImage == null) return;
                    track = RGBImage;
                }
                else if (pilChannel == 2)
                {
                    if (HSLImage == null) return;
                    track = HSLImage;
                }
                else if (pilChannel == 3)
                {
                    if (YCbCrImage == null) return;
                    track = YCbCrImage;
                }

                Bitmap tempImage = new Bitmap(sourceImage);
                pictureBox2.Image = tempImage;
                BlobCounter bc = new BlobCounter();
                bc.MinHeight = 5;
                bc.MinWidth = 5;
                bc.FilterBlobs = true;
                bc.ObjectsOrder = ObjectsOrder.Area;
                bc.ProcessImage(track);
                Rectangle[] rects = bc.GetObjectsRectangles();
                foreach (Rectangle recs in rects)
                    if (rects.Length > 0)
                    {
                        Rectangle objectRect = rects[0]; //= recs;
                        Graphics graph = Graphics.FromImage(tempImage);
                        using (Pen pen = new Pen(Color.FromArgb(0, 255, 0), 10))
                        {
                            graph.DrawRectangle(pen, objectRect);
                        }
                        graph.Dispose();
                    }
            }

        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                sourceImage = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);
                pictureBox3.Image = sourceImage;
                pictureBox1.Image = sourceImage;
                pictureBox2.Image = sourceImage;

                pictureBox3.BorderStyle = BorderStyle.FixedSingle;


                // ---- HISTOGRAM ----
                hitungHistogram(sourceImage);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenCamera();
            cameraUsed = true;

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        

        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = sourceImage;
            pictureBox2.Image = sourceImage;

            trackBarEnable(false);

            // init
            trackBar2.Maximum = 255;
            trackBar4.Maximum = 255;
            trackBar6.Maximum = 255;
            trackBar1.Minimum = 0;
            trackBar3.Minimum = 0;
            trackBar5.Minimum = 0;

            // trackbar reset
            trackBar1.Value = 0;
            trackBar3.Value = 0;
            trackBar5.Value = 0;
            trackBar2.Value = 255;
            trackBar4.Value = 255;
            trackBar6.Value = 255;



            // reset radio
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;

            // reset label
            labelReset();

            groupBox5.Text = "Image Control Trackbar"; 
        }

        

        private void radioButtonRGB()
        {
            radioButton2.Checked = false;
            radioButton3.Checked = false;
        }

        
        private void radioButtonHSL()
        {
            radioButton1.Checked = false;
            radioButton3.Checked = false;

        }

        

        private void radioButtonYCbCr()
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
        }

       

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

            pilChannel = 1;
            trackBarInit(pilChannel);
            trackBarReset(pilChannel);
            labelReset(pilChannel);
            trackBarEnable(true);
            groupBox5.Text = "RGB Image Control Function";
        }


        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

            pilChannel = 2;
            trackBarInit(pilChannel);
            trackBarReset(pilChannel);
            labelReset(pilChannel);
            trackBarEnable(true);
            groupBox5.Text = "HSL Image Control Function";
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            pilChannel = 3;
            trackBarInit(pilChannel);
            trackBarReset(pilChannel);
            labelReset(pilChannel);
            trackBarEnable(true);
            groupBox5.Text = "YCbCR Image Control Function";
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count != 0)
            {
                // add all devices to combo
                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);
                }
            }
            else
            {
                comboBox1.Items.Add("No DirectShow devices found");
            }
            comboBox1.SelectedIndex = 0;
        }

        private void hitungHistogram(Bitmap histogramImage)
        {
            if (histogramImage == null) return;



            ImageStatistics stat = new ImageStatistics(histogramImage);

            int[] redStat = stat.Red.Values;
            int[] greenStat = stat.Blue.Values;
            int[] blueStat = stat.Blue.Values;
            int[] gab = gabungHistogram(redStat, greenStat, blueStat);
            histogram1.Color = Color.White;
            histogram1.Values = gab;

        }

        int[] gabungHistogram(int[] r, int[] g, int[] b)
        {
            int[] c = new int[256 * 3];
            for (int i = 0; i < 256; i++)
                c[i] = r[i];
            for (int i = 256; i < 512; i++)
                c[i] = g[i - 256];
            for (int i = 512; i < 768; i++)
                c[i] = b[i - 512];
            return c;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            if (trackBar2.Value - trackBar1.Value <= TRACK_SPACE)
            {
                trackBar1.Value = trackBar2.Value - TRACK_SPACE;
            }

            if (pilChannel == 1)
            {
                Rmin = trackBar1.Value;
                label1.Text = string.Format("RMin : {0}", Rmin);
                RGBFiltering(sourceImage);
            }
            else if (pilChannel == 2)
            {
                Hmin = trackBar1.Value;
                label1.Text = string.Format("HueMin : {0}", Hmin);
                HSLFiltering(sourceImage);
            }
            else if (pilChannel == 3)
            {
                Ymin = (float)trackBar1.Value / 100;
                label1.Text = string.Format("Ymin : {0}", Ymin);
                YCbCrFiltering(sourceImage);
            }
            trackObject();
        }
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (trackBar2.Value - trackBar1.Value <= TRACK_SPACE)
            {
                trackBar2.Value = trackBar1.Value + TRACK_SPACE;
            }

            if (pilChannel == 1)
            {
                Rmax = trackBar2.Value;
                label2.Text = string.Format("RMax : {0}", Rmax);
                RGBFiltering(sourceImage);
            }
            else if (pilChannel == 2)
            {
                Hmax = trackBar2.Value;
                label2.Text = string.Format("HueMax : {0}", Hmax);
                HSLFiltering(sourceImage);
            }
            else if (pilChannel == 3)
            {
                Ymax = (float)trackBar2.Value / 100;
                label2.Text = string.Format("Ymax : {0}", Ymax);
                YCbCrFiltering(sourceImage);
            }
            trackObject();

        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            if (trackBar4.Value - trackBar3.Value <= TRACK_SPACE)
            {
                trackBar3.Value = trackBar4.Value - TRACK_SPACE;
            }
            if (pilChannel == 1)
            {
                Gmin = trackBar3.Value;
                label3.Text = string.Format("GMin : {0}", Gmin);
                RGBFiltering(sourceImage);
            }
            else if (pilChannel == 2)
            {
                Smin = (float)trackBar3.Value / 100; ;
                label3.Text = string.Format("SMin : {0}", Smin);
                HSLFiltering(sourceImage);
            }
            else if (pilChannel == 3)
            {
                Cbmin = (float)trackBar3.Value / 100;
                label3.Text = string.Format("Cbmin : {0}", Cbmin);
                YCbCrFiltering(sourceImage);
            }
            trackObject();
        }
        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            if (trackBar4.Value - trackBar3.Value <= TRACK_SPACE)
            {
                trackBar4.Value = trackBar3.Value + TRACK_SPACE;
            }
            if (pilChannel == 1)
            {
                Gmax = trackBar4.Value;
                label4.Text = string.Format("GMax : {0}", Gmax);
                RGBFiltering(sourceImage);
            }
            else if (pilChannel == 2)
            {
                Smax = (float)trackBar4.Value / 100;
                label4.Text = string.Format("SMax : {0}", Smax);
                HSLFiltering(sourceImage);
            }
            else if (pilChannel == 3)
            {
                Cbmax = (float)trackBar4.Value / 100;
                label4.Text = string.Format("Cbmax : {0}", Cbmax);
                YCbCrFiltering(sourceImage);
            }
            trackObject();
        }
        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            if (trackBar6.Value - trackBar5.Value <= TRACK_SPACE)
            {
                trackBar5.Value = trackBar6.Value - TRACK_SPACE;
            }
            if (pilChannel == 1)
            {
                Bmin = trackBar5.Value;
                label5.Text = string.Format("BMin : {0}", Bmin);
                RGBFiltering(sourceImage);
            }
            else if (pilChannel == 2)
            {
                Lmin = (float)trackBar5.Value / 100;
                label5.Text = string.Format("LMin : {0}", Lmin);
                HSLFiltering(sourceImage);
            }
            else if (pilChannel == 3)
            {
                Crmin = (float)trackBar5.Value / 100;
                label5.Text = string.Format("Crmin : {0}", Crmin);
                YCbCrFiltering(sourceImage);
            }
            trackObject();
        }
        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            if (trackBar6.Value - trackBar5.Value <= TRACK_SPACE)
            {
                trackBar6.Value = trackBar5.Value + TRACK_SPACE;
            }
            if (pilChannel == 1)
            {
                Bmax = trackBar6.Value;
                label6.Text = string.Format("BMax : {0}", Bmax);
                RGBFiltering(sourceImage);
            }
            else if (pilChannel == 2)
            {
                Lmax = (float)trackBar6.Value / 100;
                label6.Text = string.Format("LMax : {0}", Lmax);
                HSLFiltering(sourceImage);
            }
            else if (pilChannel == 3)
            {
                Crmax = (float)trackBar6.Value / 100;
                label6.Text = string.Format("Crmax : {0}", Crmax);
                YCbCrFiltering(sourceImage);
            }
            trackObject();
        }




        private void button5_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }
    }
}
