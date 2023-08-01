using System.Globalization;

namespace revolut_converter.Models;

public record Revolut
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

    public string Type { get; set; }
    public string Product { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime CompletedDate { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public decimal FiatAmount { get; set; }
    public decimal FiatAmountIncFees { get; set; }
    public decimal Fee { get; set; }
    public string BaseCurrency { get; set; }
    public string State { get; set; }
    public decimal Balance { get; set; }
}