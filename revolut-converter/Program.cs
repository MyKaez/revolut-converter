using System.Globalization;
using revolut_converter.IO;
using revolut_converter.Models;

var trades = ExtractTrades(args[0]).ToArray();
Console.WriteLine($"Read {trades.Length} revolut lines");

var converted = ConvertToCoinTracking(trades).ToArray();
Console.WriteLine($"Created {converted.Length} coin tracking lines");

var writer = new CoinTrackingWriter();
var output = await writer.WriteFile(args[1], converted);

Console.WriteLine($"Created file: {output.FullName}");
Console.WriteLine($"File can be imported here: https://cointracking.info/import/import_csv/");

IEnumerable<Revolut> ExtractTrades(string file)
{
    using var reader = new StreamReader(file);

    // header
    reader.ReadLine();

    while (reader.Peek() >= 0)
    {
        var parts = reader.ReadLine()?.Split(',');

        if (parts is null)
            continue;

        var trade = new Revolut(parts);

        yield return trade;
    }
}

IEnumerable<CoinTracking> ConvertToCoinTracking(IEnumerable<Revolut> trades)
{
    foreach (var trade in trades)
    {
        switch (trade.Type)
        {
            case "EXCHANGE":
                yield return new CoinTracking
                {
                    Typ = "Einzahlung",
                    Börse = "Revolut",
                    Gruppe = trade.Description,
                    Datum = trade.CompletedDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    Kauf = trade.FiatAmount.ToString("F8", new CultureInfo("en-us")),
                    CurTo = trade.BaseCurrency
                };
                yield return new CoinTracking
                {
                    Typ = "Trade",
                    Börse = "Revolut",
                    Datum = trade.CompletedDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    Kauf = trade.Amount.ToString("F8", new CultureInfo("en-us")),
                    CurFrom = trade.BaseCurrency,
                    Verkauf = trade.FiatAmount.ToString("F8", new CultureInfo("en-us")),
                    CurTo = trade.Currency,
                    Gruppe = trade.Description,
                };
                break;
            case "CRYPTO_WITHDRAWAL":
                var ct = new CoinTracking
                {
                    Typ = "Auszahlung",
                    Börse = "Revolut",
                    Datum = trade.CompletedDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    Kommentar = trade.Description
                };
                Console.WriteLine($"Won't return {ct}");
                break;
            default:
                Console.WriteLine($"Won't handle type {trade.Type}");
                break;
        }
    }
}