using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebClient.Data
{
    public class Message
    {
        [Required]
        [StringLength(128, ErrorMessage = "Текст должен быть короче, чем 128 символов!")]
        public string Text { get; set; } = String.Empty;
        [Required]
        public int SequenceNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}