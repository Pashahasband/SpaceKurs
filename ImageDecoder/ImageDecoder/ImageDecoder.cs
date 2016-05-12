namespace ImageDecoder
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    public class ImageDecoder
    {
        static void TransformDobes(ref double[] a, int n, double h0, double h1, double h2, double h3, double g0, double g1, double g2, double g3)
        {
            if (n >= 4)
            {
                int i, j;
                int half = n >> 1;

                double[] tmp = new double[n];

                i = 0;
                for (j = 0; j < n - 3; j = j + 2)
                {
                    tmp[i] = a[j] * h0 + a[j + 1] * h1 + a[j + 2] * h2 + a[j + 3] * h3;
                    tmp[i + half] = a[j] * g0 + a[j + 1] * g1 + a[j + 2] * g2 + a[j + 3] * g3;
                    i++;
                }

                tmp[i] = a[n - 2] * h0 + a[n - 1] * h1 + a[0] * h2 + a[1] * h3;
                tmp[i + half] = a[n - 2] * g0 + a[n - 1] * g1 + a[0] * g2 + a[1] * g3;

                for (i = 0; i < n; i++)
                {
                    a[i] = tmp[i];
                }
            }
        } // transform

        static void InvTransformDobes(ref double[] a, int n, double Ih0, double Ih1, double Ih2, double Ih3, double Ig0, double Ig1, double Ig2, double Ig3)
        {
            if (n >= 4)
            {
                int i, j;
                int half = n >> 1;
                int halfPls1 = half + 1;

                double[] tmp = new double[n];

                //      last smooth val  last coef.  first smooth  first coef
                tmp[0] = a[half - 1] * Ih0 + a[n - 1] * Ih1 + a[0] * Ih2 + a[half] * Ih3;
                tmp[1] = a[half - 1] * Ig0 + a[n - 1] * Ig1 + a[0] * Ig2 + a[half] * Ig3;
                j = 2;
                for (i = 0; i < half - 1; i++)
                {
                    //     smooth val     coef. val       smooth val     coef. val
                    tmp[j++] = a[i] * Ih0 + a[i + half] * Ih1 + a[i + 1] * Ih2 + a[i + halfPls1] * Ih3;
                    tmp[j++] = a[i] * Ig0 + a[i + half] * Ig1 + a[i + 1] * Ig2 + a[i + halfPls1] * Ig3;
                }
                for (i = 0; i < n; i++)
                {
                    a[i] = tmp[i];
                }
            }
        }

        public void MakeGray(Bitmap bmp)
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
            /** forward transform scaling coefficients */
            double h0, h1, h2, h3;
            /** forward transform wave coefficients */
            double g0, g1, g2, g3;

            double Ih0, Ih1, Ih2, Ih3;
            double Ig0, Ig1, Ig2, Ig3;

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

            double[,] masrkk = new double[bmp.Height, bmp.Width];
            double[,] masgkk = new double[bmp.Height, bmp.Width];
            double[,] masbkk = new double[bmp.Height, bmp.Width];
            //преобразование размера к степени 2ки
            for (int h = 0; h < kHeight; h++)
            {
                for (int w = 0; w < kWidth; w++)
                {
                    if ((w < bmp.Width) && (h < bmp.Height))
                    {
                        masrk[h, w] = rValuesk[(w + (h * kWidth))] = rValues[(w + (h * bmp.Width))];
                        masgk[h, w] = gValuesk[(w + (h * kWidth))] = gValues[(w + (h * bmp.Width))];
                        masbk[h, w] = bValuesk[(w + (h * kWidth))] = bValues[(w + (h * bmp.Width))];
                    }
                    else
                    {
                        masrk[h, w] = rValuesk[(w + (h * kWidth))] = 0;
                        masgk[h, w] = gValuesk[(w + (h * kWidth))] = 0;
                        masbk[h, w] = bValuesk[(w + (h * kWidth))] = 0;
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
                    TransformDobes(ref rValueskk, n, h0, h1, h2, h3, g0, g1, g2, g3);
                    TransformDobes(ref gValueskk, n, h0, h1, h2, h3, g0, g1, g2, g3);
                    TransformDobes(ref bValueskk, n, h0, h1, h2, h3, g0, g1, g2, g3);
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
                    TransformDobes(ref rValueskkk, n, h0, h1, h2, h3, g0, g1, g2, g3);
                    TransformDobes(ref gValueskkk, n, h0, h1, h2, h3, g0, g1, g2, g3);
                    TransformDobes(ref bValueskkk, n, h0, h1, h2, h3, g0, g1, g2, g3);
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
                for (int n = 4; n <= kWidth; n <<= 1)
                {
                    InvTransformDobes(ref rValueskk, n, Ih0, Ih1, Ih2, Ih3, Ig0, Ig1, Ig2, Ig3);
                    InvTransformDobes(ref gValueskk, n, Ih0, Ih1, Ih2, Ih3, Ig0, Ig1, Ig2, Ig3);
                    InvTransformDobes(ref bValueskk, n, Ih0, Ih1, Ih2, Ih3, Ig0, Ig1, Ig2, Ig3);
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
                    InvTransformDobes(ref rValueskkk, n, Ih0, Ih1, Ih2, Ih3, Ig0, Ig1, Ig2, Ig3);
                    InvTransformDobes(ref gValueskkk, n, Ih0, Ih1, Ih2, Ih3, Ig0, Ig1, Ig2, Ig3);
                    InvTransformDobes(ref bValueskkk, n, Ih0, Ih1, Ih2, Ih3, Ig0, Ig1, Ig2, Ig3);
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
            double[,] masrkkk = new double[bmp.Height, bmp.Width];
            double[,] masgkkk = new double[bmp.Height, bmp.Width];
            double[,] masbkkk = new double[bmp.Height, bmp.Width];

            for (int Height = 0; Height < kHeight; Height += 1)
            {
                for (int Width = 0; Width < kWidth; Width += 1)
                {
                    if ((Width < bmp.Width) && (Height < bmp.Height))
                    {
                        masrkkk[Height, Width] = masrk[Height, Width];
                        masgkkk[Height, Width] = masgk[Height, Width];
                        masbkkk[Height, Width] = masbk[Height, Width];
                    }
                }
            }
            //Дибильный переворот изображения
            for (int i = 0; i < bmp.Height / 2; i++)
            {
                for (int j = 0; j < bmp.Width / 2; j++)
                {
                    masrkk[i, j] = masrkkk[i * 2, j * 2];
                    masgkk[i, j] = masgkkk[i * 2, j * 2];
                    masbkk[i, j] = masbkkk[i * 2, j * 2];
                }
            }
            /*
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
            //конец дибильного переворота
            //преобразование к исходному размеру
            for (int Height = 0; Height < kHeight; Height += 1)
            {
                for (int Width = 0; Width < kWidth; Width += 1)
                {
                    if ((Width < bmp.Width) && (Height < bmp.Height))
                    {
                        if (masrkk[Height, Width] > 0)
                            if (masrkk[Height, Width] < 255)
                                rValues[(Width + (Height * bmp.Width))] = rValuesk[(Width + (Height * kWidth))] = masrkk[Height, Width];
                            else
                                rValues[(Width + (Height * bmp.Width))] = rValuesk[(Width + (Height * kWidth))] = 255;
                        else
                            rValues[(Width + (Height * bmp.Width))] = rValuesk[(Width + (Height * kWidth))] = 0;
                        if (masgkk[Height, Width] > 0)
                            if (masgkk[Height, Width] < 255)
                                gValues[(Width + (Height * bmp.Width))] = gValuesk[(Width + (Height * kWidth))] = masgkk[Height, Width];
                            else
                                gValues[(Width + (Height * bmp.Width))] = gValuesk[(Width + (Height * kWidth))] = 255;
                        else
                            gValues[(Width + (Height * bmp.Width))] = gValuesk[(Width + (Height * kWidth))] = 0;
                        if (masbkk[Height, Width] > 0)
                            if (masbkk[Height, Width] < 255)
                                bValues[(Width + (Height * bmp.Width))] = bValuesk[(Width + (Height * kWidth))] = masbkk[Height, Width];
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
