using Microsoft.EntityFrameworkCore;

namespace RubyBot.Model;

public class RolePlayContext : DbContext
{
	public DbSet<Player>? Players { get; set; }
	public DbSet<Persona>? Personas { get; set; }

	public RolePlayContext(DbContextOptions options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Player>()
			.Property<int?>("LastUsedId");

		modelBuilder.Entity<Player>()
			.HasOne(p => p.LastUsed)
			.WithOne()
			.HasForeignKey<Player>("LastUsedId");
	}
	
	
}