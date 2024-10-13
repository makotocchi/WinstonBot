using SpeedrunDotComAPI;
using SpeedrunDotComAPI.Runs;
using System.Reflection;
using System.Text.Json;
using SpeedrunDotComAPI.Utility;

namespace WinstonBot;

public class ImprovedSRCApiClient : SRCApiClient
{
    public new readonly ImprovedRunApiClient Runs;

    // Inaccessible in base class, re-implemented here.
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = new KebabCaseNamingPolicy(),
    };

    public ImprovedSRCApiClient(string? apiKey = null) : base(apiKey)
    {
        var httpClient = (typeof(RunApiClient).GetField("_http", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(Runs)) as HttpClient
            ?? throw new InvalidOperationException("Failed to get the HttpClient from the RunApiClient.");
        Runs = new ImprovedRunApiClient(httpClient);
    }
}
