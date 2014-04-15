using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAMTrak.Model
{
    public interface IDragHandle
    {
        int Width { get; set; }
        int Height { get; set; }
        int Left { get; set; }
        int Top { get; set; }
    }
}
