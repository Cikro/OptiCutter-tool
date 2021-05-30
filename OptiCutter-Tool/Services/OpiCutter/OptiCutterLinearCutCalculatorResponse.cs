using System;
using System.Net;


namespace OptiCutter_Tool.Services.OptiCutter
{
    public class OptiCutterLinearCutCalculatorResponse
    {
        public Cookie SessionCookie { get; set; }
        public Uri CalculatorResultUrl { get; set; }
    }
}
