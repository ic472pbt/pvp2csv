using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVP2CSV
{
    internal class Units
    {
        private static Dictionary<int, string> mapping = new()
        {
            [1] = "inch",
            [2] = "m",
            [3] = "cm",
            [4] = "mm",
            [5] = "ips",

            [6] = "MetersPerSecond",

            [7] = "cmPerSecond",

            [8] = "mmPerSecond",

            [9] = "pounds",

            [10] = "Newtons",

            [11] = "KGF",

            [12] = "Farenheit",

            [13] = "Celcius",

            [14] = "RPM",

            [15]
         = "Hz",

            [16]
         = "feet",

            [17]
         = "miles",

            [18]
         = "kilometers",

            [19]
         = "Seconds",

            [20]
         = "Minutes",

            [21]
         = "Hours",

            [22]
         = "fps",

            [23]
         = "MPH",

            [24]
         = "KMPH",

            [25]
         = "G",

            [26]
         = "fps^2",

            [27]
         = "MPerSecond^2",

            [28]
         = "InchPound",

            [29]
         = "FootPound",

            [30]
         = "NewtonMeter",

            [31]
         = "psi",

            [32]
         = "Bar",

            [33]
         = "Pascal",

            [34]
         = "Volts",

            [35]
         = "test not performed",

            [98]
         = "test not performed"
        };
        private Dictionary<int, Func<float, int, (float, int)>> imp2met;
        private readonly bool convert2Metric;
        private static Func<float, int, (float, int)> id = (x, i) => (x, i);
        public Units(bool convert2Metrc)
        {
            convert2Metric = convert2Metrc;
            imp2met = new()
            {
                [1] = (x, i) => (x * 25.4F, 4), // inch -> mm
                [2] = id,
                [3] = id,
                [4] = id,
                [5] = (x, i) => (x * 0.0254F, 6), // ips -> m/s
                [6] = id,
                [7] = id,
                [8] = id,
                [9] = (x, i) => (x * 4.44822F, 10), // pounds -> N,
                [10] = id,
                [11] = id,
                [12] = (x, i) => ((x - 32.0F) * 5.0F / 9.0F, 13), // Farenheit -> C,
                [13] = id,
                [14] = id,
                [15] = id,
                [16] = (x, i) => (x * 0.3048F, 2), // feet -> m,
                [17] = (x, i) => (x * 1.60934F, 18), // miles -> km,
                [18] = id,
                [19] = id,
                [20] = id,
                [21] = id,
                [22] = (x, i) => (x * 1.09728F, 24), // fps -> km/h,
                [23] = (x, i) => (x * 1.60934F, 24), // MPH -> km/h,
                [24] = id,
                [25] = id,
                [26] = (x, i) => (x * 0.304800F, 27), // fps^2 -> m/s^2"
                [27] = id,
                [28] = (x, i) => (x * 0.112984825F, 30), // InchPound -> Nm 
                [29] = (x, i) => (x * 1.356F, 30), // FootPound -> Nm
                [30] = id,
                [31] = (x, i) => (x * 0.0689475729F, 32), // psi -> bar
                [32] = id,
                [33] = id,
                [34] = id,
                [35] = id,
                [98] = id
            };
        }
        public static string Unit(int i)
        {
            return mapping.ContainsKey(i) ? mapping[i] : "?";
        }

        public (float, int) Metric(float value, int index)
        {
            return imp2met.ContainsKey(index) ? imp2met[index](value, index) : id(value, index);
        }

        public static (float, int) AsIs(float value, int index)
        {
            return id(value, index);
        }

        public (float, int) SelectUnit(float value, int index)
        {
            return convert2Metric ? Metric(value, index) : AsIs(value, index);
        }
    }
}
