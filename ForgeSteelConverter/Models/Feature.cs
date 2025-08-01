using System.Text.Json;

namespace ForgeSteelConverter.Models
{
    public class Feature : Element
    {
        public string type { get; set; }

        public FeatureData data { get; set; }
    }
}
