using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace WinstonBot.Models;

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
    public required Times Times { get; set; }
    public required Player[] Players { get; set; }
    public bool Emulated { get; set; }
    public required string Video { get; set; }
    public string? Comment { get; set; }
    public string? Splitsio { get; set; }
    public required Dictionary<string, Variable> Variables { get; set; }
}

public class Times
{
    [JsonPropertyName("realtime")]
    public float? RealTime { get; set; }

    [JsonPropertyName("realtime_noloads")]
    public float? RealTimeNoLoads { get; set; }

    [JsonPropertyName("ingame")]
    public float? InGame { get; set; }
}

public class Player
{
    public UserRole Rel { get; set; }
    public required string Id { get; set; }
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

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum UserRole
{
    [EnumMember(Value = "guest")]
    Guest,
    [EnumMember(Value = "banned")]
    Banned,
    [EnumMember(Value = "user")]
    User,
    [EnumMember(Value = "trusted")]
    Trusted,
    [EnumMember(Value = "moderator")]
    Moderator,
    [EnumMember(Value = "admin")]
    Admin,
    [EnumMember(Value = "programmer")]
    Programmer
}