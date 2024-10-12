using WinstonBot;

var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

var builder = Host.CreateApplicationBuilder(args);

var apiKey = builder.Configuration.GetValue<string>("SPEEDRUN_COM_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new Exception("Missing API key for speedrun.com");
}

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton(new ImprovedSRCApiClient(apiKey));
builder.Services.Configure<BotOptions>(builder.Configuration.GetSection(nameof(BotOptions)));

var host = builder.Build();
host.Run();
