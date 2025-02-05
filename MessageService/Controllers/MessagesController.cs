namespace MessageService.Controllers
{
    using System;
    using System.Text.Json;
    using MessageService.Data.Repositories;
    using MessageService.Models;
    using MessageService.Services;
    using Microsoft.AspNetCore.Mvc;
    using Serilog;

    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _repository;
        private readonly WebSocketConnectionManager _webSocketConnectionManager;
        private readonly ILogger _logger;
        
        public MessagesController(IMessageRepository repository, WebSocketConnectionManager webSocketConnectionManager, 
                                  ILogger logger)
        {
            _repository = repository;
            _webSocketConnectionManager = webSocketConnectionManager;
            _logger = logger;
        }

        /// <summary>
        /// Получает сообщения за последние N минут.
        /// </summary>
        /// <param name="minutes">Количество минут, за которые нужно получить сообщения.</param>
        /// <returns>Список сообщений.</returns>
        /// <response code="200">Возвращает список сообщений.</response>
        /// <response code="400">Если параметр minutes не указан или неверен.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MessageDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, "text/plain")]
        public IActionResult GetMessagesForLastAmountMinutes([FromQuery] int minutes)
        {
            try
            {
                if (minutes <= 0)
                {
                    _logger.Warning("В контроллере {ControllerName} и методе {ActionName} было переданно неправильное число минут ({minutes})", 
                                    nameof(MessagesController), nameof(GetMessagesForLastAmountMinutes), minutes);
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        ContentType = "text/plain",
                        Content = "Минуты должны быть положительным числом."
                    };
                }
                var messages = _repository.GetMessagesForLastAmountMinutes(minutes);

                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Произошла ошибка при обработке запроса в контроллере {ControllerName} и методе {ActionName}", nameof(MessagesController), nameof(GetMessagesForLastAmountMinutes));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Отправляет сообщение и сохраняет его в истории.
        /// </summary>
        /// <param name="sendMessageDTO">Данные отправляемого сообщения.</param>
        /// <returns>Отправленное сообщение.</returns>
        /// <response code="200">Сообщение успешно отправлено и сохранено.</response>
        /// <response code="400">Если данные сообщения неверны.</response>
        [HttpPost]
        [ProducesResponseType(typeof(MessageDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, "text/plain")]
        public IActionResult PostMessageHistory([FromBody] SendMessageDTO  sendMessageDTO)
        {
            // Проверяем, что пришло сообщение и SequenceNumber, иначе возвращаем ошибку
            if (sendMessageDTO == null || string.IsNullOrEmpty(sendMessageDTO.Text))
            {
                _logger.Warning("В контроллере {ControllerName} и методе {ActionName} получили пустое сообщение", 
                                    nameof(MessagesController), nameof(PostMessageHistory));
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Content = "Неправильный POST-запрос.",
                    ContentType = "text/plain"
                };
            }

            var messageDTO = new MessageDTO
            {
                Text = sendMessageDTO.Text,
                SequenceNumber = sendMessageDTO.SequenceNumber,
                Timestamp = DateTime.UtcNow
            };

            // Сохраняем сообщение в репозитории
            _repository.SaveMessage(messageDTO);

            // Отправляем новое сообщение всем WebSocket-клиентам
            string jsonMessage = JsonSerializer.Serialize(messageDTO);
            _webSocketConnectionManager.BroadcastMessage(jsonMessage);

            return Ok(messageDTO);
        }
    }
}