using System.Globalization;
using revolut_converter.Models;

namespace revolut_converter.Extractors;

public class StrikeExtractor : IExtractor
{
    public IEnumerable<CoinTracking> Extract(string file)
    {
        var trades = File.ReadAllLines(file).Skip(1).Select(line => new Strike(line.Split(','))).ToArray();
        Console.WriteLine($"Read {trades.Length} revolut lines");

        var converted = ConvertToCoinTracking(trades).ToArray();
        Console.WriteLine($"Created {converted.Length} coin tracking lines");

        return converted;
    }

    private IEnumerable<CoinTracking> ConvertToCoinTracking(Strike[] trades)
    {
        foreach (var trade in trades)
        {
            var ct = CoinTracking(trade);

            if (ct is null)
                continue;

            yield return ct;
        }
    }

    private static CoinTracking? CoinTracking(Strike trade)
    {
        try
        {
            switch (trade.TransactionType)
            {
                case "Trade":
                    return Trade(trade);
                case "Withdrawal":
                    Withdrawl(trade);
                    break;
                case "Lightning":
                    return Lightning(trade);
                default:
                    Console.WriteLine($"Won't handle type {trade.TransactionType}");
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            Console.WriteLine(trade);
        }

        return null;
    }

    private static CoinTracking? Lightning(Strike trade)
    {
        if (string.IsNullOrWhiteSpace(trade.Amount2))
            return null;
        
        var ct = new CoinTracking
        {
            Typ = "Auszahlung",
            Börse = "Strike",
            Verkauf = Convert.ToDecimal(trade.Amount2, new CultureInfo("en-us"))
                .ToString("F8", new CultureInfo("en-us")),
            CurTo = trade.Currency1,
            Kommentar = trade.Description
        };

        if (!string.IsNullOrWhiteSpace(trade.CompletedDateUtc))
        {
            ct.Datum = DateTime.ParseExact(trade.CompletedDateUtc, "MMM dd yyyy",
                    CultureInfo.InvariantCulture).Add(TimeSpan.Parse(trade.CompletedTimeUtc))
                .ToString("dd.MM.yyyy HH:mm:ss");
        }
        else
        {
            ct.Datum = DateTime.ParseExact(trade.InitiatedDateUtc, "MMM dd yyyy",
                    CultureInfo.InvariantCulture).Add(TimeSpan.Parse(trade.InitiatedTimeUtc))
                .ToString("dd.MM.yyyy HH:mm:ss");
        }

        return ct;
    }

    private static void Withdrawl(Strike trade)
    {
        var ct = new CoinTracking
        {
            Typ = "Auszahlung",
            Börse = "Strike",
            Kauf = string.IsNullOrWhiteSpace(trade.Fee1)
                ? 0.ToString("F8", new CultureInfo("en-us"))
                : Convert.ToDecimal(trade.Amount1, new CultureInfo("en-us"))
                    .ToString("F8", new CultureInfo("en-us")),
            CurFrom = trade.Currency1,
            Kommentar = trade.Description
        };

        ct.Datum = DateTime.ParseExact(trade.CompletedDateUtc, "MMM dd yyyy",
                CultureInfo.InvariantCulture).Add(TimeSpan.Parse(trade.CompletedTimeUtc))
            .ToString("dd.MM.yyyy HH:mm:ss");

        Console.WriteLine($"Won't return {ct}");
    }

    private static CoinTracking? Trade(Strike trade)
    {
        var bitcoin = Convert.ToDecimal(trade.Amount2, new CultureInfo("en-us"));
        var euro = Convert.ToDecimal(trade.Amount1, new CultureInfo("en-us"));
        var coinTracking = new CoinTracking
        {
            Typ = "Trade",
            Börse = "Strike",
            Datum =
                DateTime.ParseExact(
                        string.IsNullOrEmpty(trade.CompletedDateUtc)
                            ? trade.InitiatedDateUtc
                            : trade.CompletedDateUtc, "MMM dd yyyy",
                        CultureInfo.InvariantCulture).Add(TimeSpan.Parse(
                        string.IsNullOrEmpty(trade.CompletedTimeUtc)
                            ? trade.InitiatedTimeUtc
                            : trade.CompletedTimeUtc))
                    .ToString("dd.MM.yyyy HH:mm:ss"),
            Verkauf = euro
                .ToString("F8", new CultureInfo("en-us")),
            CurFrom = trade.Currency1,
            Gebühr = string.IsNullOrWhiteSpace(trade.Fee1)
                ? 0.ToString("F8", new CultureInfo("en-us"))
                : Convert.ToDecimal(trade.Fee1, new CultureInfo("en-us"))
                    .ToString("F8", new CultureInfo("en-us")),
            Kommentar = trade.TransactionId,
            Gruppe = "Exchanged to BTC"
        };

        if (bitcoin > 0)
        {
            coinTracking.Kauf = bitcoin.ToString("F8", new CultureInfo("en-us"));
            coinTracking.CurTo = trade.Currency2;

            coinTracking.Verkauf = (-euro).ToString("F8", new CultureInfo("en-us"));
            coinTracking.CurFrom = trade.Currency1;
        }
        else
        {
            coinTracking.Kauf = euro.ToString("F8", new CultureInfo("en-us"));
            coinTracking.CurTo = trade.Currency1;

            coinTracking.Verkauf = (-bitcoin).ToString("F8", new CultureInfo("en-us"));
            coinTracking.CurFrom = trade.Currency2;
        }

        return coinTracking;
    }

    private record Strike
    {
        public Strike(string[] args)
        {
            TransactionId = args[0];
            InitiatedDateUtc = args[1];
            InitiatedTimeUtc = args[2];
            CompletedDateUtc = args[3];
            CompletedTimeUtc = args[4];
            TransactionType = args[5];
            State = args[6];
            Amount1 = args[7];
            Currency1 = args[8];
            Fee1 = args[9];
            Amount2 = args[10];
            Currency2 = args[11];
            Fee2 = args[12];
            BtcPrice = args[13];
            Balance1 = args[14];
            BalanceCurrency1 = args[15];
            BalanceMinusBtc = args[16];
            Destination = args[17];
            Description = args[18];
        }

        public string TransactionId { get; set; }

        public string InitiatedDateUtc { get; set; }

        public string InitiatedTimeUtc { get; set; }

        public string CompletedDateUtc { get; set; }

        public string CompletedTimeUtc { get; set; }

        public string TransactionType { get; set; }

        public string State { get; set; }

        public string Amount1 { get; set; }

        public string Currency1 { get; set; }

        public string Fee1 { get; set; }

        public string Amount2 { get; set; }

        public string Currency2 { get; set; }

        public string Fee2 { get; set; }

        public string BtcPrice { get; set; }

        public string Balance1 { get; set; }

        public string BalanceCurrency1 { get; set; }

        public string BalanceMinusBtc { get; set; }

        public string Destination { get; set; }

        public Strike(string description, string btcPrice)
        {
            Description = description;
            BtcPrice = btcPrice;
        }

        public string Description { get; set; }
    }
}