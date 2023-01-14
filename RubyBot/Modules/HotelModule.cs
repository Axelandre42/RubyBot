using Discord;
using Discord.Interactions;
using RubyBot.Model;

namespace RubyBot.Modules;

[Group("hotel", "Gérer les chambres d'hôtel.")]
public class HotelModule : InteractionModuleBase<SocketInteractionContext>
{
	private readonly RolePlayContext _dbContext;
	
	public HotelModule(RolePlayContext dbContext)
	{
		_dbContext = dbContext;
	}

	private IChannel GetChannelOrParentAsync(IChannel channel)
	{
		if (channel.GetChannelType() == ChannelType.PublicThread)
		{
			return Context.Guild.TextChannels.First(c => c.Threads.FirstOrDefault(t => t.Id == channel.Id) != null);
		}

		if (channel.GetChannelType() == ChannelType.Text)
		{
			return channel;
		}

		throw new ArgumentException("Channel type is not valid.", nameof(channel));
	}
	
	[EnabledInDm(false)]
	[DefaultMemberPermissions(GuildPermission.ManageMessages)]
	[SlashCommand("setup", "Utiliser ce salon en tant qu'hôtel.")]
	public async Task Setup([Summary(description: "Nombre de chambres disponibles.")] byte rooms)
	{
		var id = GetChannelOrParentAsync(Context.Channel).Id;
		var hotel = await _dbContext.Hotels.FindAsync(id);

		if (hotel == null)
		{
			hotel = new Hotel
			{
				Id = id,
				Size = rooms
			};

			await _dbContext.AddAsync(hotel);
			await _dbContext.SaveChangesAsync();
			await RespondAsync("Hôtel créé.", ephemeral: true);
		}
		else
		{
			if (rooms < hotel.Rooms.Count)
			{
				await RespondAsync("Impossible de réduire l'hôtel.", ephemeral: true);
			}
			else
			{
				hotel.Size = rooms;
				await _dbContext.SaveChangesAsync();
				await RespondAsync("Hôtel mis à jour.", ephemeral: true);
			}
		}
	}
	
	[EnabledInDm(false)]
	[SlashCommand("book", "Réserver une chambre d'hôtel.")]
	public async Task Book()
	{
		var parent = GetChannelOrParentAsync(Context.Channel);
		
		var hotel = await _dbContext.Hotels.FindAsync(parent.Id);
		if (hotel == null)
		{
			await RespondAsync("Vous devez réserver un chambre dans un hotel.", ephemeral: true);
			return;
		}

		if (hotel.Rooms.Count >= hotel.Size && hotel.Rooms.TrueForAll(r => r.Busy))
		{
			await RespondAsync("**Le réceptionniste répondit :**\nDésolé mais l'hôtel est complet. Revenez une autre fois.");
			return;
		}

		Room? room;
		do
		{
			var number = (byte) (Util.Random.Next(hotel.Size) + 1);
			room = hotel.Rooms.Find(r => r.Number == number);
			IThreadChannel thread;

			if (room == null)
			{
				thread = await ((ITextChannel) parent).CreateThreadAsync($"Chambre #{number}");
				room = new Room
				{
					Id = thread.Id,
					Number = number,
					Busy = true,
					Hotel = hotel
				};

				await _dbContext.AddAsync(room);
				await _dbContext.SaveChangesAsync();
				await thread.SendMessageAsync($"**Chambre réservée par <@{Context.User.Id}>.**");
				await RespondAsync($"**Le réceptionniste répondit :**\nLa <#{room.Id}> est réservée, nous vous souhaitons un agréable séjour...");
				
				return;
			}

			if (room.Busy) continue;
			
			thread = Context.Guild.GetThreadChannel(room.Id);
			
			room.Busy = true;
			
			await _dbContext.SaveChangesAsync();
			await thread.SendMessageAsync($"**Chambre réservée par <@{Context.User.Id}>.**");
			await RespondAsync($"**Le réceptionniste répondit :**\nLa <#{room.Id}> est réservée, nous vous souhaitons un agréable séjour...");
			
			return;
			
		} while (room.Busy);
	}

	[EnabledInDm(false)]
	[SlashCommand("free", "Libérer une chambre d'hôtel.")]
	public async Task Free()
	{
		var room = await _dbContext.Rooms.FindAsync(Context.Channel.Id);

		if (room == null)
		{
			await RespondAsync("Vous n'êtes pas dans une chambre d'hôtel.", ephemeral: true);
			return;
		}

		if (room.Busy)
		{
			room.Busy = false;
			await _dbContext.SaveChangesAsync();
			await RespondAsync("**Le réceptionniste répondit :**\nMerci pour votre visite et bon voyage...");
			return;
		}
		
		await RespondAsync("Cette chambre est déjà libre.", ephemeral: true);
	}
}