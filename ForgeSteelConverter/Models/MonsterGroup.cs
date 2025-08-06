namespace ForgeSteelConverter.Models;

public class MonsterGroup : Element
{
    public Element[] information { get; set; } = [];

    public Malice[] malice { get; set; } = [];

    public Monster[] monsters { get; set; } = [];

    public AddOn[] addOns { get; set; } = [];
}