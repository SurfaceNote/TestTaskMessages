namespace MessageService.Models
{
    using System;
    public class MessageDTO
    {
        public string Text {get; set;} = string.Empty;
        public DateTime Timestamp {get; set;}
        public int SequenceNumber {get; set;}
    }
}