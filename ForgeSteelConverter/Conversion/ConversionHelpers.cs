using System.Text.RegularExpressions;

namespace ForgeSteelConverter.Conversion;

public static class ConversionHelpers
{
    public static string GetExportName(string name)
    {
        switch(name)
        {
            case "College of Black Ash":
                return "blackAsh";
            case "College of Caustic Alchemy":
                return "causticAlchemy";
            case "College of the Harlequin Mask":
                return "harlequinMask";
            case "Null":
                return "nullClass";
            case "Void":
                return "voidSubclass";
            default:
                return char.ToLower(name[0]) + name[1..].Replace("Echelon", "").Replace(" ", "").Replace(",", "").Replace("—", "");
        }
    }

    public static string GetFileName(string name)
    {
        switch (name)
        {
            case "College of Black Ash":
                return "black-ash";
            case "College of Caustic Alchemy":
                return "caustic-alchemy";
            case "College of the Harlequin Mask":
                return "harlequin-mask";
            default:
                name = name.Replace("Echelon", "").Replace(",", "").Replace("—", "").ToLower().Trim();
                name  =Regex.Replace(name, "\\s+", "-");
                return name;
        }
    }
}
