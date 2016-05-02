namespace SpaceKurs.Server
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Реестр изображений
    /// </summary>
    public class ImageRegistry
    {
        private readonly static Lazy<ImageRegistry> Instance = new Lazy<ImageRegistry>(
            () => new ImageRegistry());

        private readonly ICollection<ImageInfo> _registry;

        private ImageRegistry()
        {
            _registry = new Collection<ImageInfo>();
        }



        public static ImageInfo AddImage(
            string imagePath)
        {
            var id = Guid.NewGuid();
            
            string typeInfo = imagePath.Substring(imagePath.LastIndexOf(@".") + 1);
            Instance.Value._registry.Add(new ImageInfo
                                         {
                                             Id = id,
                                             ImagePath = imagePath,
                                             TypeName = typeInfo

        });
            
            return GetImageInfo(id);
        }

        public static void DeleteImage(
            string imagePath)
        {
            var imageInfo = GetImageInfo(imagePath);
            
            if (imageInfo != null)
            {
                Instance.Value._registry.Remove(imageInfo);
            }
        }

        public static void DeleteImage(
            Guid id)
        {
            var imageInfo = GetImageInfo(id);
            if (imageInfo != null)
            {
                Instance.Value._registry.Remove(imageInfo);
            }
        }

        public static ImageInfo GetImageInfo(
            Guid id)
        {
           return Instance.Value._registry.FirstOrDefault(ii => ii.Id == id);
        }

        public static ImageInfo GetImageInfo(
            string imagePath)
        {
            return Instance.Value._registry.FirstOrDefault(ii => ii.ImagePath == imagePath);
        }

        public static void Initialize(
            string[] paths)
        {
            foreach (var path in paths)
            {
                AddImage(path);
            }
        }

        /// <summary>
        /// Обновить реестр изображений
        /// </summary>
        /// <param name="paths">Пути всех доступных изображений</param>
        /// <returns>Добавленные в реестр изображения</returns>
        public static IEnumerable<ImageInfo> Update(
            string[] paths)
        {
            var addedImages = paths.Where(p => !Instance.Value._registry.Select(ii => ii.ImagePath).Contains(p)).ToList();
            var deletedImages = Instance.Value._registry.Where(ii => !paths.Contains(ii.ImagePath)).ToList();

            var addedImageInfos = addedImages.Aggregate(
                new Collection<ImageInfo>(),
                (infos, path) =>
                {
                    infos.Add(AddImage(path));
                    return infos;
                });

            foreach (var deletedImage in deletedImages)
            {
                DeleteImage(deletedImage.Id);
            }

            return addedImageInfos;
        }
    }
}
