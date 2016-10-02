using WebApplication1.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WebApplication1.DAL
{
    public class GameReplayContext : DbContext
    {
        public GameReplayContext() : base("GameReplayContext")
        {
        }

        public DbSet<GameReplay> Games { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}