using PVP2CSV;

bool toMetric = Environment.GetCommandLineArgs().Length > 2 && 
                (Environment.GetCommandLineArgs()[2] == "--metric" 
                 || Environment.GetCommandLineArgs()[2] == "-m");

using PVPparser parser = new(Environment.GetCommandLineArgs()[1], toMetric);
var results = parser.Parse();
foreach(var kv in results.strings)
{
    Console.WriteLine($"{kv.Key} : {kv.Value}");
}

foreach (var kv in results.floats)
{
    // F3 precision
    Console.WriteLine($"{kv.Key} : {kv.Value.value:f3} ({kv.Value.unit})");
}

foreach (var kv in results.series)
{
    Console.WriteLine($"{kv.Key} : ({kv.Value.unit}) {string.Join(';', kv.Value.values.Select(v => v.ToString("f3")))}");
}

var outputCSVname = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[1]) + ".csv";
parser.ExportToCSV(Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[1]), outputCSVname));
