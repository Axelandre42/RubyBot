﻿using Microsoft.EntityFrameworkCore;

namespace RubyBot.Model;

public class RolePlayContext : DbContext
{
	public DbSet<Player> Players { get; set; } = null!;
	public DbSet<Persona> Personas { get; set; } = null!;
	public DbSet<Hotel> Hotels { get; set; } = null!;
	public DbSet<Room> Rooms { get; set; } = null!;

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