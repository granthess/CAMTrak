using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAMTrak.Model.Scale
{
    public class ScaleManager
    {
        public Dictionary<string, double> Scales { get; private set; }
        public Dictionary<string, RailCode> Codes { get; private set; }

        public string CurrentScale { get; private set; }
        public RailCode CurrentCode { get; private set; }
        public double CurrentRatio { get; private set; }

        public ScaleManager()
        {
            PopulateScales();
            PopulateRailCodes();

            CurrentScale = "N";
            CurrentCode = Codes["Atlas:Code 55"];
            CurrentRatio = Scales[CurrentScale];
        }

        private void PopulateScales()
        {
            Scales = new Dictionary<string, double>();

            Scales.Add("HO", 87.0);
            Scales.Add("N", 160.0);
            Scales.Add("Z", 220.0);
            Scales.Add("T", 450.0);
            Scales.Add("S", 64.0);
            Scales.Add("O", 48);
        }

        private void PopulateRailCodes()
        {
            Codes = new Dictionary<string, RailCode>();

            PopulateRailCode("N", "Atlas:Code 80", 80, 0.354, 0.052, 0.80);
            PopulateRailCode("N", "Atlas:Code 55", 55, 0.354, 0.055, 0.55);

        }

        private void PopulateRailCode(string Scale, string Descriptor, int Code, double Gauge, double Width, double Height)
        {
            Codes.Add(Descriptor, new RailCode(Scale, Descriptor, Code, Gauge, Width, Height));
        }

        public double ToPrototype(double value)
        {
            return value * CurrentRatio;
        }

        public double ToScale(double value)
        {
            return value / CurrentRatio;
        }
    }
}
