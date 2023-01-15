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

		config.Token = context.Configuration["Token"] ?? string.Empty;
	})
	.UseInteractionService((context, config) =>
	{
		config.LogLevel = LogSeverity.Info;
		config.UseCompiledLambda = true;
	})
	.ConfigureServices((context, services) =>
	{
		var databaseConfig = context.Configuration.GetSection("Database");
		
		var connectionString = databaseConfig["ConnectionString"] ?? string.Empty;
		var serverVersion = ServerVersion.Parse(databaseConfig["ServerVersion"] ?? string.Empty);
		
		services.AddDbContext<RolePlayContext>(b => b
			.UseLazyLoadingProxies()
			.UseMySql(connectionString, serverVersion));
		services.AddHostedService<InteractionHandler>();
	})
	.Build();

await host.RunAsync();