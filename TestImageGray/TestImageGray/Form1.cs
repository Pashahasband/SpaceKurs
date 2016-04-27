using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace TestImageGray
{
    public partial class Form1 : Form
    {
        /** forward transform scaling coefficients */
        static double h0, h1, h2, h3;
        /** forward transform wave coefficients */
        static double g0, g1, g2, g3;

        static double Ih0, Ih1, Ih2, Ih3;
        static double Ig0, Ig1, Ig2, Ig3;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Открываем файл картинки...
                System.IO.FileStream fs = new System.IO.FileStream(openFileDialog1.FileName, System.IO.FileMode.Open);
                System.Drawing.Image img = System.Drawing.Image.FromStream(fs);
                fs.Close();
                //Помещаем исходное изображение в PictureBox1
                pictureBox1.Image = img;

                var bmp = new Bitmap(img);
                label1.Text = "";
                label1.Text = label1.Text + DateTime.Now.Second.ToString() + "." + DateTime.Now.Millisecond.ToString() + " - "; //Время начала обработки (секунды и миллисекунды).
                //Преобразуем картинку
                MakeGray(bmp);
                label1.Text = label1.Text + DateTime.Now.Second.ToString() + "." + DateTime.Now.Millisecond.ToString(); //Время окончания обработки (секунды и миллисекунды).
                //Помещаем преобразованное изображение в PictureBox2
                pictureBox2.Image = bmp;
                
            }

        }


        static void HaarTransform(ref double[] array, int SIZE)
        {
            double[] avg = new double[SIZE];    // Суммы
            double[] diff = new double[SIZE];   // Разности

            for (int count = SIZE; count > 1; count /= 2)
            {
                for (int i = 0; i < count / 2; i++)
                {
                    avg[i] = (array[2 * i] + array[2 * i + 1]) / 2;
                    diff[i] = array[2 * i] - avg[i];
                }

                for (int i = 0; i < count / 2; i++)
                {
                    array[i] = avg[i];
                    array[i + count / 2] = diff[i];
                }
            }
        }
        static void transformDobes( double[] a, int n )
   {
      if (n >= 4) {
         int i, j;
         int half = n >> 1;

         double[] tmp = new double[n];

         i = 0;
         for (j = 0; j < n-3; j = j + 2) {
            tmp[i]      = a[j]*h0 + a[j+1]*h1 + a[j+2]*h2 + a[j+3]*h3;
            tmp[i+half] = a[j]*g0 + a[j+1]*g1 + a[j+2]*g2 + a[j+3]*g3;
            i++;
         }

         tmp[i]      = a[n-2]*h0 + a[n-1]*h1 + a[0]*h2 + a[1]*h3;
         tmp[i+half] = a[n-2]*g0 + a[n-1]*g1 + a[0]*g2 + a[1]*g3;

         for (i = 0; i < n; i++) {
            a[i] = tmp[i];
         }
      }
   } // transform
        static void TransformRow(ref double[,] data, int row, int SIZE)
        {
            double[] rowdata = new double[SIZE];
            int i;

            for (i = 0; i < SIZE; i++)
                rowdata[i] = data[row, i];

            HaarTransform(ref rowdata, SIZE);

            for (i = 0; i < SIZE; i++)
                data[row, i] = rowdata[i];
        }

        static void TransformColumn(ref double[,] data, int col, int SIZE)
        {
            double[] coldata = new double[SIZE];
            int i;

            for (i = 0; i < SIZE; i++)
                coldata[i] = data[i, col];

            HaarTransform(ref coldata, SIZE);

            for (i = 0; i < SIZE; i++)
                data[i, col] = coldata[i];
        }
        static void HaarTransformInverse(ref double[] array, int SIZE)
        {
            //int SIZE = array.GetLength(0);

            double[] tmp = new double[SIZE];
            int i, count;

            for (count = 2; count <= SIZE; count *= 2)
            {
                for (i = 0; i < count / 2; i++)
                {
                    tmp[2 * i] = array[i] + array[i + count / 2];
                    tmp[2 * i + 1] = array[i] - array[i + count / 2];
                }

                for (i = 0; i < count; i++) array[i] = tmp[i];
            }
        }

         static void invTransformDobes( double[] a, int n )
   {
      if (n >= 4) {
        int i, j;
        int half = n >> 1;
        int halfPls1 = half + 1;

        double[] tmp = new double[n];

        //      last smooth val  last coef.  first smooth  first coef
        tmp[0] = a[half-1]*Ih0 + a[n-1]*Ih1 + a[0]*Ih2 + a[half]*Ih3;
        tmp[1] = a[half-1]*Ig0 + a[n-1]*Ig1 + a[0]*Ig2 + a[half]*Ig3;
        j = 2;
        for (i = 0; i < half-1; i++) {
          //     smooth val     coef. val       smooth val     coef. val
          tmp[j++] = a[i]*Ih0 + a[i+half]*Ih1 + a[i+1]*Ih2 + a[i+halfPls1]*Ih3;
          tmp[j++] = a[i]*Ig0 + a[i+half]*Ig1 + a[i+1]*Ig2 + a[i+halfPls1]*Ig3;
        }
        for (i = 0; i < n; i++) {
          a[i] = tmp[i];
        }
      }
   }

        static void TransformColumnInverse(ref double[,] data, int col, int SIZE)
        {
            //int SIZE = data.GetLength(0);
            double[] coldata = new double[SIZE];
            int i;

            for (i = 0; i < SIZE; i++)
                coldata[i] = data[i, col];

            HaarTransformInverse(ref coldata, SIZE);

            for (i = 0; i < SIZE; i++)
                data[i, col] = coldata[i];
        }

        static void TransformRowInverse(ref double[,] data, int row, int SIZE)
        {
            //int SIZE = data.GetLength(0);

            double[] rowdata = new double[SIZE];
            int i;

            for (i = 0; i < SIZE; i++)
                rowdata[i] = data[row, i];

            HaarTransformInverse(ref rowdata, SIZE);

            for (i = 0; i < SIZE; i++)
                data[row, i] = rowdata[i];
        }
        private void MakeGray(Bitmap bmp)
        {
            // Задаём формат Пикселя.
            PixelFormat pxf = PixelFormat.Format24bppRgb;

            // Получаем данные картинки.
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            //Блокируем набор данных изображения в памяти
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

            // Получаем адрес первой точки.
            IntPtr ptr = bmpData.Scan0;
 
            // Задаём массив из Byte и помещаем в него набор данных.
            // int numBytes = bmp.Width * bmp.Height * 3; 
            //На 3 умножаем - поскольку RGB цвет кодируется 3-мя байтами
            //Либо используем вместо Width - Stride
            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            int kWidth = 0;
            int kHeight = 0;
            byte[] rgbValues = new byte[numBytes];
            byte[] rgbValuesnew = new byte[numBytes/4];
            double[] rValues = new double[bmp.Width * bmp.Height];
            double[] gValues = new double[bmp.Width * bmp.Height];
            double[] bValues = new double[bmp.Width * bmp.Height];
            double[] rValuesnew = new double[bmp.Width * bmp.Height/4];
            double[] gValuesnew = new double[bmp.Width * bmp.Height/4];
            double[] bValuesnew = new double[bmp.Width * bmp.Height/4];

            //double[,] masr = new double[bmp.Height, bmp.Width];
            //double[,] masg = new double[bmp.Height, bmp.Width];
            //double[,] masb = new double[bmp.Height, bmp.Width];
            //double[,] mascolor = new double[bmp.Height, bmpData.Stride];
            double sqrt_3 = Math.Sqrt(3);
            double denom = 4 * Math.Sqrt(2);

            //
            // forward transform scaling (smoothing) coefficients
            //
            h0 = (1 + sqrt_3) / denom;
            h1 = (3 + sqrt_3) / denom;
            h2 = (3 - sqrt_3) / denom;
            h3 = (1 - sqrt_3) / denom;
            //
            // forward transform wavelet coefficients
            //
            g0 = h3;
            g1 = -h2;
            g2 = h1;
            g3 = -h0;

            Ih0 = h2;
            Ih1 = g2;  // h1
            Ih2 = h0;
            Ih3 = g0;  // h3

            Ig0 = h3;
            Ig1 = g3;  // -h0
            Ig2 = h1;
            Ig3 = g1;  // -h2


            Marshal.Copy(ptr, rgbValues, 0, numBytes);

            

            // Перебираем пикселы по 3 байта на каждый и меняем значения
            /*for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                
                int value = rgbValues[counter] + rgbValues[counter + 1] + rgbValues[counter + 2];
                byte color_b = 0;        

                color_b = Convert.ToByte(value / 3);

                
                rgbValues[counter] = color_b;
                rgbValues[counter + 1] = color_b;
                rgbValues[counter + 2] = color_b;

            }
            */
            for (int counter = 0; counter < bmp.Width * bmp.Height; counter += 1)
            {
                rValues[counter] = rgbValues[counter*3];
                gValues[counter] = rgbValues[counter*3+1];
                bValues[counter] = rgbValues[counter*3+2];
            }
            for (int i = 0; i < 15; i++)
            {
                if (bmp.Width <= Math.Pow(2, i))
                {
                    kWidth = Convert.ToInt16(Math.Pow(2, i));
                    break;
                }
            }
            for (int i = 0; i < 15; i++)
            {
                if (bmp.Height <= Math.Pow(2, i))
                {
                    kHeight = Convert.ToInt16(Math.Pow(2, i));
                    break;
                }
            }

            double[] rValuesk = new double[kWidth * kHeight];
            double[] gValuesk = new double[kWidth * kHeight];
            double[] bValuesk = new double[kWidth * kHeight];

            for (int Height = 0; Height < kHeight; Height += 1)
            {
                for (int Width = 0; Width < kWidth; Width += 1)
                {
                    if ((Width < bmp.Width) && (Height < bmp.Height))
                    {
                        rValuesk[(Width + (Height * kWidth))] = rValues[(Width + (Height * bmp.Width))];
                        gValuesk[(Width + (Height * kWidth))] = gValues[(Width + (Height * bmp.Width))];
                        bValuesk[(Width + (Height * kWidth))] = bValues[(Width + (Height * bmp.Width))];
                    }
                    else
                    {
                        rValuesk[(Width + (Height * kWidth))] = 0;
                        gValuesk[(Width + (Height * kWidth))] = 0;
                        bValuesk[(Width + (Height * kWidth))] = 0;
                    }
                }
            }
           /* for (int Height = 0; Height < bmp.Height; Height += 1)             
            {
                for (int Width = 0; Width < bmp.Width; Width += 1)              
                {
                    masr[Height, Width] = rgbValues[(Width + (Height * bmp.Width)) * 3];
                    masg[Height, Width] = rgbValues[(Width + (Height * bmp.Width)) * 3 + 1];
                    masb[Height, Width] = rgbValues[(Width + (Height * bmp.Width)) * 3 + 2];
                }
            }
            // Применить преобразование к строкам...
            for (int i = 0; i < bmp.Height; i++)
            {
                TransformRow(ref masr, i, bmp.Width);
                TransformRow(ref masg, i, bmp.Width);
                TransformRow(ref masb, i, bmp.Width);
            }
            // ...и столбцам.
            for (int i = 0; i < bmp.Width; i++)
            {
                TransformColumn(ref masr, i, bmp.Height);
                TransformColumn(ref masg, i, bmp.Height);
                TransformColumn(ref masb, i, bmp.Height);
            }

            for (int Height = 0; Height < bmp.Height; Height += 1)
            {
                for (int Width = 0; Width < bmp.Width; Width += 1)
                {
                    if (Math.Abs(masr[Height,Width]) < 0.05)
                        masr[Height, Width] = 0;
                    if (Math.Abs(masg[Height, Width]) < 0.05)
                        masg[Height, Width] = 0;
                    if (Math.Abs(masb[Height, Width]) < 0.05)
                        masb[Height, Width] = 0;
                }
            }

            // Применить преобразование к строкам...
            for (int i = 0; i < bmp.Width; i++)
            {
                TransformColumnInverse(ref masr, i, bmp.Height);
                TransformColumnInverse(ref masg, i, bmp.Height);
                TransformColumnInverse(ref masb, i, bmp.Height);
            }

            // ...и столбцам.
            for (int i = 0; i < bmp.Height; i++)
            {
                TransformRowInverse(ref masr, i, bmp.Width);
                TransformRowInverse(ref masg, i, bmp.Width);
                TransformRowInverse(ref masb, i, bmp.Width);
            }
            */

            for (int n = kWidth * kHeight; n >= 4; n >>= 1)
            {
                transformDobes(rValuesk, n);
                transformDobes(gValuesk, n);
                transformDobes(bValuesk, n);
            }

            for (int counter = 0; counter < kWidth * kHeight; counter += 1)
            {
                if (Math.Abs(rValuesk[counter]) < 0.05)
                    rValuesk[counter] = 0;
                if (Math.Abs(gValuesk[counter]) < 0.05)
                    gValuesk[counter] = 0;
                if (Math.Abs(bValuesk[counter]) < 0.05)
                    bValuesk[counter] = 0;
            }

            for (int n = 4; n <= kWidth * kHeight; n <<= 1)
            {
                invTransformDobes(rValuesk, n);
                invTransformDobes(gValuesk, n);
                invTransformDobes(bValuesk, n);
            }

            for (int Height = 0; Height < kHeight; Height += 1)
            {
                for (int Width = 0; Width < kWidth; Width += 1)
                {
                    if ((Width < bmp.Width) && (Height < bmp.Height))
                    {
                        rValues[(Width + (Height * bmp.Width))] = rValuesk[(Width + (Height * kWidth))] ;
                        gValues[(Width + (Height * bmp.Width))] = gValuesk[(Width + (Height * kWidth))] ;
                        bValues[(Width + (Height * bmp.Width))] = bValuesk[(Width + (Height * kWidth))] ;
                    }
                    
                }
            }

            for (int counter = 0; counter < bmp.Width * bmp.Height; counter += 1)
            {
                rgbValues[counter * 3] = Convert.ToByte(rValues[counter]);
                rgbValues[counter * 3 + 1] = Convert.ToByte(gValues[counter]);
                rgbValues[counter * 3 + 2] = Convert.ToByte(bValues[counter]);
            }

            /*for (int n = bmp.Width * bmp.Height; n >= 4; n >>= 1)
            {
                transformDobes(rValues, n);
                transformDobes(gValues, n);
                transformDobes(bValues, n);
            }

            for (int counter = 0; counter < bmp.Width * bmp.Height; counter += 1)
            {
                if (Math.Abs(rValues[counter]) < 0.05)
                    rValues[counter] = 0;
                if (Math.Abs(gValues[counter]) < 0.05)
                    gValues[counter] = 0;
                if (Math.Abs(bValues[counter]) < 0.05)
                    bValues[counter] = 0;
            }

            for (int n = 4; n <= bmp.Width * bmp.Height; n <<= 1)
            {
                invTransformDobes(rValues, n);
                invTransformDobes(gValues, n);
                invTransformDobes(bValues, n);
            }

            for (int counter = 0; counter < bmp.Width * bmp.Height; counter += 1)
            {
                rgbValues[counter * 3] = Convert.ToByte(rValues[counter]);
                rgbValues[counter * 3 + 1] = Convert.ToByte(gValues[counter]);
                rgbValues[counter * 3 + 2] = Convert.ToByte(bValues[counter]);
            } */
            
            /*for (int Height = 0; Height < bmp.Height; Height += 1)
            {
                for (int Width = 0; Width < bmp.Width; Width += 1)
                {
                    rgbValues[(Width + (Height * bmp.Width)) * 3] = Convert.ToByte(masr[Height,Width]);
                    rgbValues[(Width + (Height * bmp.Width)) * 3 + 1] = Convert.ToByte(masg[Height, Width]);
                    rgbValues[(Width + (Height * bmp.Width)) * 3 + 2] = Convert.ToByte(masb[Height, Width]);
                }
            }

            /*
            for (int counter = 0; counter < rValuesnew.Length; counter += 1)
            {

                double value = rValues[counter * 4] + rValues[counter * 4 + 1] + rValues[counter * 4 + 2] + rValues[counter * 4 + 3];
                byte color_b = 0;
                color_b = Convert.ToByte(value / 4);
                rValuesnew[counter] = color_b;
                value = gValues[counter * 4] + gValues[counter * 4 + 1] + gValues[counter * 4 + 2] + gValues[counter * 4 + 3];
                color_b = 0;
                color_b = Convert.ToByte(value / 4);
                gValuesnew[counter] = color_b;
                value = bValues[counter * 4] + bValues[counter * 4 + 1] + bValues[counter * 4 + 2] + bValues[counter * 4 + 3];
                color_b = 0;
                color_b = Convert.ToByte(value / 4);
                bValuesnew[counter] = color_b;
            }
            for (int counter = 0; counter < rValuesnew.Length; counter += 1)
            {
                rgbValuesnew[counter * 3] = Convert.ToByte(rValuesnew[counter]);
                rgbValuesnew[counter * 3 + 1] = Convert.ToByte(gValuesnew[counter]);
                rgbValuesnew[counter * 3 + 2] = Convert.ToByte(bValuesnew[counter]);
            }
            */
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
           /* for (int Height = 0; Height < bmp.Height; Height += 1)
            {
                for (int Stride = 0; Stride < bmpData.Stride; Stride += 1)
                {
                    mascolor[Height, Stride] = rgbValues[Stride + (Height * bmpData.Stride)];
                }
            }

            // Применить преобразование к строкам...
            for (int i = 0; i < bmp.Height; i++)
                TransformRow(ref mascolor, i, bmpData.Stride);
            // ...и столбцам.
            for (int i = 0; i < bmpData.Stride; i++)
                TransformColumn(ref mascolor, i, bmp.Height);


            for (int Height = 0; Height < bmp.Height; Height += 1)
            {
                for (int Stride = 0; Stride < bmpData.Stride; Stride += 1)
                {
                    if (Math.Abs(mascolor[Height, Stride]) < 0.05)
                        mascolor[Height, Stride] = 0;
                }
            }

            // ...и столбцам.
            for (int i = 0; i < bmp.Height; i++)
                TransformRowInverse(ref mascolor, i, bmpData.Stride);
            // Применить преобразование к строкам...
            for (int i = 0; i < bmpData.Stride; i++)
                TransformColumnInverse(ref mascolor, i, bmp.Height);



            for (int Height = 0; Height < bmp.Height; Height += 1)
            {
                for (int Stride = 0; Stride < bmpData.Stride; Stride += 1)
                {
                    if (mascolor[Height, Stride] >= 0 && mascolor[Height, Stride] <= 256)
                        rgbValues[Stride + (Height * bmpData.Stride)] = Convert.ToByte(mascolor[Height, Stride]);
                    if (mascolor[Height, Stride] < 0)
                        rgbValues[Stride + (Height * bmpData.Stride)] = Convert.ToByte(mascolor[Height, Stride] + 128);
                    if (mascolor[Height, Stride] > 256)
                        rgbValues[Stride + (Height * bmpData.Stride)] = Convert.ToByte(mascolor[Height, Stride] - 128);
                }
            } 

            */
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Копируем набор данных обратно в изображение
            Marshal.Copy(rgbValues, 0, ptr, numBytes);
           // Marshal.Copy(rgbValuesnew, 0, ptr, numBytes/4);
            // Разблокируем набор данных изображения в памяти.
            bmp.UnlockBits(bmpData);
        }




    }
}
