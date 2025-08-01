using ForgeSteelConverter.Models;
using Microsoft.AspNetCore.Components;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace ForgeSteelConverter.Conversion;

public static class ClassConverter
{
    private const string heroClassHeader = """
        import { AbilityDistanceType } from '../../../enums/abiity-distance-type';
        import { AbilityKeyword } from '../../../enums/ability-keyword';
        import { Characteristic } from '../../../enums/characteristic';
        import { DamageModifierType } from '../../../enums/damage-modifier-type';
        import { DamageType } from '../../../enums/damage-type';
        import { FactoryLogic } from '../../../logic/factory-logic';
        import { FeatureField } from '../../../enums/feature-field';
        import { HeroClass } from '../../../models/class';
        import { KitArmor } from '../../../enums/kit-armor';
        import { KitWeapon } from '../../../enums/kit-weapon';
        import { PerkList } from '../../../enums/perk-list';
        import { SkillList } from '../../../enums/skill-list';

        """;

    private const string subclassHeader = """
        import { AbilityDistanceType } from '../../../enums/abiity-distance-type';
        import { AbilityKeyword } from '../../../enums/ability-keyword';
        import { Characteristic } from '../../../enums/characteristic';
        import { DamageModifierType } from '../../../enums/damage-modifier-type';
        import { DamageType } from '../../../enums/damage-type';
        import { FactoryLogic } from '../../../logic/factory-logic';
        import { FeatureField } from '../../../enums/feature-field';
        import { SkillList } from '../../../enums/skill-list';
        import { SubClass } from '../../../models/subclass';

        """;

    public static (string baseClass, List<string> subclasses) ConvertHero(HeroClass heroClass)
    {
        List<string> subclasses = new();
        StringBuilder builder = new StringBuilder(heroClassHeader);
        foreach (Subclass subclass in heroClass.subclasses)
        {
            builder.AppendLine($"import {{ {ConversionHelpers.GetExportName(subclass.name)} }} from './{ConversionHelpers.GetFileName(subclass.name)}';");
            subclasses.Add(ConvertSubclass(subclass));
        }
        builder.AppendLine();
        builder.AppendLine($"export const {ConversionHelpers.GetExportName(heroClass.name)}: HeroClass = {{");
        builder.AppendLine($"\tid: '{heroClass.id}',");
        builder.AppendLine($"\tname: '{heroClass.name}',");
        builder.AppendLine($"\tdescription: {SanitizeValue(heroClass.description)},");
        builder.AppendLine($"\tsubclassName: '{heroClass.subclassName}',");
        builder.AppendLine($"\tsubclassCount: {heroClass.subclassCount},");
        builder.AppendLine("\tprimaryCharacteristicsOptions: [");
        for (int index = 0; index < heroClass.primaryCharacteristicsOptions.Length; index++)
        {
            string[] options = heroClass.primaryCharacteristicsOptions[index];
            string characteristics = string.Join(", ", options.Select(characteristic => $"Characteristic.{characteristic}"));
            string ending = index == heroClass.primaryCharacteristicsOptions.Length - 1 ? string.Empty : ",";
            builder.AppendLine($"\t\t[ {characteristics} ]{ending}");
        }
        builder.AppendLine("\t],");
        builder.AppendLine("\tprimaryCharacteristics: [],");
        ConvertFeaturesByLevel(builder, heroClass.featuresByLevel);
        builder.AppendLine("\tabilities: [");
        for (int index = 0; index < heroClass.abilities.Length; index++)
        {
            Ability ability = heroClass.abilities[index];
            ConvertAbility(builder, ability, false, new('\t', 2), index == heroClass.abilities.Length - 1);
        }
        builder.AppendLine("\t],");
        builder.AppendLine("\tsubclasses: [");
        for (int index = 0; index < heroClass.subclasses.Length; index++)
        {
            Subclass subclass = heroClass.subclasses[index];
            string ending = index == heroClass.subclasses.Length - 1 ? string.Empty : ",";
            builder.AppendLine($"\t\t{ConversionHelpers.GetExportName(subclass.name)}{ending}");
        }
        builder.AppendLine("\t],");
        builder.AppendLine("\tlevel: 1,");
        builder.AppendLine("\tcharacteristics: []");
        builder.AppendLine("};");

        return (builder.ToString(), subclasses);
    }

