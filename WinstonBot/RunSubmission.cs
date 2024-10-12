using SpeedrunDotComAPI.Runs;

namespace WinstonBot;

public class RunSubmission
{
    public Run Run { get; set; }
}

public class Run
{
    public string Category { get; set; }
    public string Level { get; set; }
    public string Date { get; set; }
    public string Region { get; set; }
    public string Platform { get; set; }
    public bool Verified { get; set; }
    public RunTimesModel Times { get; set; }
    public List<RunPlayerModel> Players { get; set; }
    public bool Emulated { get; set; }
    public string Video { get; set; }
    public string Comment { get; set; }
    public string Splitsio { get; set; }
    public Dictionary<string, Variable> Variables { get; set; }
}

public class Variable
{
    public string Type { get; set; }
    public string Value { get; set; }
}
