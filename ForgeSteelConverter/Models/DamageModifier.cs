namespace ForgeSteelConverter.Models;

public class DamageModifier
{
    public string damageType { get; set; }

    public string type { get; set; }

    public int value { get; set; }

    public string[] valueCharacteristics { get; set; } = [];

    public int valueCharacteristicMultiplier { get; set; }

    public int valuePerLevel { get; set; }

    public int valuePerEchelon { get; set; }
}