    private static string ConvertSubclass(Subclass subclass)
    {
        StringBuilder builder = new(subclassHeader);
        builder.AppendLine();
        builder.AppendLine($"export const {ConversionHelpers.GetExportName(subclass.name)}: SubClass = {{");
        builder.AppendLine($"\tid: '{subclass.id}',");
        builder.AppendLine($"\tname: '{subclass.name}',");
        builder.AppendLine($"\tdescription: {SanitizeValue(subclass.description)},");
        ConvertFeaturesByLevel(builder, subclass.featuresByLevel);
        builder.AppendLine("\tselected: false");
        builder.AppendLine("};");

        return builder.ToString();
    }

    private static void ConvertFeaturesByLevel(StringBuilder builder, LeveledFeatures[] featuresByLevel)
    {
        builder.AppendLine("\tfeaturesByLevel: [");
        for (int index = 0; index < featuresByLevel.Length; index++)
        {
            LeveledFeatures leveledFeatures = featuresByLevel[index];
            string ending = index == featuresByLevel.Length - 1 ? string.Empty : ",";
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\tlevel: {leveledFeatures.level},");
            if (leveledFeatures.features.Length > 0)
            {
                builder.AppendLine("\t\t\tfeatures: [");
                ConvertFeatures(builder, leveledFeatures.features, new('\t', 4));
                builder.AppendLine("\t\t\t]");
            }
            else
            {
                builder.AppendLine("\t\t\tfeatures: []");
            }
            builder.AppendLine($"\t\t}}{ending}");
        }
        builder.AppendLine("\t],");
    }

    private static void ConvertFeatures(StringBuilder builder, Feature[] features, string indent)
    {
        for (int index = 0; index < features.Length; index++)
        {
            Feature feature = features[index];
            bool isLast = index == features.Length - 1;
            ConvertFeature(builder, feature, false, indent, isLast);
        }
    }

    private static void ConvertFeature(StringBuilder builder, Feature feature, bool fromChoice, string indent, bool isLast)
    {
        string begining = fromChoice ? "feature: " : string.Empty;
        builder.Append($"{indent}{begining}");
        switch (feature.type)
        {
            case "Bonus":
                ConvertBonusFeature(builder, feature, indent, isLast); break;
            case "Heroic Resource":
                ConvertHeroicResourceFeature(builder, feature, indent, isLast); break;
            case "Skill Choice":
                ConvertSkillChoiceFeature(builder, feature, indent, isLast); break;
            case "Domain":
                ConvertDomainChoiceFeature(builder, feature, indent, isLast); break;
            case "Ability":
                ConvertAbilityFeature(builder, feature, indent, isLast); break;
            case "Kit":
                ConvertKitFeature(builder, feature, indent, isLast); break;
            case "Domain Feature":
                ConvertDomainFeature(builder, feature, indent, isLast); break;
            case "Class Ability":
                ConvertClassAbilityChoiceFeature(builder, feature, indent, isLast); break;
            case "Perk":
                ConvertPerkFeature(builder, feature, indent, isLast); break;
            case "Text":
                ConvertTextFeature(builder, feature, indent, isLast); break;
            case "Package Content":
                ConvertPackageContentFeature(builder, feature, indent, isLast); break;
            case "Choice":
                ConvertChoiceFeature(builder, feature, indent, isLast); break;
            case "Package":
                ConvertPackageFeature(builder, feature, indent, isLast); break;
            case "Ability Damage":
                ConvertAbilityDamageFeature(builder, feature, indent, isLast); break;
            case "Ability Distance":
                ConvertAbilityDistanceFeature(builder, feature, indent, isLast); break;
            case "Multiple Features":
                ConvertMultipleFeature(builder, feature, indent, isLast); break;
            case "Proficiency":
                ConvertProficiencyFeature(builder, feature, indent, isLast); break;
            case "Damage Modifier":
                ConvertDamageModifierFeature(builder, feature, indent, isLast); break;
            case "Language":
                ConvertLanguageFeature(builder, feature, indent, isLast); break;
            case "Skill":
                ConvertSkillFeature(builder, feature, indent, isLast); break;
            case "Characteristic Bonus":
                ConvertCharacteristicBonusFeature(builder, feature, indent, isLast); break;
            case "Heroic Resource Gain":
                ConvertHeroicResourceGainFeature(builder, feature, indent, isLast); break;
            default:
                builder.AppendLine($"ERROR: UNKNOWN FEATURE"); break;
        }
    }

