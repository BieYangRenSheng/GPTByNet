namespace ChatGptByNet.Models
{
    public class LogModel
    {
        public int Id { get; set; }

        public string? Request { get; set; }

        public string? Response { get; set; }

        public long UserId { get; set; }

        public string? UserName { get; set; }

        public DateTime InsertTime { get; set; }

        public int StatusCode { get; set; } = 0;    
    }
}
