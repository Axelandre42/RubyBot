using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using RubyBot.Model;

namespace RubyBot.Modules;

[Group("roll", "Lancer un dé")]
public class RollModule : InteractionModuleBase<SocketInteractionContext>
{
	private readonly RolePlayContext _dbContext;

	public RollModule(RolePlayContext dbContext)
	{
		_dbContext = dbContext;
	}

	private static int Random(Player player, int min, int max)
	{
		var random = new Random(player.Seed);
		player.Seed = random.Next();
		
		return random.Next(min, max + 1);
	}

	private static Embed CreateRollEmbed(string persona, string name, int roll, int bonus, bool canBeCritical)
	{
		var embed = new EmbedBuilder
		{
			Color = Color.Red,
			Title = $"{name} - {persona}"
		};
		var baseDesc = $"{roll + bonus} (__{roll}__ + {bonus})\n";

		if (!canBeCritical)
		{
			embed.WithDescription(baseDesc);
			return embed.Build();
		}
		switch (roll)
		{
			case 1:
				embed.WithDescription(baseDesc + "Il s'agit d'un **échec critique**.");
				break;
			case 20:
				embed.WithDescription(baseDesc + "Il s'agit d'une **réussite critique**.");
				break;
			default:
			{
				embed.WithDescription(baseDesc + (roll + bonus > 10
					? "Il s'agit d'une **réussite**."
					: "Il s'agit d'un **échec**."));

				break;
			}
		}

		return embed.Build();
	}

	[SlashCommand("action", "Lancer un dé pour un personnage.")]
	public async Task Action([Summary(description: "Statistique à utiliser.")] StatType? type = null,
		[Summary(description: "Bonus de statistique.")] int bonus = 0,
		[Summary(description: "Alias du personnage.")] string? alias = null)
	{
		var player = await _dbContext.Players.FindAsync(Context.User.Id);
		if (player != null)
		{
			Persona personaObj;
			if (alias == null)
			{
				if (player.LastUsed == null)
				{
					await RespondAsync("Impossible de lancer un dé pour ce personnage.");
					return;
				}
				
				personaObj = player.LastUsed;
			}
			else
			{
				personaObj =
					await _dbContext.Personas.SingleOrDefaultAsync(p => p.Player == player && p.Alias == alias);
			}

			if (personaObj != null)
			{
				var roll = Random(player, 1, 20);
				var canBeCritical = false;
				var name = "";

				switch (type)
				{
					case StatType.Precision:
						name = "Précision";
						bonus += personaObj.Precision;
						canBeCritical = true;
						break;
					case StatType.Strength:
						name = "Force";
						bonus += personaObj.Strength;
						canBeCritical = true;
						break;
					case StatType.Agility:
						name = "Agilité";
						bonus += personaObj.Agility;
						break;
					case StatType.Parade:
						name = "Parade";
						bonus += personaObj.Parade;
						break;
					case StatType.Resistance:
						name = "Résistance";
						bonus += personaObj.Resistance;
						break;
					default:
						name = "(aucun bonus)";
						break;
				}


				player.LastUsed = personaObj;
				await _dbContext.SaveChangesAsync();

				await RespondAsync($"Résultat du lancé.",
					embed: CreateRollEmbed(personaObj.Name, name, roll, bonus, canBeCritical));
				return;
			}
		}

		await RespondAsync("Impossible de lancer un dé pour ce personnage.");
	}

	[SlashCommand("damage", "Lancer un dé de dégâts.")]
	public async Task Damage()
	{
		var player = await _dbContext.Players.FindAsync(Context.User.Id);
		if (player == null)
		{
			player = new Player
			{
				Id = Context.User.Id,
				Seed = PersonaModule.Random.Next()
			};
			
			await _dbContext.AddAsync(player);
		}

		var roll = Random(player, 1, 10);
		await _dbContext.SaveChangesAsync();
		var embed = new EmbedBuilder
		{
			Color = Color.Red,
			Title = "Dégâts",
			Description = $"__{roll}__"
		};
		
		await RespondAsync($"Résultat du lancé.", embed: embed.Build());
	}

	[SlashCommand("target", "Lancer un dé de ciblage.")]
	public async Task Target()
	{
		var player = await _dbContext.Players.FindAsync(Context.User.Id);
		if (player == null)
		{
			player = new Player
			{
				Id = Context.User.Id,
				Seed = PersonaModule.Random.Next()
			};
			
			await _dbContext.AddAsync(player);
		}

		var roll = Random(player, 1, 6);
		await _dbContext.SaveChangesAsync();
		var embed = new EmbedBuilder
		{
			Color = Color.Red,
			Title = "Ciblage",
			Description = $"__{roll}__"
		};
		
		await RespondAsync($"Résultat du lancé.", embed: embed.Build());
	}

	[SlashCommand("simple", "Lancer un dé simple.")]
	public async Task Simple([Summary(description: "Taille du dé.")] int size)
	{
		var player = await _dbContext.Players.FindAsync(Context.User.Id);
		if (player == null)
		{
			player = new Player
			{
				Id = Context.User.Id,
				Seed = PersonaModule.Random.Next()
			};
			
			await _dbContext.AddAsync(player);
		}

		var roll = Random(player, 1, size);
		await _dbContext.SaveChangesAsync();
		var embed = new EmbedBuilder
		{
			Color = Color.Red,
			Title = $"Dé {size}",
			Description = $"__{roll}__"
		};
		
		await RespondAsync($"Résultat du lancé.", embed: embed.Build());
	}
	
	

	[SlashCommand("current", "Voir ou définir le personnage utilisé lors des prochains lancés.")]
	public async Task Current([Summary(description: "Alias du personnage.")] string? alias = null)
	{
		var player = await _dbContext.Players.FindAsync(Context.User.Id);
		if (player != null || player.LastUsed != null || alias != null)
		{
			if (alias != null)
			{
				var persona = await _dbContext.Personas.SingleOrDefaultAsync(p => p.Player == player && p.Alias == alias);
				if (persona == null)
				{
					await RespondAsync("Impossible de trouver ce personnage.");
					return;
				}

				player.LastUsed = persona;
				await _dbContext.SaveChangesAsync();
			}
			await RespondAsync($"Vous jouez avec {player.LastUsed.Name}.");
			return;
		}

		await RespondAsync("Impossible de trouver ce personnage.");
	}
}