using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;


namespace Laba1
{
    public partial class Form1 : Form
    {
        public string InputFileName = "";
        public string OutputFileName = "";


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
                startStop.Text = "Stop";
                FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
                FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
                FinalFrame.Start();
            }
            else
            {
                IsStart = !IsStart;
                startStop.Text = "Start";
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
                SaveFileDialog save = new SaveFileDialog();
                save.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
                save.Title = "Save file .bmp";
                save.DefaultExt = ".bmp";
                save.Filter = "File BMP|*.bmp";
                save.ShowDialog();
                OutputFileName = save.FileName;
                pictureBox1.Image.Save(OutputFileName, ImageFormat.Bmp);
                MessageBox.Show("Image saved successfully");
            }
            else MessageBox.Show("File not found!");
        }
        private void toTxt_Click(object sender, EventArgs e)
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

            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            save.Title = "Save file .bmp";
            save.DefaultExt = ".bmp";
            save.Filter = "File BMP|*.bmp";
            save.ShowDialog();

            OutputFileName = save.FileName;
            TxtToBmp();
            Image image = Image.FromFile(OutputFileName);
            pictureBox1.Image = image;
        }
        private void toBmp_Click(object sender, EventArgs e)
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

            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            save.Title = "Save file .txt";
            save.DefaultExt = ".txt";
            save.Filter = "File TXT|*.txt";
            save.ShowDialog();

            OutputFileName = save.FileName;
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
            OpenFileDialog open = new OpenFileDialog();
            open.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            open.Title = "Select a file .BMP";
            open.Multiselect = false;
            open.DefaultExt = ".bmp";
            open.Filter = "File BMP|*.bmp";
            open.FilterIndex = 0;

            open.ShowDialog();

            InputFileName = open.FileName;

            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            save.Title = "Save file .BMP";
            save.DefaultExt = ".bmp";
            save.Filter = "File BMP|*.bmp";
            save.ShowDialog();

            OutputFileName = save.FileName;
            Image image = Image.FromFile(InputFileName);
            Bitmap bmp = new Bitmap(image);
            pictureBox1.Image = bmp;

        }
    }
}
