using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        public string InputFileName = "";  // Змінна для вхідного зображення
        public string OutputFileName = ""; // Змінна для вихідного зображення
        int rad;  // Змінна для параметру зглажування
        float brightness; // Змінна для параметру контрастування
        int w_b;
        int h_b;
        private Point mousePos1; // позиція мишки для паралельного перенесення
        private Point mousePos2;
        private DraggedFragment draggedFragment;
        public Form1()
        {
            InitializeComponent();
        }
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        private bool IsStart = false;

        private void pbImage_MouseMove(object sender, MouseEventArgs e) // реакція  на рух мишки
        {
            if (e.Button == MouseButtons.Left)
            {
                //юзер тягне фрагмент?
                if (draggedFragment != null)
                {
                    //зміщуємо фрагмент
                    draggedFragment.Location.Offset(e.Location.X - mousePos2.X, e.Location.Y - mousePos2.Y);
                    mousePos1 = e.Location;
                }
                //зміщуємо вибрану область
                mousePos2 = e.Location;
                pictureBox2.Invalidate();
            }
            else
            {
                mousePos1 = mousePos2 = e.Location;
            }
        } 
        private void pbImage_MouseDown(object sender, MouseEventArgs e) // реакція на те шо мишка відпущена
        {
            //юзер натиснув мишкою мимо фрагменту?
            if (draggedFragment != null && !draggedFragment.Rect.Contains(e.Location))
            {
                //знищуємо фрагмент
                draggedFragment = null;
                pictureBox2.Invalidate();
            }
        }
        private void pbImage_MouseUp(object sender, MouseEventArgs e)
        {
            //юзер виділив фрамент і відпустив мишку?
            if (mousePos1 != mousePos2)
            {
                //створюємо DraggedFragment
                var rect = GetRect(mousePos1, mousePos2);
                draggedFragment = new DraggedFragment() { SourceRect = rect, Location = rect.Location };
            }
            else
            {
                //користувач перемістив фрагмент і відпустив мишку?
                if (draggedFragment != null)
                {
                    //фіксуємо зміни в боксі
                    draggedFragment.Fix(pictureBox2.Image);
                    //знищуємо фрагмент
                    draggedFragment = null;
                    mousePos1 = mousePos2 = e.Location;
                }
            }
            pictureBox2.Invalidate();
        }
        private void pbImage_Paint(object sender, PaintEventArgs e)
        {
            //якщо існує виділений фрагмент
            if (draggedFragment != null)
            {
                //заміняємо виділене місце білим кольором
                e.Graphics.SetClip(draggedFragment.SourceRect);
                e.Graphics.Clear(Color.White);

                //малюємо виділений фрагмент
                e.Graphics.SetClip(draggedFragment.Rect);
                e.Graphics.DrawImage(pictureBox2.Image, draggedFragment.Location.X - draggedFragment.SourceRect.X, draggedFragment.Location.Y - draggedFragment.SourceRect.Y);

                //малюємо рамку
                e.Graphics.ResetClip();
                ControlPaint.DrawFocusRectangle(e.Graphics, draggedFragment.Rect);
            }
            else
            {
                //якщо виділена область
                if (mousePos1 != mousePos2)
                    ControlPaint.DrawFocusRectangle(e.Graphics, GetRect(mousePos1, mousePos2));//рисуем рамку
            }
        }

        Rectangle GetRect(Point p1, Point p2) // отримуємо координати фрагменту
        {
            var x1 = Math.Min(p1.X, p2.X);
            var x2 = Math.Max(p1.X, p2.X);
            var y1 = Math.Min(p1.Y, p2.Y);
            var y2 = Math.Max(p1.Y, p2.Y);
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialog = MessageBox.Show(
             "Are you sure you want to quit the program? ",
              "End of the program",
             MessageBoxButtons.YesNo,
             MessageBoxIcon.Warning
            );
            if (dialog == DialogResult.Yes)
            {
                if (IsStart)
                {
                    FinalFrame.Stop();
                }
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        } // реакція на закриття програми
        private void Form1_Load(object sender, EventArgs e)
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
            {
                comboBox1.Items.Add(Device.Name);
            }
            comboBox1.SelectedIndex = 0;
            FinalFrame = new VideoCaptureDevice();
        } // реакція на запуск програми
        private void ParalelTransformation_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = pictureBox1.Image;
        } // переносимо зображення з box1 в box2
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
        } // почати запис з відеокамери
        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        } // зберігаємо зображення в box1
        private void clear_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
        } // очищуємо bo1 та bo2
        private void savePicture_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                SavePicture();
            }
            else MessageBox.Show("Picture not found!");
        } // збереження зображення з box1
        private void OpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|JPeg Image|*.jpg";
            openFileDialog1.Title = "Choose an Image File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(@openFileDialog1.FileName);
            }
        } // відкриття файлу зображення та занесення його в box1 
        private void toTxt_Click(object sender, EventArgs e)
        {
            OpenTxtDialog();
            SaveBmpDialog();
            TxtToBmp();
            Image image = Image.FromFile(OutputFileName);
            pictureBox1.Image = image;
        } // конвертація зображення в txt формат
        private void toBmp_Click(object sender, EventArgs e)
        {
            OpenBmpDialog();
            SaveTxtDialog();
            Image image = Image.FromFile(InputFileName);
            pictureBox1.Image = image;
            BmpToTxt(pictureBox1.Image);
        } // конвертація зображення в bmp формат
        private void MedianFiltering_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null) // перевіряємо чи в box1 присутнє зображення
            {

                Bitmap my_bitmap = (Bitmap)pictureBox1.Image;
                int w_b = my_bitmap.Width;
                int h_b = my_bitmap.Height;
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
                    }

                }
                pictureBox2.Image = my_bitmap;
            }
            else MessageBox.Show("Picture not found!");
        } // медіанна фільтрація
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
                    brightness = 2;
                }
                Bitmap my_bitmap = (Bitmap)pictureBox1.Image;
                pictureBox2.Image = LinearContrasting(my_bitmap);
            }
            else MessageBox.Show("Picture not found!");
        }
        private void Imposition_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                OpenBmpDialog();
                Bitmap OverlayBitmap = (Bitmap)Bitmap.FromFile(InputFileName);
                Bitmap TargetBitmap = (Bitmap)pictureBox1.Image;
                pictureBox2.Image = Imposition(TargetBitmap, OverlayBitmap);
            }
            else MessageBox.Show("Picture not found!");
        }
        private void buttonScale_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                try
                {
                    w_b = System.Convert.ToInt32(toolStripTextBox3.Text);
                    h_b = System.Convert.ToInt32(toolStripTextBox4.Text);
                }
                catch
                {
                    w_b = 847;
                    h_b = 399;
                }
                Bitmap my_bitmap = (Bitmap)pictureBox1.Image;
                pictureBox2.Image = ResizeImage(my_bitmap, w_b, h_b);
            }
            else MessageBox.Show("Picture not found!");
        }
        private void rotate_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                try
                {
                    rad = System.Convert.ToInt32(toolStripTextBox5.Text);
                }
                catch
                {
                    rad = 90;
                }
                Bitmap my_bitmap = (Bitmap)pictureBox1.Image;
                pictureBox2.Image = RotateImage(my_bitmap, rad);
            }
            else MessageBox.Show("Picture not found!");
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
            open.Filter = "Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|JPeg Image|*.jpg";
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
        private Bitmap RotateImage(Image sourceImage, int rad)
        {
            var rotateImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            rotateImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            using (Graphics gdi = Graphics.FromImage(rotateImage))
            {
                //Rotate the image
                gdi.TranslateTransform((float)sourceImage.Width / 2, (float)sourceImage.Height / 2);
                gdi.RotateTransform(rad);
                gdi.TranslateTransform(-(float)sourceImage.Width / 2, -(float)sourceImage.Height / 2);
                gdi.DrawImage(sourceImage, new System.Drawing.Point(0, 0));
            }

            return rotateImage;
        }
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
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
        private Image Imposition(Bitmap TargetBitmap, Bitmap OverlayBitmap)
        {
            Bitmap ResultBitmap = new Bitmap(TargetBitmap.Width, TargetBitmap.Height, PixelFormat.Format32bppArgb);
            Graphics graph = Graphics.FromImage(ResultBitmap);
            graph.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            graph.DrawImage(TargetBitmap, 0, 0);
            graph.DrawImage(OverlayBitmap, (TargetBitmap.Width - OverlayBitmap.Width) / 2,
                (TargetBitmap.Height - OverlayBitmap.Height) / 2, OverlayBitmap.Width, OverlayBitmap.Height);
            return ResultBitmap;
        }

    }
}