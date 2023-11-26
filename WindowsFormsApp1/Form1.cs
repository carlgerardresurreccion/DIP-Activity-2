using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebCamLib;
using AForge.Video;
using AForge.Video.DirectShow;





namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        Bitmap loaded, processed;
        Bitmap imageB, imageA;
        private Device cam;
        bool isRunning = false;
        private Func<Bitmap, Bitmap> selectedFilter;
        private VideoCaptureDevice videoSource;
        private FilterInfoCollection videoDevices;




        public Form1()
        {
            InitializeComponent();

            

            
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            if (op.ShowDialog() == DialogResult.OK)
            {
                Image img = Image.FromFile(op.FileName);
                pictureBox1.Image = img;
                loaded = new Bitmap(img);
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isRunning == true)
            {

            }
            else
            {
                Color pixel;
                processed = new Bitmap(loaded.Width, loaded.Height);
                for (int x = 0; x < loaded.Width; x++)
                {
                    for (int y = 0; y < loaded.Height; y++)
                    {
                        pixel = loaded.GetPixel(x, y);
                        processed.SetPixel(x, y, pixel);
                    }
                }
            }
            pictureBox2.Image = processed;
        }

        private void grayScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            Color pixel;
            int grey;
            if (isRunning == true)
            {
                selectedFilter = ApplyGrayscaleFilter;
            }
            else
            {
                processed = new Bitmap(loaded.Width, loaded.Height);
                for (int x = 0; x < loaded.Width; x++)
                {
                    for (int y = 0; y < loaded.Height; y++)
                    {
                        pixel = loaded.GetPixel(x, y);
                        grey = (byte)((pixel.R + pixel.G + pixel.B) / 3);
                        processed.SetPixel(x, y, Color.FromArgb(grey, grey, grey));
                    }
                }
            }
            pictureBox2.Image = processed;
        }

        private void invertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color pixel;
            if (isRunning == true)
            {
                selectedFilter = ApplyInvertFilter;
            }
            else
            {
                processed = new Bitmap(loaded.Width, loaded.Height);
                for (int x = 0; x < loaded.Width; x++)
                {
                    for (int y = 0; y < loaded.Height; y++)
                    {
                        pixel = loaded.GetPixel(x, y);
                        processed.SetPixel(x, y, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
                    }
                }
            }
            pictureBox2.Image = processed;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color pixel;
            int a, r, g, b;
            if (isRunning == true)
            {
                selectedFilter = ApplySepiaFilter;
            }
            else if(pictureBox1 != null)
            {
                processed = new Bitmap(loaded.Width, loaded.Height);
                for (int x = 0; x < loaded.Width; x++)
                {
                    for (int y = 0; y < loaded.Height; y++)
                    {
                        pixel = loaded.GetPixel(x, y);
                        a = pixel.A;
                        r = pixel.R;
                        g = pixel.G;
                        b = pixel.B;

                        int tr = (int)(0.393 * r + 0.769 * g + 0.189 * b);
                        int tg = (int)(0.349 * r + 0.686 * g + 0.168 * b);
                        int tb = (int)(0.272 * r + 0.534 * g + 0.131 * b);


                        if (tr > 255)
                        {
                            r = 255;
                        }
                        else
                        {
                            r = tr;
                        }

                        if (tg > 255)
                        {
                            g = 255;
                        }
                        else
                        {
                            g = tg;
                        }

                        if (tb > 255)
                        {
                            b = 255;
                        }
                        else
                        {
                            b = tb;
                        }
                        processed.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                    }
                }
            }
            pictureBox2.Image = processed;


        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isRunning == true)
            {
                selectedFilter = ApplyHistoFilter;
            }
            else
            {
                processed = new Bitmap(loaded.Width, loaded.Height);
                Color pixel;
                int grey;
                for (int i = 0; i < loaded.Width; i++)
                {
                    for (int j = 0; j < loaded.Height; j++)
                    {
                        pixel = loaded.GetPixel(i, j);
                        grey = (pixel.R + pixel.G + pixel.B) / 3;
                        processed.SetPixel(i, j, Color.FromArgb(grey, grey, grey));
                    }
                }
                int[] hist = new int[256];

                Color sample;
                for (int i = 0; i < loaded.Width; i++)
                {
                    for (int j = 0; j < loaded.Height; j++)
                    {
                        sample = processed.GetPixel(i, j);
                        hist[sample.R] = hist[sample.R] + 1;
                    }
                }

                Bitmap histG = new Bitmap(256, 800);

                for (int i = 0; i < 256; i++)
                {
                    for (int j = 0; j < 800; j++)
                    {
                        histG.SetPixel(i, j, Color.White);
                    }
                }

                for (int i = 0; i < 256; i++)
                {
                    for (int j = 0; j < Math.Min(hist[i] / 5, 800); j++)
                    {
                        histG.SetPixel(i, 799 - j, Color.Black);
                    }
                }

                pictureBox2.Image = histG;
            }
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null && pictureBox2.Image == null)
            {

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Image Files(*.jpeg;*.bmp;*.png;)|*.jpeg;*.bmp;*.png;";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox2.Image.Save(saveFileDialog.FileName);
                }
            } else if(pictureBox3.Image != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Image Files(*.jpeg;*.bmp;*.png;)|*.jpeg;*.bmp;*.png;";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox3.Image.Save(saveFileDialog.FileName);
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (isRunning == true)
            {
                selectedFilter = ApplySubtractionFilter;
            }
            else
            {
                int maxWidth = Math.Max(imageB.Width, imageA.Width);
                int maxHeight = Math.Max(imageB.Width, imageA.Width);

                Bitmap resizedLoadImage = ResizeImage(imageB, maxWidth, maxHeight);
                Bitmap resizedBackgroundImage = ResizeImage(imageA, maxWidth, maxHeight);

                processed = new Bitmap(maxWidth, maxHeight);

                for (int x = 0; x < maxWidth; x++)
                {
                    for (int y = 0; y < maxHeight; y++)
                    {
                        Color pixelA = resizedLoadImage.GetPixel(x, y);
                        Color pixelB = resizedBackgroundImage.GetPixel(x, y);

                        int greenThreshold = 100;
                        if (pixelA.G > greenThreshold && pixelA.G > pixelA.R && pixelA.G > pixelA.B)
                        {
                            processed.SetPixel(x, y, pixelB);
                        }
                        else
                        {
                            processed.SetPixel(x, y, pixelA);
                        }
                    }
                }
            }
            pictureBox3.Image = processed;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;


        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            imageB = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = imageB;
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            imageA = new Bitmap(openFileDialog2.FileName);
            pictureBox2.Image = imageA;
        }

        private void StartWebcam()
        {
            
            try
            {
                if (videoSource == null)
                {
                    videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                    if (videoDevices.Count > 0)
                    {
                        videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                        videoSource.NewFrame += VideoSource_NewFrame;

                       
                        videoSource.Start();
                        isRunning = true;
                    }
                    else
                    {
                        MessageBox.Show("No video devices found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting webcam: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            
            pictureBox1.BeginInvoke((MethodInvoker)delegate
            {
                pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
            });

            if (selectedFilter != null)
            {
                pictureBox2.Invoke((MethodInvoker)delegate
                {
                    pictureBox2.Image = selectedFilter((Bitmap)eventArgs.Frame.Clone());
                });
            }
            else
            {
                pictureBox2.Invoke((MethodInvoker)delegate
                {
                    pictureBox2.Image = (Bitmap)eventArgs.Frame.Clone();
                });
            }
        }

        private void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (selectedFilter != null)
            {
                Bitmap webcamFrame = new Bitmap(eventArgs.Frame);
                pictureBox1.Image = selectedFilter(webcamFrame);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopWebcam();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StartWebcam();
            isRunning = true;
        }

        private void StopWebcam()
        {

            videoSource.SignalToStop();
            videoSource.WaitForStop();
            videoSource.NewFrame -= VideoSource_NewFrame;
            videoSource = null;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            if (isRunning)
            {
                StopWebcam();
                isRunning = false;
            }

            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private Bitmap ApplyFilter(Bitmap loaded)
        {
            return ApplyGrayscaleFilter(loaded);
        }


        private Bitmap ApplyGrayscaleFilter(Bitmap loaded)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);

            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    Color pixel = loaded.GetPixel(x, y);
                    int grey = (byte)((pixel.R + pixel.G + pixel.B) / 3);
                    processed.SetPixel(x, y, Color.FromArgb(grey, grey, grey));
                }
            }

            return processed;
        }

        private Bitmap ApplyInvertFilter(Bitmap loaded)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    Color pixel = loaded.GetPixel(x, y);
                    processed.SetPixel(x, y, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
                }
            }
            return processed;
        }

        private Bitmap ApplySepiaFilter(Bitmap loaded)
        {
            Color pixel;
            int a, r, g, b;
            
            processed = new Bitmap(loaded.Width, loaded.Height);
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    a = pixel.A;
                    r = pixel.R;
                    g = pixel.G;
                    b = pixel.B;

                    int tr = (int)(0.393 * r + 0.769 * g + 0.189 * b);
                    int tg = (int)(0.349 * r + 0.686 * g + 0.168 * b);
                    int tb = (int)(0.272 * r + 0.534 * g + 0.131 * b);


                    if (tr > 255)
                    {
                        r = 255;
                    }
                    else
                    {
                        r = tr;
                    }

                    if (tg > 255)
                    {
                        g = 255;
                    }
                    else
                    {
                        g = tg;
                    }

                    if (tb > 255)
                    {
                        b = 255;
                    }
                    else
                    {
                        b = tb;
                    }
                    processed.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }
            return processed;
        }

        private Bitmap ApplyHistoFilter(Bitmap loaded)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            int grey;
            for (int i = 0; i < loaded.Width; i++)
            {
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    grey = (pixel.R + pixel.G + pixel.B) / 3;
                    processed.SetPixel(i, j, Color.FromArgb(grey, grey, grey));
                }
            }
            int[] hist = new int[256];

            Color sample;
            for (int i = 0; i < loaded.Width; i++)
            {
                for (int j = 0; j < loaded.Height; j++)
                {
                    sample = processed.GetPixel(i, j);
                    hist[sample.R] = hist[sample.R] + 1;
                }
            }

            Bitmap histG = new Bitmap(256, 800);

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 800; j++)
                {
                    histG.SetPixel(i, j, Color.White);
                }
            }

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < Math.Min(hist[i] / 5, 800); j++)
                {
                    histG.SetPixel(i, 799 - j, Color.Black);
                }
            }

            return histG;
        }

        private Bitmap ApplySubtractionFilter(Bitmap loaded)
        {
            int maxWidth = Math.Max(imageB.Width, imageA.Width);
            int maxHeight = Math.Max(imageB.Width, imageA.Width);

            Bitmap resizedLoadImage = ResizeImage(imageB, maxWidth, maxHeight);
            Bitmap resizedBackgroundImage = ResizeImage(imageA, maxWidth, maxHeight);

            processed = new Bitmap(maxWidth, maxHeight);

            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    Color pixelA = resizedLoadImage.GetPixel(x, y);
                    Color pixelB = resizedBackgroundImage.GetPixel(x, y);

                    int greenThreshold = 100;
                    if (pixelA.G > greenThreshold && pixelA.G > pixelA.R && pixelA.G > pixelA.B)
                    {
                        processed.SetPixel(x, y, pixelB);
                    }
                    else
                    {
                        processed.SetPixel(x, y, pixelA);
                    }
                }
            }

            
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;

            return processed;
        }

    }
}