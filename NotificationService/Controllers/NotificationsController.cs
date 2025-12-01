//using Microsoft.AspNetCore.Mvc;
//using NotificationService.Models;
//using NotificationService.Repositories;
//using NotificationService.Queues;

//namespace NotificationService.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class NotificationsController : ControllerBase
//    {
//        private readonly INotificationRepository _repo;
//        private readonly IQueueProducer _producer;

//        public NotificationsController(INotificationRepository repo, IQueueProducer producer)
//        {
//            _repo = repo;
//            _producer = producer;
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(NotificationCreateRequest req)
//        {
//            var notif = new Notification
//            {
//                ToEmail = req.To,
//                Subject = req.Subject,
//                Body = req.Body,
//                Type = req.Type,
//                DesiredSendAt = req.SendAt
//            };

//            await _producer.PublishAsync(notif);

//            return Accepted(notif);
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> Get(int id)
//        {
//            var n = await _repo.GetByIdAsync(id);
//            return n == null ? NotFound() : Ok(n);
//        }
//    }

//    public class NotificationCreateRequest
//    {
//        public string To { get; set; } = "";
//        public string Subject { get; set; } = "";
//        public string Body { get; set; } = "";
//        public string Type { get; set; } = "";
//        public DateTime? SendAt { get; set; }
//    }
//}


using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Repositories;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _repo;

        public NotificationsController(INotificationRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetPending()
        {
            return Ok(await _repo.GetPendingAsync());
        }
    }
}
