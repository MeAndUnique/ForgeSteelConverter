namespace ForgeSteelConverter.Models;

public class AbilityType
{
    public string usage { get; set; }

    public bool free { get; set; }

    public string trigger { get; set; }

    public string time { get; set; }

    public string[] qualifiers { get; set; } = [];
}
