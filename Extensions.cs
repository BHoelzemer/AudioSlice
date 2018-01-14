using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSlicer
{
    public static class Extensions
    {
        public static int Digits(this int Value)
        {
            return ((Value == 0) ? 1 : ((int)Math.Floor(Math.Log10(Math.Abs(Value))) + 1));
        }
    }
}
