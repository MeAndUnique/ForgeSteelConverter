namespace ForgeSteelConverter.Models;

public class AbilitySection
{
    public string type { get; set; }

    public string text { get; set; }

    public string tag { get; set; }

    public string name { get; set; }

    public int value { get; set; }

    public bool repeatable { get; set; }

    public string effect { get; set; }

    public Roll roll { get; set; }
}