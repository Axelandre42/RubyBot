using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using RubyBot.Model;

namespace RubyBot.Modules;

[Group("persona", "Gérer des personnages.")]
// ReSharper disable once ClassNeverInstantiated.Global
public class PersonaModule : InteractionModuleBase<SocketInteractionContext>
{
	private readonly RolePlayContext _dbContext;
	public static readonly Random Random = new();

	public PersonaModule(RolePlayContext dbContext)
	{
		_dbContext = dbContext;
	}

	private static Embed CreatePersonaEmbed(Persona persona)
	{
		var embed = new EmbedBuilder
		{
			Color = Color.Red,
			Title = $"Personnage : {persona.Name}",
			Description = "Statistiques détaillées."
		};
		embed.AddField("Précision", persona.Precision)
			.AddField("Force", persona.Strength)
			.AddField("Agilité", persona.Agility)
			.AddField("Parade", persona.Parade)
			.AddField("Résistance", persona.Resistance);
		return embed.Build();
	}

	[SlashCommand("get", "Afficher un personnage.")]
	public async Task Get([Summary(description: "Alias du personnage.")] string alias)
	{
		var player = await _dbContext.Players.FindAsync(Context.User.Id);
		if (player != null)
		{
			var personaObj = await _dbContext.Personas.SingleOrDefaultAsync(p => p.Player == player && p.Alias == alias);
			if (personaObj != null)
			{
				await RespondAsync(embed: CreatePersonaEmbed(personaObj));
				return;
			}
		}

		await RespondAsync("Impossible de trouver le personnage.");
	}
	
	[SlashCommand("set", "Créer ou modifier un personnage.")]
	public async Task Set([Summary(description: "Alias du personnage.")] string alias,
		[Summary(description: "Nom du personnage.")] string name,
		[Summary(description: "Statistique de précision.")] byte precision,
		[Summary(description: "Statistique de force.")] byte strength,
		[Summary(description: "Statistique d'agilité.")] byte agility,
		[Summary(description: "Statistique de parade.")] byte parade,
		[Summary(description: "Statistique de résistance.")] byte resistance)
	{
		if (precision + strength + agility + parade + resistance != 15)
		{
			await RespondAsync("Les statistiques ne font pas un total de 15.");
			return;
		}
		
		var player = await _dbContext.Players.FindAsync(Context.User.Id);
		if (player == null)
		{
			player = new Player
			{
				Id = Context.User.Id,
				Seed = Random.Next()
			};
			
			await _dbContext.AddAsync(player);
			await _dbContext.SaveChangesAsync();
		}

		var personaObj = await _dbContext.Personas.SingleOrDefaultAsync(p => p.Player == player && p.Alias == alias);
		if (personaObj == null)
		{
			personaObj = new Persona
			{
				Name = name,
				Alias = alias,
				Locked = false,
				Precision = precision,
				Strength = strength,
				Agility = agility,
				Parade = parade,
				Resistance = resistance,
				Player = player
			};
			
			await _dbContext.AddAsync(personaObj);

			player.LastUsed = personaObj;
			await _dbContext.SaveChangesAsync();
			await RespondAsync("Personnage créé.", embed: CreatePersonaEmbed(personaObj));
		}
		else if (!personaObj.Locked)
		{
			personaObj.Name = name;
			personaObj.Precision = precision;
			personaObj.Strength = strength;
			personaObj.Agility = agility;
			personaObj.Parade = parade;
			personaObj.Resistance = resistance;
			
			player.LastUsed = personaObj;
			await _dbContext.SaveChangesAsync();
			await RespondAsync("Personnage modifié.", embed: CreatePersonaEmbed(personaObj));
			return;
		}
		
		await RespondAsync("Votre personnage est verrouillé.");
	}
	
	[SlashCommand("list", "Lister tous les personnages vous appartenant.")]
	public async Task List()
	{
		var player = await _dbContext.Players.FindAsync(Context.User.Id);
		if (player != null)
		{
			if (player.Personas.Count == 0)
			{
				await RespondAsync("Vous n'avez aucun personnage.");
				return;
			}
			
			var embed = new EmbedBuilder
			{
				Color = Color.Red,
				Title = $"Personnages pour {Context.User.Username}"
			};
			foreach (var persona in player.Personas)
			{
				embed.AddField(persona.Name, $"Alias : `{persona.Alias}`");
			}

			await RespondAsync("Vos personnages.", embed: embed.Build());
			return;
		}

		await RespondAsync("Vous n'avez aucun personnage.");
	}

	[SlashCommand("delete", "Supprimer un personnage.")]
	public async Task Delete([Summary(description: "Alias du personnage.")] string alias)
	{
		var player = await _dbContext.Players.FindAsync(Context.User.Id);
		if (player != null)
		{
			var personaObj = await _dbContext.Personas.SingleOrDefaultAsync(p => p.Player == player && p.Alias == alias);
			if (personaObj is {Locked: false})
			{
				_dbContext.Personas.Remove(personaObj);
				if (player.LastUsed == personaObj)
				{
					player.LastUsed = null;
				}
				
				await _dbContext.SaveChangesAsync();
				await RespondAsync("Personnage retiré.", embed: CreatePersonaEmbed(personaObj));
				return;
			}
			
			await RespondAsync("Votre personnage est verrouillé.");
			return;
		}

		await RespondAsync("Impossible de trouver le personnage.");
	}
	
	[EnabledInDm(false)]
	[DefaultMemberPermissions(GuildPermission.ManageMessages)]
	[SlashCommand("locked", "Verrouiller un personnage.")]
	public async Task Locked([Summary(description: "Joueur concerné.")] IUser player,
		[Summary(description: "Alias du personnage.")] string alias,
		[Summary(description: "Appliquer un verrouillage.")] bool locked)
	{
		if (player.Id == Context.User.Id)
		{
			await RespondAsync("Vous ne pouvez pas effectuer cette commande pour vous-même.");
			return;
		}

		var playerObj = await _dbContext.Players.FindAsync(player.Id);
		if (playerObj != null)
		{
			var personaObj = await _dbContext.Personas.SingleOrDefaultAsync(p => p.Player == playerObj && p.Alias == alias);
			if (personaObj != null)
			{
				personaObj.Locked = locked;
				await _dbContext.SaveChangesAsync();
				await RespondAsync(locked ? "Personnage verrouillé." : "Personnage déverrouillé",
					embed: CreatePersonaEmbed(personaObj));
				return;
			}
		}

		await RespondAsync("Impossible de trouver le personnage.");
	}

}