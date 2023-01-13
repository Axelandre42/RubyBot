using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RubyBot;
using RubyBot.Model;

var host = Host.CreateDefaultBuilder(args)
	.ConfigureDiscordHost((context, config) =>
	{
		config.SocketConfig = new DiscordSocketConfig
		{
			LogLevel = LogSeverity.Verbose,
			AlwaysDownloadUsers = true,
			MessageCacheSize = 200,
			UseInteractionSnowflakeDate = false,
			GatewayIntents = GatewayIntents.AllUnprivileged
			                 - GatewayIntents.GuildScheduledEvents
			                 - GatewayIntents.GuildInvites
		};

		config.Token = context.Configuration["token"] ?? string.Empty;
	})
	.UseInteractionService((context, config) =>
	{
		config.LogLevel = LogSeverity.Info;
		config.UseCompiledLambda = true;
	})
	.ConfigureServices((context, services) =>
	{
		var dbPath = Path.Join(Environment.CurrentDirectory, "ruby_bot.db");
		services.AddDbContext<RolePlayContext>(b => b
			.UseLazyLoadingProxies()
			.UseSqlite($"Data Source={dbPath}"));
		services.AddHostedService<InteractionHandler>();
	})
	.Build();

await host.RunAsync();