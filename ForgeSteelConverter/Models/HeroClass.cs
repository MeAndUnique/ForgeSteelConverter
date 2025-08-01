using System.Text.Json;

namespace ForgeSteelConverter.Models;

public class HeroClass : Element
{
	public string heroicResource { get; set; }

	public string subclassName { get; set; }

	public int subclassCount { get; set; }

	public string[][] primaryCharacteristicsOptions { get; set; }

	public string[] primaryCharacteristics { get; set; }

	public LeveledFeatures[] featuresByLevel { get; set; }

	public Ability[] abilities {  get; set; }

	public Subclass[] subclasses { get; set; }
}
