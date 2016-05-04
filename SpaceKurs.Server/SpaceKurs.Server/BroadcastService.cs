namespace SpaceKurs.Server
{
    using System;

    using Microsoft.AspNet.SignalR;

    /// <summary>
    /// Класс рассылки уведомлений всем подключенным клиентам
    /// </summary>
    public class BroadcastService
    {
        private readonly IHubContext _context;

        public BroadcastService()
        {
            _context = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
        }

        /// <summary>
        /// Рассылка уведомлений о новом изображении всем клиентам (вызов метода на клиентах onNewImagereceived)
        /// </summary>
        /// <param name="imageId">Идентификатор нового изображения</param>
        public void SendNewImageNotification(
            Guid imageId,
            string imageType)
        {
            _context.Clients.All.onNewImageReceived(imageId, imageType);
        }
    }
}
