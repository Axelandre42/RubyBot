using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RubyBot.Model;

namespace RubyBot;

public class InteractionHandler : DiscordClientService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly InteractionService _interactionService;
	
	public InteractionHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceScopeFactory scopeFactory, InteractionService interactionService) : base(client, logger)
	{
		_scopeFactory = scopeFactory;
		_interactionService = interactionService;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var serviceProvider = _scopeFactory.CreateScope().ServiceProvider;
		var dbContext = serviceProvider.GetService<RolePlayContext>();
		
		Client.InteractionCreated += HandleInteraction;
		
		await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
		await Client.WaitForReadyAsync(stoppingToken);
		
		if (dbContext != null)
		{
			await dbContext.Database.MigrateAsync(cancellationToken: stoppingToken);
		}
		
		await _interactionService.RegisterCommandsGloballyAsync();
	}

	private async Task HandleInteraction(SocketInteraction arg)
	{
		try
		{
			var ctx = new SocketInteractionContext(Client, arg);
			await _interactionService.ExecuteCommandAsync(ctx, _scopeFactory.CreateScope().ServiceProvider);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Exception occurred whilst attempting to handle interaction.");

			if (arg.Type == InteractionType.ApplicationCommand)
			{
				var msg = await arg.GetOriginalResponseAsync();
				await msg.DeleteAsync();
			}

		}
	}
}