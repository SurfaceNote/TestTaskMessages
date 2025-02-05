namespace MessageService.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Security.AccessControl;
    using MessageService.Data.Database;
    using MessageService.Models;
    using MessageService.Services;
    using Serilog;

    public class MessageRepository : IMessageRepository
    {
        private readonly IDatabase _database;
        private readonly ILogger _logger;
        
        public MessageRepository(IDatabase database, ILogger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void SaveMessage(MessageDTO messageDTO)
        {
            try
            {
                string query = "INSERT INTO messages (Text, Timestamp, SequenceNumber) VALUES (@Time, @Timestamp, @SequenceNumber)";
                var parameters = new Dictionary<string, object>
                {
                    { "@Time", messageDTO.Text },
                    { "@Timestamp", messageDTO.Timestamp },
                    { "@SequenceNumber", messageDTO.SequenceNumber }
                };
                _logger.Debug("Вызов {ActionName} SQL Query: {Query} with Parameters: {@Parameters}", nameof(SaveMessage), query, parameters);
                
                _database.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Произошла ошибка при обработке запроса в {RepositoryName} и методе {ActionName}", nameof(MessageRepository), nameof(SaveMessage));
            }
        }

        public List<MessageDTO> GetMessagesForLastAmountMinutes(int minutes)
        {
            try 
            {
                var startTime = DateTime.UtcNow.AddMinutes(-minutes);

                var query = "SELECT Text, Timestamp, SequenceNumber FROM messages " +
                            "WHERE Timestamp >= @startTime";

                var parameters = new Dictionary<string, object>(){
                    { "@startTime", startTime }
                };

                var dataTable = _database.ExecuteQuery(query, parameters);
                var messages = new List<MessageDTO>();

                // Преобразуем строки из DataTable в объекты MessageDTO
                foreach (DataRow row in dataTable.Rows)
                {
                    messages.Add(new MessageDTO
                    {
                        Text = row["Text"].ToString(),
                        Timestamp = Convert.ToDateTime(row["Timestamp"]),
                        SequenceNumber = Convert.ToInt32(row["SequenceNumber"])
                    });
                }

                return messages;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Произошла ошибка при обработке запроса в {RepositoryName} и методе {ActionName}", nameof(MessageRepository), nameof(GetMessagesForLastAmountMinutes));
                throw;
            }
        }
    }
}