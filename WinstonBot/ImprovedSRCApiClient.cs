using SpeedrunDotComAPI;
using SpeedrunDotComAPI.Runs;
using System.Reflection;

namespace WinstonBot;

public class ImprovedSRCApiClient : SRCApiClient
{
    public ImprovedSRCApiClient(string? apiKey = null) : base(apiKey)
    {
        var httpClient = (typeof(RunApiClient).GetField("_http", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(Runs)) as HttpClient
            ?? throw new InvalidOperationException("Failed to get the HttpClient from the RunApiClient.");
        Runs = new ImprovedRunApiClient(httpClient);
    }
}
