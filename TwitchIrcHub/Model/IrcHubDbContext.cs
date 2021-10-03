using Microsoft.EntityFrameworkCore;
using TwitchIrcHub.Model.Schema;

namespace TwitchIrcHub.Model;

public class IrcHubDbContext : DbContext
{
    public DbSet<Bot> Bots { get; set; } = null!;
    public DbSet<Channel> Channels { get; set; } = null!;
    public DbSet<Connection> Connections { get; set; } = null!;
    public DbSet<RegisteredApp> RegisteredApps { get; set; } = null!;

    public IrcHubDbContext(DbContextOptions<IrcHubDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Bot.BuildModel(modelBuilder);
        Channel.BuildModel(modelBuilder);
        Connection.BuildModel(modelBuilder);
        RegisteredApp.BuildModel(modelBuilder);
    }
}
