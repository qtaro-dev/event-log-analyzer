using System.Collections.Generic;

namespace LogAnalyzer.Models
{
    public sealed class UserConfig
    {
        public string SelectedPiePaletteId { get; set; } = string.Empty;
        public string SelectedBarPaletteId { get; set; } = string.Empty;
        public string BarLabelTextSlot { get; set; } = string.Empty;
        public List<GraphPalette> PiePalettes { get; set; } = new();
        public List<GraphPalette> BarPalettes { get; set; } = new();
    }
}
