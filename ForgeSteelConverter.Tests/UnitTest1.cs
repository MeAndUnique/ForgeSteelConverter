using ForgeSteelConverter.Conversion;
using ForgeSteelConverter.Models;
using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace ForgeSteelConverter.Tests
{
    public class FunctionalTest
    {
        [Fact]
        public void TestMonsterConversion()
        {
            string sourceFile = ""; // put file path here
            string jsonString = File.ReadAllText(sourceFile);
            MonsterGroup data = JsonSerializer.Deserialize<MonsterGroup>(jsonString)!;
            string file = ClassConverter.ConvertMonsters(data);
            Debugger.Break();
        }
    }
}