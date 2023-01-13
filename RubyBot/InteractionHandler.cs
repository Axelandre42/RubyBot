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
	private readonly RolePlayContext _dbContext;
	
	public InteractionHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceScopeFactory scopeFactory, InteractionService interactionService, RolePlayContext dbContext) : base(client, logger)
	{
		_scopeFactory = scopeFactory;
		_interactionService = interactionService;
		_dbContext = dbContext;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		Client.InteractionCreated += HandleInteraction;
		
		await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _scopeFactory.CreateScope().ServiceProvider);
		await Client.WaitForReadyAsync(stoppingToken);

		await _dbContext.Database.MigrateAsync(cancellationToken: stoppingToken);
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