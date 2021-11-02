using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laba1
{
    class DraggedFragment
    {
        public Rectangle SourceRect;//фргмент в исходном изображении
        public Point Location;//положение сдвинутого фрагмента

        //прямоугольник сдвинутого фрагмента
        public Rectangle Rect
        {
            get { return new Rectangle(Location, SourceRect.Size); }
        }

        //фиксация изменений в исх изображении
        public void Fix(Image image)
        {
            using (var clone = (Image)image.Clone())
            using (var gr = Graphics.FromImage(image))
            {
                //рисуем выеразанное белое место
                gr.SetClip(SourceRect);
                gr.Clear(Color.White);

                //рисуем сдвинутый фрагмент
                gr.SetClip(Rect);
                gr.DrawImage(clone, Location.X - SourceRect.X, Location.Y - SourceRect.Y);
            }
        }
    }
}
