using revolut_converter.Extractors;
using revolut_converter.IO;

//var extractor = new RevolutExtractor();
var extractor = new StrikeExtractor();

var files = Directory.GetFiles(args[0]);

var converted = files.SelectMany(file => extractor.Extract(file)).ToArray();
var writer = new CoinTrackingWriter();
var output = await writer.WriteFile(args[1], converted);

Console.WriteLine($"Created file: {output.FullName}");
Console.WriteLine($"File can be imported here: https://cointracking.info/import/import_csv/");