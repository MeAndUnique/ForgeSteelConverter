namespace ForgeSteelConverter.Models;

public class Monster : Element
{
    public int level { get; set; }

    public Role role { get; set; }

    public string[] keywords { get; set; } = [];

    public int encounterValue { get; set; }

    public Size size { get; set; }

    public Speed speed { get; set; }

    public int stamina { get; set; }

    public int stability { get; set; }

    public int freeStrikeDamage { get; set; }

    public string withCaptain { get; set; }

    public CharacteristicValue[] characteristics { get; set; } = [];

    public Feature[] features { get; set; } = [];

    public Retainer retainer { get; set; }
}