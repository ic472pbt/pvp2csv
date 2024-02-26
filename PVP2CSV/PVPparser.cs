using Microsoft.VisualBasic;

namespace PVP2CSV
{
    // https://en.wikipedia.org/wiki/Compound_File_Binary_Format
    using OpenMcdf;
    public class PVPparser : IDisposable
    {
        private readonly CompoundFile file;
        private readonly BinaryReader reader;
        private readonly Units u;

        // keep results for the export stage
        // store string values only
        private Dictionary<string, string> resultsString = new();
        // store float values
        private Dictionary<string, (float value, string unit)> resultsFloat = new();
        // store series
        private Dictionary<string, (float[] values, string unit)> resultsSeries = new();

        public PVPparser(string fileName, bool convertToMetric)
        {
            // open compound file
            file = new CompoundFile(fileName);
            CFStream foundStream = file.RootStorage.GetStream("Session");
            MemoryStream stream = new(foundStream.GetData());
            reader = new(stream);

            u = new(convertToMetric);
        }

        public (Dictionary<string, string> strings, 
                Dictionary<string, (float value, string unit)> floats, 
                Dictionary<string, (float[] values, string unit)> series)  Parse()
        {
            resultsString["Header"] = reader.ReadInt32().ToString();
            resultsString["Version"] = reader.ReadInt32().ToString();
            resultsString["Signature"] = reader.ReadInt32().ToString();
            resultsString["DateTime"] = DateTime.FromOADate(reader.ReadDouble()).ToString("dd.MM.yyyy hh:mm:ss");
            resultsString["Junk0"] = reader.ReadInt32().ToString();
            resultsString["Junk"] = new string(reader.ReadChars(4));
            resultsString["Junk1"] = reader.ReadInt32().ToString();

            // read members with units of measure
            int length = reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                int charsLength = reader.ReadInt32();
                string propertyName = new(reader.ReadChars(charsLength));
                // read value and unit
                var vu = u.SelectUnit(reader.ReadSingle(), reader.ReadInt32());
                resultsFloat[propertyName] = (vu.Item1, Units.Unit(vu.Item2));
            }

            // read string members
            int length2 = reader.ReadInt32();
            for (int i = 0; i < length2; i++)
            {
                int nameLength = reader.ReadInt32();
                string propertyName = new(reader.ReadChars(nameLength));
                int valueLength = reader.ReadInt32();
                resultsString[propertyName] = new string(reader.ReadChars(valueLength));
            }

            resultsString["Type"] = new string(reader.ReadChars(reader.ReadInt32()));

            resultsString["Junk2"] = reader.ReadBytes(0x21).ToString();

            // read members with units of measure
            int length3 = reader.ReadInt32();
            for (int i = 0; i < length3; i++)
            {
                int charsLength = reader.ReadInt32();
                string propertyName = new(reader.ReadChars(charsLength));
                var vu = u.SelectUnit(reader.ReadSingle(), reader.ReadInt32());
                resultsFloat[propertyName] = (vu.Item1, Units.Unit(vu.Item2));
            }

            // read string members
            int length4 = reader.ReadInt32();
            for (int i = 0; i < length4; i++)
            {
                int nameLength = reader.ReadInt32();
                string propertyName = new(reader.ReadChars(nameLength));
                int valueLength = reader.ReadInt32();
                resultsString[propertyName] = new string(reader.ReadChars(valueLength));
            }

            // read series members
            int length5 = reader.ReadInt32();
            for (int i = 0; i < length5; i++)
            {
                string propertyName = new(reader.ReadChars(reader.ReadInt32()));
                int junk = reader.ReadInt32();
                int unit = u.SelectUnit(0.0F, reader.ReadInt32()).Item2;
                int junk1 = reader.ReadInt32();
                int size = reader.ReadInt32();
                float[] value = new float[size];
                for (int k = 0; k < size; k++)
                {
                    var vu = u.SelectUnit(reader.ReadSingle(), unit);
                    value[k] = vu.Item1;
                }
                resultsSeries[propertyName] = (value, Units.Unit(unit));
            }

            return (resultsString, resultsFloat, resultsSeries);
        }

        // export stage. It will fail before Parse().
        public void ExportToCSV(string fileName)
        {
            File.WriteAllLines(
                fileName,
                resultsString.
                    Where(kv => !kv.Key.StartsWith("Junk")). // remove junk/unknown fields
                    Select(kv => $"{kv.Key}; {kv.Value}").
                Concat(resultsFloat.
                        Where(kv => !kv.Key.StartsWith("Junk")). // remove junk/unknown fields
                        Select(kv => $"{kv.Key}; {kv.Value.value}; {kv.Value.unit}")).
                Concat(resultsSeries.
                        Select(kv => $"{kv.Key}; {kv.Value.unit}; {string.Join(';', kv.Value.values)}"))
            );
        }

        public void Dispose()
        {
            reader.Dispose();
            file.Close();
        }
    }
}