using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;

namespace RubyBot;

public class InteractionHandler : DiscordClientService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly InteractionService _interactionService;
	private readonly IHostEnvironment _environment;
	private readonly IConfiguration _configuration;
	
	public InteractionHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceScopeFactory scopeFactory, InteractionService interactionService, IHostEnvironment environment, IConfiguration configuration) : base(client, logger)
	{
		_scopeFactory = scopeFactory;
		_interactionService = interactionService;
		_environment = environment;
		_configuration = configuration;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		Client.InteractionCreated += HandleInteraction;
		
		await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _scopeFactory.CreateScope().ServiceProvider);
		await Client.WaitForReadyAsync(stoppingToken);
		
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