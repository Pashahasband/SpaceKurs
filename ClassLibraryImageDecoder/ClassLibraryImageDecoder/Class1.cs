using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibraryImageDecoder
{
            /** forward transform scaling coefficients */
        static double h0, h1, h2, h3;
        /** forward transform wave coefficients */
        static double g0, g1, g2, g3;

        static double Ih0, Ih1, Ih2, Ih3;
        static double Ig0, Ig1, Ig2, Ig3;
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

            // Копируем набор данных обратно в изображение
            Marshal.Copy(rgbValues, 0, ptr, numBytes);
            // Разблокируем набор данных изображения в памяти.
            bmp.UnlockBits(bmpData);
        }
    public class Class1
    {

    }
}
