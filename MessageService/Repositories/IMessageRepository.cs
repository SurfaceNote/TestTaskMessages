namespace MessageService.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using MessageService.Models;

    public interface IMessageRepository
    {
        void SaveMessage(MessageDTO messageDTO);
        List<MessageDTO> GetMessagesForLastAmountMinutes(int minutes);
    }
}