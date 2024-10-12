using Microsoft.Extensions.Options;

namespace WinstonBot;

public class Worker : BackgroundService
{
    private readonly ImprovedSRCApiClient _client;
    private readonly BotOptions _options;
    private readonly ILogger<Worker> _logger;

    public Worker(ImprovedSRCApiClient client, IOptions<BotOptions> options, ILogger<Worker> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int onlyOnce = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (onlyOnce == 0)
            {
                foreach (var game in _options.Games)
                {
                    var gameVariables = await _client.Games.GetSingleGameVariables(game.Id);

                    var controlSchemeVariable = gameVariables.FirstOrDefault(x => x.Name == "Control Scheme");

                    var tankControlSchemeValue = controlSchemeVariable.Values.Values.FirstOrDefault(x => x.Value.Label == "Tank");
                }

                // var result = await _client.Runs.GetSingleRun("yvldjd6z");
                onlyOnce++;
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
