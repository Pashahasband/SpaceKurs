namespace SpaceKurs.Server
{
    using System;

    /// <summary>
    /// Информация об изображении
    /// </summary>
    public class ImageInfo
    {
        public Guid Id { get; set; }

        public string Extension { get; set; }

        public string ImagePath { get; set; }

        public string PreviewPath { get; set; }
    }
}
