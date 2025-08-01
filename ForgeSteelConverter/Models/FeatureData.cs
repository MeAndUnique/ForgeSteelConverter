using System.Diagnostics;
using System.Text.Json;

namespace ForgeSteelConverter.Models;

public class FeatureData
{
    public string field { get; set; }

    public int value { get; set; }

    public int valuePerLevel { get; set; }

    public HeroicGains[] gains { get; set; } = [];

    private object[] _options = [];
    public object[] options
    {
        get => _options;
        set
        {
            if (value.Length > 0 && value[0] is JsonElement firstElement)
            {
                switch(firstElement.ValueKind)
                {
                    case JsonValueKind.Object:
                        _options = [.. value.Cast<JsonElement>().Select(element => element.GetProperty("feature").Deserialize<Feature>()!)];
                        break;
                    case JsonValueKind.String:
                        _options = [.. value.Select(element => element.ToString()!)];
                        break;
                    default:
                        Debugger.Break(); break;
                }
            }
        }
    }

    public string[] listOptions { get; set; } = [];

    public int count { get; set; }

    public Ability ability { get; set; }

    public string[] types { get; set; } = [];

    public int level { get; set; }

    private object _cost;
    public object cost
    {
        get => _cost;
        set
        {
            if (value is JsonElement element)
            {
                switch (element.ValueKind)
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

    public string[] lists { get; set; } = [];

    public string tag { get; set; }

    public string[] selected { get; set; } = [];

    public string[] keywords { get; set; } = [];

    public string[] valueCharacteristics { get; set; }

    public int valueCharacteristicMultiplier { get; set; }

    public int valuePerEchelon { get; set; }

    public string damageType { get; set; }

    public Feature[] features { get; set; } = [];

    public string[] weapons { get; set; } = [];

    public string[] armor { get; set; } = [];

    public DamageModifier[] modifiers { get; set; } = [];

    public int minLevel { get; set; }

    public string language { get; set; }

    public string details { get; set; }

    public bool canBeNegative { get; set; }
}
