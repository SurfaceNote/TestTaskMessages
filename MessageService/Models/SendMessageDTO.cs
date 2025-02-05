namespace MessageService.Models
{
    using System;

    public class SendMessageDTO
    {
        public string Text {get; set;} = string.Empty;
        public int SequenceNumber {get; set;}
    }
}