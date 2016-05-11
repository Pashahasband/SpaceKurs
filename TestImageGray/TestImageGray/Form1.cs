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

   static void transformDobes(ref double[] a, int n )
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


   static void invTransformDobes(ref double[] a, int n )
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
            double[] rValues = new double[bmp.Width * bmp.Height];
            double[] gValues = new double[bmp.Width * bmp.Height];
            double[] bValues = new double[bmp.Width * bmp.Height];


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


            //разбираем картинку на 3 матрицы ргб
            for (int counter = 0; counter < bmp.Width * bmp.Height; counter += 1)
            {
                rValues[counter] = rgbValues[counter * 3];
                gValues[counter] = rgbValues[counter * 3 + 1];
                bValues[counter] = rgbValues[counter * 3 + 2];
            }
            //нахождение ближайшей степени 2ки для ширины
            for (int i = 0; i < 15; i++)
            {
                if (bmp.Width <= Math.Pow(2, i))
                {
                    kWidth = Convert.ToInt16(Math.Pow(2, i));
                    break;
                }
            }
            //нахождение ближайшей степени 2ки для высоты
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

            double[,] masrk = new double[kHeight, kWidth];
            double[,] masgk = new double[kHeight, kWidth];
            double[,] masbk = new double[kHeight, kWidth];

            double[,] masrkk = new double[kHeight, kWidth];
            double[,] masgkk = new double[kHeight, kWidth];
            double[,] masbkk = new double[kHeight, kWidth];
            //преобразование размера к степени 2ки
            for (int Height = 0; Height < kHeight; Height += 1)
            {
                for (int Width = 0; Width < kWidth; Width += 1)
                {
                    if ((Width < bmp.Width) && (Height < bmp.Height))
                    {
                        masrk[Height, Width] = rValuesk[(Width + (Height * kWidth))] = rValues[(Width + (Height * bmp.Width))];
                        masgk[Height, Width] = gValuesk[(Width + (Height * kWidth))] = gValues[(Width + (Height * bmp.Width))];
                        masbk[Height, Width] = bValuesk[(Width + (Height * kWidth))] = bValues[(Width + (Height * bmp.Width))];
                    }
                    else
                    {
                        masrk[Height, Width] = rValuesk[(Width + (Height * kWidth))] = 0;
                        masgk[Height, Width] = gValuesk[(Width + (Height * kWidth))] = 0;
                        masbk[Height, Width] = bValuesk[(Width + (Height * kWidth))] = 0;
                    }
                }
            }


            double[] rValueskk = new double[kWidth];
            double[] gValueskk = new double[kWidth];
            double[] bValueskk = new double[kWidth];
            double[] rValueskkk = new double[kHeight];
            double[] gValueskkk = new double[kHeight];
            double[] bValueskkk = new double[kHeight];
            //Прямое преобразование Добеши
            // Применить преобразование к строкам...
            for (int i = 0; i < kHeight; i++)
            {
                for (int j = 0; j < kWidth; j++)
                {
                    rValueskk[j] = masrk[i, j];
                    gValueskk[j] = masgk[i, j];
                    bValueskk[j] = masbk[i, j];
                }

                for (int n = kWidth; n >= 4; n >>= 1)
                {
                    transformDobes(ref rValueskk, n);
                    transformDobes(ref gValueskk, n);
                    transformDobes(ref bValueskk, n);
                }

                for (int j = 0; j < kWidth; j++)
                {
                    masrk[i, j] = rValueskk[j];
                    masgk[i, j] = gValueskk[j];
                    masbk[i, j] = bValueskk[j];
                    rValueskk[j] = 0;
                    gValueskk[j] = 0;
                    bValueskk[j] = 0;
                }
            }
            // ...и столбцам.
            for (int i = 0; i < kWidth; i++)
            {
                for (int j = 0; j < kHeight; j++)
                {
                    rValueskkk[j] = masrk[j, i];
                    gValueskkk[j] = masgk[j, i];
                    bValueskkk[j] = masbk[j, i];
                }

                for (int n = kHeight; n >= 4; n >>= 1)
                {
                    transformDobes(ref rValueskkk, n);
                    transformDobes(ref gValueskkk, n);
                    transformDobes(ref bValueskkk, n);
                }

                for (int j = 0; j < kHeight; j++)
                {
                    masrk[j, i] = rValueskkk[j];
                    masgk[j, i] = gValueskkk[j];
                    masbk[j, i] = bValueskkk[j];
                    rValueskk[j] = 0;
                    gValueskk[j] = 0;
                    bValueskk[j] = 0;
                }
            }

            //Провекра коэффициентов
            for (int i = 0; i < kHeight; i++)
            {
                for (int j = 0; j < kWidth; j++)
                {
                    if (Math.Abs(masrk[i, j]) < 0.5)
                        masrk[i, j] = 0;
                    if (Math.Abs(masgk[i, j]) < 0.5)
                        masgk[i, j] = 0;
                    if (Math.Abs(masbk[i, j]) < 0.5)
                        masbk[i, j] = 0;
                }
            }
            ///Дибильный переворот изображения
            /*for (int i = 0; i < kHeight / 2; i++)
            {
                for (int j = 0; j < kWidth / 2; j++)
                {
                    masrkk[i, j] = masrk[i * 2, j * 2];
                    masgkk[i, j] = masgk[i * 2, j * 2];
                    masbkk[i, j] = masbk[i * 2, j * 2];
                }
            }

            for (int i = kHeight / 2; i < kHeight; i++)
            {
                for (int j = 0; j < kWidth / 2; j++)
                {
                    masrkk[i, j] = masrk[(i - kHeight / 2) * 2 + 1, j * 2];
                    masgkk[i, j] = masgk[(i - kHeight / 2) * 2 + 1, j * 2];
                    masbkk[i, j] = masbk[(i - kHeight / 2) * 2 + 1, j * 2];
                }
            }

            for (int i = 0; i < kHeight / 2; i++)
            {
                for (int j = kWidth / 2; j < kWidth; j++)
                {
                    masrkk[i, j] = masrk[i * 2, (j - kWidth / 2) * 2 + 1];
                    masgkk[i, j] = masgk[i * 2, (j - kWidth / 2) * 2 + 1];
                    masbkk[i, j] = masbk[i * 2, (j - kWidth / 2) * 2 + 1];
                }
            }
            for (int i = kHeight / 2; i < kHeight; i++)
            {
                for (int j = kWidth / 2; j < kWidth; j++)
                {
                    masrkk[i, j] = masrk[(i - kHeight / 2) * 2 + 1, (j - kWidth / 2) * 2 + 1];
                    masgkk[i, j] = masgk[(i - kHeight / 2) * 2 + 1, (j - kWidth / 2) * 2 + 1];
                    masbkk[i, j] = masbk[(i - kHeight / 2) * 2 + 1, (j - kWidth / 2) * 2 + 1];
                }
            }*/
            ////конец дибильного переворота
            //Обратное преобразование Добеши
            // Применить преобразование к строкам...
             for (int i = 0; i < kHeight; i++)
             {
                 for (int j = 0; j < kWidth; j++)
                 {
                     rValueskk[j] = masrk[i, j];
                     gValueskk[j] = masgk[i, j];
                     bValueskk[j] = masbk[i, j];
                 }
                 for (int n = 4; n <= kWidth ; n <<= 1)
                 {
                     invTransformDobes(ref rValueskk, n);
                     invTransformDobes(ref gValueskk, n);
                     invTransformDobes(ref bValueskk, n);
                 }

                 for (int j = 0; j < kWidth; j++)
                 {
                     masrk[i, j] = rValueskk[j];
                     masgk[i, j] = gValueskk[j];
                     masbk[i, j] = bValueskk[j];
                     rValueskk[j] = 0;
                     gValueskk[j] = 0;
                     bValueskk[j] = 0;
                 }
             }
             // ...и столбцам.
             for (int i = 0; i < kWidth; i++)
             {
                 for (int j = 0; j < kHeight; j++)
                 {
                     rValueskkk[j] = masrk[j, i];
                     gValueskkk[j] = masgk[j, i];
                     bValueskkk[j] = masbk[j, i];
                 }

                 for (int n = 4; n <= kHeight; n <<= 1)
                 {
                     invTransformDobes(ref rValueskkk, n);
                     invTransformDobes(ref gValueskkk, n);
                     invTransformDobes(ref bValueskkk, n);
                 }

                 for (int j = 0; j < kHeight; j++)
                 {
                     masrk[j, i] = rValueskkk[j];
                     masgk[j, i] = gValueskkk[j];
                     masbk[j, i] = bValueskkk[j];
                     rValueskk[j] = 0;
                     gValueskk[j] = 0;
                     bValueskk[j] = 0;
                 }
             }
             
            
            //преобразование к исходному размеру
            for (int Height = 0; Height < kHeight; Height += 1)
            {
                for (int Width = 0; Width < kWidth; Width += 1)
                {
                    if ((Width < bmp.Width) && (Height < bmp.Height))
                    {
                        if (masrk[Height, Width] > 0)
                            if (masrk[Height, Width] < 255)
                                rValues[(Width + (Height * bmp.Width))] = rValuesk[(Width + (Height * kWidth))] = masrk[Height, Width];
                            else
                                rValues[(Width + (Height * bmp.Width))] = rValuesk[(Width + (Height * kWidth))] = 255;
                        else
                            rValues[(Width + (Height * bmp.Width))] = rValuesk[(Width + (Height * kWidth))] = 0;
                        if (masgk[Height, Width] > 0)
                            if (masgk[Height, Width] < 255)
                                gValues[(Width + (Height * bmp.Width))] = gValuesk[(Width + (Height * kWidth))] = masgk[Height, Width];
                            else
                                gValues[(Width + (Height * bmp.Width))] = gValuesk[(Width + (Height * kWidth))] = 255;
                        else
                            gValues[(Width + (Height * bmp.Width))] = gValuesk[(Width + (Height * kWidth))] = 0;
                        if (masbk[Height, Width] > 0)
                            if (masbk[Height, Width] < 255)
                                bValues[(Width + (Height * bmp.Width))] = bValuesk[(Width + (Height * kWidth))] = masbk[Height, Width];
                            else
                                bValues[(Width + (Height * bmp.Width))] = bValuesk[(Width + (Height * kWidth))] = 255;
                        else
                            bValues[(Width + (Height * bmp.Width))] = bValuesk[(Width + (Height * kWidth))] = 0;
                    }

                }
            }
            //конец преобразования

            //сборка в единое ргб изображение
            for (int counter = 0; counter < bmp.Width * bmp.Height; counter += 1)
            {
                rgbValues[counter * 3] = Convert.ToByte(rValues[counter]);
                rgbValues[counter * 3 + 1] = Convert.ToByte(gValues[counter]);
                rgbValues[counter * 3 + 2] = Convert.ToByte(bValues[counter]);
            }



            // Копируем набор данных обратно в изображение
            Marshal.Copy(rgbValues, 0, ptr, numBytes);
            // Разблокируем набор данных изображения в памяти.
            bmp.UnlockBits(bmpData);
        }




    }
}