    private static void ConvertBonusFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createBonus({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        if(feature.name != feature.data.field)
        {
            builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        }
        if (!string.IsNullOrEmpty(feature.description))
        {
            builder.AppendLine($"{indent}\tdescription: {SanitizeValue(feature.description)},");
        }
        builder.Append($"{indent}\tfield: FeatureField.{feature.data.field}");
        if (feature.data.value is int valueInt && valueInt > 0)
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tvalue: {feature.data.value}");
        }
        if (feature.data.valuePerLevel != 0)
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tvaluePerLevel: {feature.data.valuePerLevel}");
        }
        if (feature.data.valuePerEchelon != 0)
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tvaluePerEchelon: {feature.data.valuePerEchelon}");
        }
        if (feature.data.valueCharacteristics.Length > 0)
        {
            string characteristics = string.Join(", ", feature.data.valueCharacteristics.Select(characteristic => $"Characteristic.{characteristic}"));
            builder.AppendLine(",");
            builder.Append($"{indent}\tvalueCharacteristics: [ {characteristics} ]");
        }
        builder.AppendLine();
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertHeroicResourceFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createHeroicResource({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        builder.AppendLine($"{indent}\tgains: [");
        for (int index = 0; index < feature.data.gains.Length; index++)
        {
            HeroicGains gains = feature.data.gains[index];
            string gainsEnding = index == feature.data.gains.Length - 1 ? string.Empty : ",";
            builder.AppendLine($"{indent}\t\t{{");
            builder.AppendLine($"{indent}\t\t\ttrigger: '{gains.trigger}',");
            builder.AppendLine($"{indent}\t\t\tvalue: '{gains.value}'");
            builder.AppendLine($"{indent}\t\t}}{gainsEnding}");
        }
        builder.Append($"{indent}\t]");
        if(!string.IsNullOrEmpty(feature.data.details))
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tdetails: {SanitizeValue(feature.data.details)}");
        }
        if(feature.data.canBeNegative)
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tcanBeNegative: true");
        }
        builder.AppendLine();
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertSkillChoiceFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createSkillChoice({");
        builder.Append($"{indent}\tid: '{feature.id}'");
        if (feature.data.options.Length > 0)
        {
            builder.AppendLine(",");
            string options = string.Join(", ", feature.data.options.Select(SanitizeValue));
            builder.Append($"{indent}\toptions: [ {options} ]");
        }
        if (feature.data.listOptions.Length > 0)
        {
            builder.AppendLine(",");
            string listOptions = string.Join(", ", feature.data.listOptions.Select(option => $"SkillList.{option}"));
            builder.Append($"{indent}\tlistOptions: [ {listOptions} ]");
        }
        if (feature.data.count > 1)
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tcount: {feature.data.count}");
        }
        if (feature.data.selected.Length > 0)
        {
            string selectedSkills = string.Join(", ", feature.data.selected.Select(SanitizeValue));
            builder.AppendLine(",");
            builder.Append($"{indent}\tselected: [ {selectedSkills} ]");
        }
        builder.AppendLine();
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertDomainChoiceFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        string idEnding = feature.data.count > 1 ? "," : string.Empty;
        builder.AppendLine("FactoryLogic.feature.createDomainChoice({");
        builder.AppendLine($"{indent}\tid: '{feature.id}'{idEnding}");
        if(feature.data.count > 1)
        {
            builder.AppendLine($"{indent}\tcount: {feature.data.count}");
        }
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertAbilityFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createAbility({");
        ConvertAbility(builder, feature.data.ability, true, $"{indent}\t", true);
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertKitFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string types = string.Join(", ", feature.data.types.Where(type => !string.IsNullOrEmpty(type)).Select(SanitizeValue));
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createKitChoice({");
        builder.Append($"{indent}\tid: '{feature.id}'");
        if(feature.name != "Kit")
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tname: {SanitizeValue(feature.name)}");
        }
        if (!string.IsNullOrEmpty(types))
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\ttypes: [ {types} ]");
        }
        if (feature.data.count > 1)
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tcount: {feature.data.count}");
        }
        builder.AppendLine();
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertDomainFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createDomainFeature({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tlevel: {feature.data.level}");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertClassAbilityChoiceFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createClassAbilityChoice({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.Append($"{indent}\tcost: {SanitizeValue(feature.data.cost)}");
        if (feature.data.minLevel > 1)
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tminLevel: {SanitizeValue(feature.data.minLevel)}");
        }
        if (feature.data.count > 1)
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tcount: {SanitizeValue(feature.data.count)}");
        }
        builder.AppendLine();
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertPerkFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createPerk({");
        builder.Append($"{indent}\tid: '{feature.id}'");
        if (0 < feature.data.lists.Length && feature.data.lists.Length < 6)
        {
            builder.AppendLine(",");
            string listOptions = string.Join(", ", feature.data.lists.Select(option => $"PerkList.{option}"));
            builder.Append($"{indent}\tlists: [ {listOptions} ]");
        }
        builder.AppendLine();
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertTextFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.create({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        builder.AppendLine($"{indent}\tdescription: {SanitizeValue(feature.description)}");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertPackageContentFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createPackageContent({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        builder.AppendLine($"{indent}\tdescription: {SanitizeValue(feature.description)},");
        builder.AppendLine($"{indent}\ttag: '{feature.data.tag}'");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertChoiceFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createChoice({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        if (!string.IsNullOrEmpty(feature.name) && feature.name != "Choice")
        {
            builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        }
        if (!string.IsNullOrEmpty(feature.description))
        {
            builder.AppendLine($"{indent}\tdescription: {SanitizeValue(feature.description)},");
        }
        builder.AppendLine($"{indent}\toptions: [");
        for(int index = 0; index < feature.data.options.Length; index++)
        {
            Feature featureChoice = (Feature)feature.data.options[index];
            string choiceEnding = index == feature.data.options.Length - 1 ? string.Empty : ",";
            builder.AppendLine($"{indent}\t\t{{");
            ConvertFeature(builder, featureChoice, true, $"{indent}\t\t\t", false);
            builder.AppendLine($"{indent}\t\t\tvalue: 1");
            builder.AppendLine($"{indent}\t\t}}{choiceEnding}");
        }
        builder.AppendLine($"{indent}\t]");
        builder.AppendLine($"{indent}}}){ending}");

    }

    private static void ConvertPackageFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createPackage({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        builder.AppendLine($"{indent}\tdescription: {SanitizeValue(feature.description)},");
        builder.AppendLine($"{indent}\ttag: '{feature.data.tag}'");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertAbilityDamageFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string keywords = string.Join(", ", feature.data.keywords.Select(option => $"AbilityKeyword.{option}"));
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createAbilityDamage({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        if (!string.IsNullOrEmpty(feature.description))
        {
            builder.AppendLine($"{indent}\tdescription: {SanitizeValue(feature.description)},");
        }
        builder.AppendLine($"{indent}\tkeywords: [ {keywords} ],");
        builder.AppendLine($"{indent}\tvalue: {feature.data.value}");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertAbilityDistanceFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string keywords = string.Join(", ", feature.data.keywords.Select(option => $"AbilityKeyword.{option}"));
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createAbilityDistance({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        if (!string.IsNullOrEmpty(feature.description))
        {
            builder.AppendLine($"{indent}\tdescription: {SanitizeValue(feature.description)},");
        }
        builder.AppendLine($"{indent}\tkeywords: [ {keywords} ],");
        builder.AppendLine($"{indent}\tvalue: {feature.data.value}");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertMultipleFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createMultiple({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        if (!string.IsNullOrEmpty(feature.description) && feature.description != string.Join(", ", feature.data.features.Select(innerFeature => innerFeature.name)))
        {
            builder.AppendLine($"{indent}\tdescription: {SanitizeValue(feature.description)},");
        }
        builder.AppendLine($"{indent}\tfeatures: [");
        for (int index = 0; index < feature.data.features.Length; index++)
        {
            Feature featureEntry = feature.data.features[index];
            ConvertFeature(builder, featureEntry, false, $"{indent}\t\t", index == feature.data.features.Length - 1);
        }
        builder.AppendLine($"{indent}\t]");
        builder.AppendLine($"{indent}}}){ending}");

    }

    private static void ConvertProficiencyFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string weapons = string.Join(", ", feature.data.weapons.Select(weapon => $"KitWeapon.{weapon.Replace("Weapon", string.Empty).Trim()}"));
        string armors = string.Join(", ", feature.data.armor.Select(armor => $"KitArmor.{armor.Replace("Armor", string.Empty).Trim()}"));
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createProficiency({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tweapons: [ {weapons} ],");
        builder.AppendLine($"{indent}\tarmor: [ {armors} ]");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertDamageModifierFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createDamageModifier({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        if (feature.name != "Damage Modifier")
        {
            builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        }
        if (!string.IsNullOrEmpty(feature.description) && feature.data.modifiers.Length > 1)
        {
            builder.AppendLine($"{indent}\tdescription: {SanitizeValue(feature.description)},");
        }
        builder.AppendLine($"{indent}\tmodifiers: [");
        for(int index = 0; index < feature.data.modifiers.Length; index++)
        {
            DamageModifier modifier = feature.data.modifiers[index];
            bool isLastModifier = index == feature.data.modifiers.Length - 1;
            ConvertDamageModifier(builder, modifier, $"{indent}\t\t", isLastModifier);
        }
        builder.AppendLine($"{indent}\t]");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertDamageModifier(StringBuilder builder, DamageModifier damageModifier, string indent, bool isLast)
    {
        if (damageModifier.valueCharacteristics.Length > 0)
        {
            ConvertCharacteristicDamageModifier(builder, damageModifier, indent, isLast);
        }
        else if (damageModifier.value > 0 && damageModifier.valuePerLevel > 0)
        {
            ConvertValuePlusPerLevelDamageModifier(builder, damageModifier, indent, isLast);
        }
        else
        {
            builder.AppendLine($"ERROR: UNKNOWN DAMAGE MODIFIER");
        }
    }

    private static void ConvertCharacteristicDamageModifier(StringBuilder builder, DamageModifier damageModifier, string indent, bool isLast)
    {
        string characteristics = string.Join(", ", damageModifier.valueCharacteristics.Select(characteristic => $"Characteristic.{characteristic}"));
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine($"{indent}FactoryLogic.damageModifier.createCharacteristic({{");
        builder.AppendLine($"{indent}\tdamageType: DamageType.{damageModifier.damageType},");
        builder.AppendLine($"{indent}\tmodifierType: DamageModifierType.{damageModifier.type},");
        builder.Append($"{indent}\tcharacteristics: [ {characteristics} ]");
        if(damageModifier.valueCharacteristicMultiplier > 1)
        {
            builder.AppendLine(",");
            builder.Append($"{indent}\tmultiplier: {damageModifier.valueCharacteristicMultiplier}");
        }
        builder.AppendLine();
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertValuePlusPerLevelDamageModifier(StringBuilder builder, DamageModifier damageModifier, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine($"{indent}FactoryLogic.damageModifier.createValuePlusPerLevel({{");
        builder.AppendLine($"{indent}\tdamageType: DamageType.{damageModifier.damageType},");
        builder.AppendLine($"{indent}\tmodifierType: DamageModifierType.{damageModifier.type},");
        builder.AppendLine($"{indent}\tvalue: {damageModifier.value - damageModifier.valuePerLevel},");
        builder.AppendLine($"{indent}\tperLevel: {damageModifier.valuePerLevel}");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertLanguageFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createLanguage({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        builder.AppendLine($"{indent}\tlanguage: {SanitizeValue(feature.data.language)}");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertSkillFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createSkill({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        if(feature.name != feature.data.skill)
        {
            builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)}");
        }
        builder.AppendLine($"{indent}\tskill: {SanitizeValue(feature.data.skill)}");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertCharacteristicBonusFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createLanguage({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        if (feature.name != feature.data.characteristic)
        {
            builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        }
        builder.AppendLine($"{indent}\tcharacteristic: {SanitizeValue(feature.data.characteristic)},");
        builder.AppendLine($"{indent}\tvalue: {feature.data.value}");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertHeroicResourceGainFeature(StringBuilder builder, Feature feature, string indent, bool isLast)
    {
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine("FactoryLogic.feature.createLanguage({");
        builder.AppendLine($"{indent}\tid: '{feature.id}',");
        if (feature.name != "Herioc Resource Gain")
        {
            builder.AppendLine($"{indent}\tname: {SanitizeValue(feature.name)},");
        }
        builder.AppendLine($"{indent}\ttrigger: {SanitizeValue(feature.data.trigger)},");
        builder.AppendLine($"{indent}\tvalue: {SanitizeValue(feature.data.value)}");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertAbility(StringBuilder builder, Ability ability, bool fromFeature, string indent, bool isLast)
    {
        string begining = fromFeature ? "ability: " : string.Empty;
        string ending = isLast ? string.Empty : ",";
        builder.AppendLine($"{indent}{begining}FactoryLogic.createAbility({{");
        builder.AppendLine($"{indent}\tid: '{ability.id}',");
        builder.AppendLine($"{indent}\tname: {SanitizeValue(ability.name)},");
        builder.AppendLine($"{indent}\tdescription: {SanitizeValue(ability.description)},");
        ConvertAbilityType(builder, ability.type, indent);
        if (ability.keywords.Length > 0)
        {
            string keywords = string.Join(", ", ability.keywords.Select(option => $"AbilityKeyword.{option}"));
            builder.AppendLine($"{indent}\tkeywords: [ {keywords} ],");
        }
        ConvertAbilityDistances(builder, ability.distance, indent);
        builder.AppendLine($"{indent}\ttarget: '{ability.target}',");
        if (ability.cost is not int costValue || costValue != 0)
        {
            builder.AppendLine($"{indent}\tcost: {SanitizeValue(ability.cost)},");
        }
        if(ability.minLevel > 1)
        {
            builder.AppendLine($"{indent}\tminLevel: {ability.minLevel},");
        }
        builder.AppendLine($"{indent}\tsections: [");
        for(int index = 0; index < ability.sections.Length; index++)
        {
            AbilitySection section = ability.sections[index];
            bool isLastSection = index == ability.sections.Length - 1;
            ConvertAbilitySection(builder, section, indent, isLastSection);
        }
        builder.AppendLine($"{indent}\t]");
        builder.AppendLine($"{indent}}}){ending}");
    }

    private static void ConvertAbilityType(StringBuilder builder, AbilityType type, string indent)
    {
        switch(type.usage)
        {
            case "Maneuver":
                builder.Append($"{indent}\ttype: FactoryLogic.type.createManeuver(");
                if (type.free)
                {
                    builder.Append("{ free: true }");
                }
                builder.AppendLine("),");
                break;
            case "Triggered Action":
                builder.Append($"{indent}\ttype: FactoryLogic.type.createTrigger('{type.trigger}'");
                if(type.free)
                {
                    builder.Append(", { free: true }");
                }
                builder.AppendLine("),");
                break;
            case "Action":
            case "Main Action":
                builder.Append($"{indent}\ttype: FactoryLogic.type.createMain(");
                if(type.qualifiers.Length > 0)
                {
                    string qualifiers = string.Join(", ", type.qualifiers.Select(SanitizeValue));
                    builder.Append($"{{ qualifiers: [ {qualifiers} ] }}");
                }
                builder.AppendLine("),");
                break;
            case "No Action":
                builder.AppendLine($"{indent}\ttype: FactoryLogic.type.createNoAction(),"); break;
            default:
                builder.AppendLine($"ERROR: UNKNOWN ABILITY TYPE"); break;
        }
    }

    private static void ConvertAbilityDistances(StringBuilder builder, AbilityDistance[] distances, string indent)
    {
        builder.Append($"{indent}\tdistance: [");
        if (distances.Length > 1)
        {
            builder.AppendLine();
            builder.Append($"{indent}\t\t");
        }
        else
        {
            builder.Append(" ");
        }
        for(int index = 0; index < distances.Length; index++)
        {
            AbilityDistance distance = distances[index];
            switch (distance.type)
            {
                case "Ranged":
                    builder.Append($"FactoryLogic.distance.createRanged({distance.value})"); break;
                case "Cube":
                    builder.Append($"FactoryLogic.distance.create({{ type: AbilityDistanceType.Cube, value: {distance.value}, within: {distance.within} }})"); break;
                case "Melee":
                    builder.Append("FactoryLogic.distance.createMelee(");
                    if(distance.value > 1)
                    {
                        builder.Append(distance.value);
                    }
                    builder.Append(")");
                    break;
                case "Aura":
                    builder.Append($"FactoryLogic.distance.create({{ type: AbilityDistanceType.Aura, value: {distance.value} }})"); break;
                case "Burst":
                    builder.Append($"FactoryLogic.distance.create({{ type: AbilityDistanceType.Burst, value: {distance.value} }})"); break;
                case "Self":
                    builder.Append("FactoryLogic.distance.createSelf()"); break;
                case "Wall":
                    builder.Append($"FactoryLogic.distance.create({{ type: AbilityDistanceType.Wall, value: {distance.value}, within: {distance.within} }})"); break;
                case "Line":
                    builder.Append($"FactoryLogic.distance.create({{ type: AbilityDistanceType.Line, value: {distance.value}, value2: {distance.value2}, within: {distance.within} }})"); break;
                case "Special":
                    builder.Append($"FactoryLogic.distance.createSpecial({SanitizeValue(distance.special)})"); break;
                default:
                    builder.Append($"ERROR: UNKNOWN ABILITY DISTANCE"); break;
            }
            if(index < distances.Length - 1)
            {
                builder.AppendLine(",");
                builder.Append($"{indent}\t\t");
            }
        }
        if (distances.Length > 1)
        {
            builder.AppendLine();
            builder.Append($"{indent}\t");
        }
        else
        {
            builder.Append(" ");
        }
        builder.AppendLine("],");
    }

    private static void ConvertAbilitySection(StringBuilder builder, AbilitySection section, string indent, bool isLast)
    {
        indent = $"{indent}\t\t";
        string ending = isLast ? string.Empty : ",";
        switch (section.type)
        {
            case "text":
                builder.AppendLine($"{indent}FactoryLogic.createAbilitySectionText({SanitizeValue(section.text)}){ending}"); break;
            case "package":
                builder.AppendLine($"{indent}FactoryLogic.createAbilitySectionPackage({SanitizeValue(section.tag)}){ending}"); break;
            case "field":
                builder.AppendLine($"{indent}FactoryLogic.createAbilitySectionField({{");
                builder.AppendLine($"{indent}\tname: {SanitizeValue(section.name)},");
                if (section.value > 0)
                {
                    builder.AppendLine($"{indent}\tvalue: {SanitizeValue(section.value)},");
                }
                if(section.repeatable)
                {
                    builder.AppendLine($"{indent}\trepeatable: true,");
                }
                builder.AppendLine($"{indent}\teffect: {SanitizeValue(section.effect)}");
                builder.AppendLine($"{indent}}}){ending}");
                break;
            case "roll":
                string characteristics = string.Join(", ", section.roll.characteristic.Select(characteristic => $"Characteristic.{characteristic}"));
                builder.AppendLine($"{indent}FactoryLogic.createAbilitySectionRoll(");
                builder.AppendLine($"{indent}\tFactoryLogic.createPowerRoll({{");
                builder.AppendLine($"{indent}\t\tcharacteristic: [ {characteristics} ],");
                builder.AppendLine($"{indent}\t\ttier1: {SanitizeValue(section.roll.tier1)},");
                builder.AppendLine($"{indent}\t\ttier2: {SanitizeValue(section.roll.tier2)},");
                builder.AppendLine($"{indent}\t\ttier3: {SanitizeValue(section.roll.tier3)}");
                builder.AppendLine($"{indent}\t}})");
                builder.AppendLine($"{indent}){ending}");
                break;
            default:
                builder.AppendLine($"ERROR: UNKNOWN ABILITY SECTION"); break;
        }

    }

    private static string SanitizeValue(object value)
    {
        string wrapper = "";
        if (value is string stringValue)
        {
            if (stringValue.Contains('\n'))
            {
                wrapper = "`";
            }
            else
            {
                wrapper = "'";
                value = stringValue.Replace("'", "\\'");
            }
        }
        return $"{wrapper}{value}{wrapper}";
    }
}
