using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioSlicer
{
    public static class Extensions
    {
        public static int Digits(this int Value)
        {
            return ((Value == 0) ? 1 : ((int)Math.Floor(Math.Log10(Math.Abs(Value))) + 1));
        }

        public static void InvokeIfRequired(this Control control, MethodInvoker action)
        {

            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();

            }
        }
    }
}
