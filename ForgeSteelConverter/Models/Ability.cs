using System.Diagnostics;
using System.Text.Json;

namespace ForgeSteelConverter.Models;

public class Ability : Element
{
    public AbilityType type { get; set; }

    public string[] keywords { get; set; } = [];

    public AbilityDistance[] distance { get; set; } = [];

    public string target { get; set; }

    private object _cost;
    public object cost {
        get => _cost;
        set
        {
            if(value is JsonElement element)
            {
                switch(element.ValueKind)
                {
                    case JsonValueKind.String:
                        _cost = element.GetString(); break;
                    case JsonValueKind.Number:
                        _cost = element.GetInt32(); break;
                    default:
                        Debugger.Break(); break;
                }
            }
        }
    }

    public bool repeatable { get; set; }

    public int minLevel { get; set; }

    public AbilitySection[] sections { get; set; } = [];

    public string preEffect { get; set; }

    //public object roll { get; set; }

    //public object test { get; set; }

    public string effect { get; set; }

    public string strained { get; set; }

    //public object[] alternateEffects { get; set; }

    //public object[] spend { get; set; }

    //public object[] persistence { get; set; }
}
