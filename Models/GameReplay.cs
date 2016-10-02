using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class GameReplay
    {
        [Key]
        public int id { get; set; }
        public string hash { get; set; }
        public byte[] moves { get; set; }
        public byte movesCount { get; set; }
    }
}