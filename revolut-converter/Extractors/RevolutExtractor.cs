using System.Globalization;
using revolut_converter.Models;

namespace revolut_converter.Extractors;

public class RevolutExtractor : IExtractor
{
    public IEnumerable<CoinTracking> Extract(string file)
    {
        var trades = ExtractTrades(file).ToArray();
        Console.WriteLine($"Read {trades.Length} revolut lines");

        var converted = ConvertToCoinTracking(trades).ToArray();
        Console.WriteLine($"Created {converted.Length} coin tracking lines");

        return converted;
    }

    private IEnumerable<Revolut> ExtractTrades(string file)
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

    private IEnumerable<CoinTracking> ConvertToCoinTracking(IEnumerable<Revolut> trades)
    {
        foreach (var trade in trades)
            switch (trade.Type)
            {
                case "EXCHANGE":
                    // yield return new CoinTracking
                    // {
                    //     Typ = "Einzahlung",
                    //     Börse = "Revolut",
                    //     Gruppe = trade.Description,
                    //     Datum = trade.CompletedDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    //     Kauf = trade.FiatAmount.ToString("F8", new CultureInfo("en-us")),
                    //     CurTo = trade.BaseCurrency
                    // };
                    yield return new CoinTracking
                    {
                        Typ = "Trade",
                        Börse = "Revolut",
                        Datum = trade.CompletedDate.ToString("dd.MM.yyyy HH:mm:ss"),
                        Kauf = trade.Amount.ToString("F8", new CultureInfo("en-us")),
                        CurFrom = trade.BaseCurrency,
                        Verkauf = trade.FiatAmount.ToString("F8", new CultureInfo("en-us")),
                        CurTo = trade.Currency,
                        //Gruppe = trade.Description,
                        Gruppe = "Exchanged to BTC"
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

    private record Revolut
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        private static readonly CultureInfo CultureInfo = new("en-us");

        public Revolut(string[] values)
        {
            Type = values[0];
            Product = values[1];
            StartedDate = DateTime.ParseExact(values[2], DateFormat, CultureInfo);
            CompletedDate = DateTime.ParseExact(values[3], DateFormat, CultureInfo);
            Description = values[4].Trim('"').Trim();
            Amount = decimal.Parse(values[5], CultureInfo);
            Currency = values[6];
            FiatAmount = decimal.Parse(values[7], CultureInfo);
            FiatAmountIncFees = decimal.Parse(values[8], CultureInfo);
            Fee = decimal.Parse(values[9], CultureInfo);
            BaseCurrency = values[10];
            State = values[11];
            Balance = decimal.Parse(values[12], CultureInfo);
        }

        public string Type { get; }
        public string Product { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime CompletedDate { get; }
        public string Description { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public decimal FiatAmount { get; }
        public decimal FiatAmountIncFees { get; set; }
        public decimal Fee { get; set; }
        public string BaseCurrency { get; }
        public string State { get; set; }
        public decimal Balance { get; set; }
    }
}