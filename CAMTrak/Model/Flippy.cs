using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAMTrak.Model
{
    public class Flippy
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsTrue { get; set; }
        public DateTime When { get; set; }

        public Flippy()
        {
            Name = "Flippy";
            Value = "74";
            IsTrue = true;
            When = DateTime.Now;
        }
    }
}
