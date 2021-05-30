using System.Collections.Generic;

namespace OptiCutter_Tool.Services.OptiCutter
{
    public class OptiCutterLinearCutCalculatorRequest
    {
        public List<OptiCutterLinearCutCalculatorBoard> Stock { get; set; }
        public List<OptiCutterLinearCutCalculatorBoard> Requirements { get; set; }
        public double Kerf { get; set; }
    }
    public class OptiCutterLinearCutCalculatorBoard
    {
        public double Length { get; set; }
        public double Quantity { get; set; }
    }
}