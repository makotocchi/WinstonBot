using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SpeedrunDotComAPI.Categories;
using SpeedrunDotComAPI.Games;
using SpeedrunDotComAPI.Runs;

namespace WinstonBot;

public class RunSubmission
{
    public required Run Run { get; set; }
}

public class Run
{
    public required string Category { get; set; }
    public string? Level { get; set; }
    public string? Date { get; set; }
    public string? Region { get; set; }
    public string? Platform { get; set; }
    public bool? Verified { get; set; }
    public required RunTimesModel Times { get; set; }
    public RunPlayerModel[]? Players { get; set; }
    public bool Emulated { get; set; }
    public string? Video { get; set; }
    public string? Comment { get; set; }
    public string? Splitsio { get; set; }
    public Dictionary<string, Variable>? Variables { get; set; }

    [JsonIgnore]
    public CategoryModel? CategoryModel { get; set; }
    [JsonIgnore]
    public GameModel? GameModel { get; set; }
}

public class Variable
{
    public required VariableType Type { get; set; }
    public required string Value { get; set; }
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum VariableType
{
    [EnumMember(Value = "pre-defined")]
    Predefined,
    [EnumMember(Value = "user-defined")]
    UserDefined
}