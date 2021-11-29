using System.Collections.Generic;

namespace JumpenoWebassembly.Shared
{
    public class ChartData
    {
        public string Label { get; set; }
        public string BorderColor { get; set; }
        public bool Fill { get; set; }
        public List<double> Data { get; set; }
    }
}
