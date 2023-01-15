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
	private readonly IConfiguration _configuration;
	private readonly IHostEnvironment _environment;
	
	public InteractionHandler(DiscordSocketClient client,
		ILogger<DiscordClientService> logger,
		IServiceScopeFactory scopeFactory,
		InteractionService interactionService,
		IConfiguration configuration,
		IHostEnvironment environment) : base(client, logger)
	{
		_scopeFactory = scopeFactory;
		_interactionService = interactionService;
		_configuration = configuration;
		_environment = environment;
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

		if (_environment.IsProduction())
		{
			await _interactionService.RegisterCommandsGloballyAsync();
		}
		else
		{
			var guildIds = _configuration.GetValue<ulong[]>("TestGuilds") ?? Array.Empty<ulong>();
			foreach (var guildId in guildIds)
			{
				await _interactionService.RegisterCommandsToGuildAsync(guildId);
			}
		}
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