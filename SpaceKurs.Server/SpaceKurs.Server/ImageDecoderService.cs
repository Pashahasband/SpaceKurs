namespace SpaceKurs.Server
{
    using System;
    using System.Drawing;
    using System.IO;

    using ImageDecoder;

    /// <summary>
    /// Сервис преобразования изображений
    /// </summary>
    public class ImageDecoderService
    {
        private readonly ImageDecoder ImageDecoder = new ImageDecoder();

        /// <summary>
        /// Кодирование промежуточного изображения
        /// </summary>
        /// <param name="imagePath">Путь к изображению, которое необходимо кодировать</param>
        /// <returns>Путь к кодированному изображению</returns>
        public string EncodeIntermediateImage(
            string imagePath)
        {
            if (imagePath == null)
            {
                throw new ArgumentNullException(string.Format("Argument {0} cannot be null", "imagePath"));
            }
            Bitmap bitmap;
            var directoryPath = Path.GetDirectoryName(imagePath);
            var filename = Path.GetFileNameWithoutExtension(imagePath);
            var extension = Path.GetExtension(imagePath);
            var intermediateFilename = string.Format("{0}_intermediate{1}", filename, extension);
            var fullIntermediatePath = Path.Combine(directoryPath, "Intermediate", intermediateFilename);
            bitmap = new Bitmap(imagePath);
            ImageDecoder.MakeGray(bitmap);
            Bitmap cropBmp = bitmap.Clone(new Rectangle(0, 0, bitmap.Width / 2, bitmap.Height / 2), bitmap.PixelFormat);
            cropBmp.Save(fullIntermediatePath);


            return fullIntermediatePath;
        }

        /// <summary>
        /// Кодирование изображения
        /// </summary>
        /// <param name="imagePath">Путь к изображению, которое необходимо кодировать</param>
        /// <returns>Путь к кодированному изображению</returns>
        public string EncodeImage(
            string imagePath)
        {
            if (imagePath == null)
            {
                throw new ArgumentNullException(string.Format("Argument {0} cannot be null", "imagePath"));
            }
            Bitmap bitmap;
            var directoryPath = Path.GetDirectoryName(imagePath);
            var filename = Path.GetFileNameWithoutExtension(imagePath);
            var extension = Path.GetExtension(imagePath);
            var previewFilename = string.Format("{0}_preview{1}", filename, extension);
            var fullPreviewPath = Path.Combine(directoryPath, "Previews", previewFilename);
            bitmap = new Bitmap(imagePath);
            ImageDecoder.MakeGray(bitmap);
            Bitmap cropBmp = bitmap.Clone(new Rectangle(0, 0, bitmap.Width / 2, bitmap.Height / 2), bitmap.PixelFormat);
            cropBmp.Save(fullPreviewPath);


            return fullPreviewPath;
        }
    }
}
