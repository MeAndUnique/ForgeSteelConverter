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
                return name.ToLower();
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
                return name.ToLower();
        }
    }
}
