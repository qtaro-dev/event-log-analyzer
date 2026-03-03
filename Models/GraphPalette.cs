using System.Collections.Generic;

namespace LogAnalyzer.Models
{
    public sealed class GraphPalette
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> Colors { get; set; } = new();
    }
}
