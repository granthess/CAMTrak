using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAMTrak.Model.Scale
{
    public class RailCode
    {
        public string Manufacturer {get;private set;}
        public string Product {get;private set;}
        public string Scale { get; private set; }
        public int Code { get; private set; }
        public double Gauge { get; private set; }
        public double RailWidth { get; private set; }
        public double RailHeight { get; private set; }


        public RailCode(string Scale, string Descriptor, int Code, double Gauge, double Width, double Height)
        {
            ParseDescriptor(Descriptor);
            this.Scale = Scale;
            this.Code = Code;
            this.Gauge = Gauge;

            this.RailWidth = Width;
            this.RailHeight = Height;
        }

        private void ParseDescriptor(string Descriptor)
        {
            var parts = Descriptor.Split(":".ToCharArray());

            Manufacturer = parts[0].Trim();
            Product = parts[1].Trim();
        }

    }
}
