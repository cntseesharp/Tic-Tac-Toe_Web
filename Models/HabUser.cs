namespace WebApplication1.Models
{
    public class HubUser
    {
        public Game ActiveGame { get; set; }
        public bool Player { get; set; }
        public bool PVP { get; set; }
        public System.DateTime LastConnection { get; set; }
    }
}