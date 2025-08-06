using System.Diagnostics;
using System.Text.Json;

namespace ForgeSteelConverter.Models;

public class MaliceData
{
    public int cost { get; set; }

    private List<object> _sections = [];
    public List<object> sections
    {
        get => _sections;
        set
        {
            _sections.Clear();
            foreach(JsonElement element in value)
            {
                switch (element.ValueKind)
                {
                    case JsonValueKind.Object:
                        _sections.Add(element.Deserialize<Roll>()!);
                        break;
                    case JsonValueKind.String:
                        _sections.Add(element.ToString());
                        break;
                    default:
                        Debugger.Break(); break;
                }
            }
        }
    }
}