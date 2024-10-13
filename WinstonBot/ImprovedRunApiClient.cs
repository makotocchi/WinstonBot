using System.Text;
using System.Text.Json;
using FluentResults;
using SpeedrunDotComAPI.Runs;

namespace WinstonBot;

public class ImprovedRunApiClient(HttpClient http) : RunApiClient(http)
{
    // Inaccessible in base class, re-implemented here.
    private readonly HttpClient _http = http;

    public async Task<Result<RunModel>> PostRun(Run run)
    {
        var submission = new RunSubmission { Run = run, };
        
        HttpContent httpContent;
        try
        {
            string jsonContent = JsonSerializer.Serialize(submission, ImprovedSRCApiClient.SerializerOptions);
            httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError("Request failed", e));
        }

        var responseMessage = await _http.PostAsync(RunEndpointUri, httpContent);
        try
        {
            // Response contains a "Location" header for the new run, but should be also available through RunModel.Weblink,
            // so it can be accessed programmatically by caller without needing a custom response type to capture Location.
            var runModel = await JsonSerializer.DeserializeAsync<RunModel>(await responseMessage.Content.ReadAsStreamAsync());
            return Result.Ok(runModel);
        }
        catch (Exception outerException)
        {
            try
            {
                var errorModel = await JsonSerializer.DeserializeAsync<RunErrorModel>(await responseMessage.Content.ReadAsStreamAsync());
                return new Error($"{errorModel!.Message} | Errors: {string.Join(", ", errorModel.Errors)}");
            }
            catch (Exception _)
            {
                return Result.Fail(new ExceptionalError("Response deserialization failed", outerException));
            }
        }
    }
}
