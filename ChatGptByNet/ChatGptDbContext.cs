using ChatGptByNet.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatGptByNet
{
    public class ChatGptDbContext : DbContext
    {
        public ChatGptDbContext(DbContextOptions<ChatGptDbContext> options) : base(options)
        {
        }

        public DbSet<LogModel> LogModels { get; set; }

        public DbSet<UserModel> UserModels { get; set; }
    }
}
