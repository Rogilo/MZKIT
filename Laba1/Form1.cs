﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;


namespace Laba1
{
    public partial class Form1 : Form
    {
        public string InputFileName = "";
        public string OutputFileName = "";
        int rad;
        float brightness;
        public Form1()
        {
            InitializeComponent();
        }
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        private bool IsStart = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
            {
                comboBox1.Items.Add(Device.Name);
            }
            comboBox1.SelectedIndex = 0;
            FinalFrame = new VideoCaptureDevice();
        }

        private void startStop_Click(object sender, EventArgs e)
        {
            if (!IsStart)
            {
                IsStart = !IsStart;
                FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
                FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
                FinalFrame.Start();
            }
            else
            {
                IsStart = !IsStart;
                FinalFrame.Stop();
            }
        }
        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        private void clear_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
        }
        private void savePicture_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                SavePicture();
            }
            else MessageBox.Show("Picture not found!");
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|JPeg Image|*.jpg";
            openFileDialog1.Title = "Choose an Image File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(@openFileDialog1.FileName);
            }
        }

        private void SavePicture()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            save.Title = "Save file .bmp";
            save.DefaultExt = ".bmp";
            save.Filter = "File BMP|*.bmp";
            save.ShowDialog();
            OutputFileName = save.FileName;
            pictureBox1.Image.Save(OutputFileName, ImageFormat.Bmp);

        }
        private void OpenTxtDialog()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            open.Title = "Select a file .txt";
            open.Multiselect = false;
            open.DefaultExt = ".txt";
            open.Filter = "File TXT|*.txt";
            open.FilterIndex = 0;
            open.ShowDialog();
            InputFileName = open.FileName;
        }
        private void OpenBmpDialog()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            open.Title = "Select a file .BMP";
            open.Multiselect = false;
            open.DefaultExt = ".bmp";
            open.Filter = "File BMP|*.bmp";
            open.FilterIndex = 0;
            open.ShowDialog();
            InputFileName = open.FileName;

        }
        private void SaveBmpDialog()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            save.Title = "Save file .bmp";
            save.DefaultExt = ".bmp";
            save.Filter = "File BMP|*.bmp";
            save.ShowDialog();

            OutputFileName = save.FileName;
        }
        private void SaveTxtDialog()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            save.Title = "Save file .txt";
            save.DefaultExt = ".txt";
            save.Filter = "File TXT|*.txt";
            save.ShowDialog();
            OutputFileName = save.FileName;
        }
        private void toTxt_Click(object sender, EventArgs e)
        {
            OpenTxtDialog();
            SaveBmpDialog();
            TxtToBmp();
            Image image = Image.FromFile(OutputFileName);
            pictureBox1.Image = image;
        }
        private void toBmp_Click(object sender, EventArgs e)
        {
            OpenBmpDialog();
            SaveTxtDialog();
            Image image = Image.FromFile(InputFileName);
            pictureBox1.Image = image;
            BmpToTxt(pictureBox1.Image);
        }
        private void BmpToTxt(Image image)
        {
            string stringPartOfColor;
            Cursor = Cursors.WaitCursor;
            using (Bitmap bmp = new Bitmap(image))
            {
                using (StreamWriter sw = new StreamWriter(OutputFileName, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(bmp.Width);
                    sw.WriteLine(bmp.Height);

                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            Color pxl = bmp.GetPixel(x, y);
                            stringPartOfColor = Convert.ToString(pxl.R, 2).PadLeft(8, '0');
                            sw.WriteLine(stringPartOfColor);
                            stringPartOfColor = Convert.ToString(pxl.G, 2).PadLeft(8, '0');
                            sw.WriteLine(stringPartOfColor);
                            stringPartOfColor = Convert.ToString(pxl.B, 2).PadLeft(8, '0');
                            sw.WriteLine(stringPartOfColor);
                        }
                    }
                    sw.Close();
                }
            }
            Cursor = Cursors.Default;
        }
        private void TxtToBmp()
        {
            Cursor = Cursors.WaitCursor;
            Stream imageStream = new MemoryStream();
            int width, height;
            int r, g, b;

            using (StreamReader sr = new StreamReader(InputFileName, System.Text.Encoding.Default))
            {
                width = Convert.ToInt16(sr.ReadLine());
                height = Convert.ToInt16(sr.ReadLine());
                using (Bitmap bmp = new Bitmap(width, height))
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            r = Convert.ToInt32(sr.ReadLine(), 2);
                            g = Convert.ToInt32(sr.ReadLine(), 2);
                            b = Convert.ToInt32(sr.ReadLine(), 2);
                            bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                        }
                    }
                    bmp.Save(OutputFileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                sr.Close();
            }
            Cursor = Cursors.Default;

        }

        private void MedianFiltering_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {

                Bitmap my_bitmap = (Bitmap)pictureBox1.Image;
                int w_b = my_bitmap.Width;
                int h_b = my_bitmap.Height;

                toolStripProgressBar1.Step = 1;
                toolStripProgressBar1.Maximum = w_b * h_b;
                try
                {
                    rad = System.Convert.ToInt32(toolStripTextBox1.Text);
                }
                catch
                {
                    rad = 1;
                }

                for (int x = rad + 1; x < w_b - rad; x++)
                {
                    for (int y = rad + 1; y < h_b - rad; y++)
                    {
                        Median_filter(my_bitmap, x, y);
                        toolStripProgressBar1.PerformStep();

                    }

                }
                pictureBox1.Refresh();
                toolStripProgressBar1.Value = 0;
            }
            else MessageBox.Show("Picture not found!");
        }
        private void LinearContrasting_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                try
                {
                    brightness = System.Convert.ToInt32(toolStripTextBox2.Text);
                }
                catch
                {
                    brightness = 1;
                }
                Bitmap my_bitmap = (Bitmap)pictureBox1.Image;
                pictureBox1.Image = LinearContrasting(my_bitmap);
            }
            else MessageBox.Show("Picture not found!");
        }
        private void Median_filter(Bitmap my_bitmap, int x, int y)
        {
            int n;
            int cR_, cB_, cG_;
            int k = 1;

            n = (2 * rad + 1) * (2 * rad + 1);
            int[] cR = new int[n + 1];
            int[] cB = new int[n + 1];
            int[] cG = new int[n + 1];

            for (int i = 0; i < n + 1; i++)
            {
                cR[i] = 0;
                cG[i] = 0;
                cB[i] = 0;
            }

            int w_b = my_bitmap.Width;
            int h_b = my_bitmap.Height;

            for (int i = x - rad; i < x + rad + 1; i++)
            {
                for (int j = y - rad; j < y + rad + 1; j++)
                {

                    System.Drawing.Color c = my_bitmap.GetPixel(i, j);
                    cR[k] = System.Convert.ToInt32(c.R);
                    cG[k] = System.Convert.ToInt32(c.G);
                    cB[k] = System.Convert.ToInt32(c.B);
                    k++;
                }
            }

            Quicksort(cR, 0, n - 1);
            Quicksort(cG, 0, n - 1);
            Quicksort(cB, 0, n - 1);

            int n_ = (int)(n / 2) + 1;

            cR_ = cR[n_];
            cG_ = cG[n_];
            cB_ = cB[n_];

            my_bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(cR_, cG_, cB_));
        }
        private int Partition(int[] a, int p, int r)
        {
            int x = a[r];
            int i = p - 1;
            int tmp;
            for (int j = p; j < r; j++)
            {
                if (a[j] <= x)
                {
                    i++;
                    tmp = a[i];
                    a[i] = a[j];
                    a[j] = tmp;
                }
            }
            tmp = a[r];
            a[r] = a[i + 1];
            a[i + 1] = tmp;
            return (i + 1);

        }
        private void Quicksort(int[] a, int p, int r)
        {
            if (p < r)
            {
                int q = Partition(a, p, r);
                Quicksort(a, p, q - 1);
                Quicksort(a, q + 1, r);
            }
        }
        private Image LinearContrasting(Bitmap image) 
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            int width = image.Width;
            int height = image.Height;

            float[][] colorMatrixElements = {
            new float[] {brightness, 0, 0, 0, 0},
            new float[] {0, brightness, 0, 0, 0},
            new float[] {0, 0, brightness, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1}
            };
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            imageAttributes.SetColorMatrix(
                colorMatrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);
            Graphics graphics = Graphics.FromImage(image);
            graphics.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, width,
                height,
                GraphicsUnit.Pixel,
                imageAttributes);
            return image;
        }
     
    }
}