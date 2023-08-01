namespace revolut_converter.Models;

public record CoinTracking
{
    public string Typ { get; set; }
    public string Kauf { get; set; }
    public string CurTo { get; set; }
    public string Verkauf { get; set; }
    public string CurFrom { get; set; }
    public string Gebühr { get; set; }
    public string CurEmpty { get; set; }
    public string Börse { get; set; }
    public string Gruppe { get; set; }
    public string Kommentar { get; set; }
    public string Datum { get; set; }
}