using revolut_converter.Models;

namespace revolut_converter.IO;

public class CoinTrackingWriter
{
    public async Task<FileInfo> WriteFile(string filePath, CoinTracking[] input)
    {
        await using var writer = new StreamWriter(filePath, false);

        var header = new[]
            { "Typ", "Kauf", "Cur.", "Verkauf", "Cur.", "Gebühr", "Cur.", "Börse", "Gruppe", "Kommentar", "Datum" };

        await writer.WriteLineAsync(string.Join(",", header.Select(t => $"\"{t}\"")));
        
        foreach (var trade in input)
        {
            var values = new[]
            {
                trade.Typ, trade.Kauf, trade.CurTo, trade.Verkauf, trade.CurFrom, trade.Gebühr,
                trade.CurEmpty, trade.Börse, trade.Gruppe, trade.Kommentar, trade.Datum
            };
            var line = string.Join(",", values.Select(t => $"\"{t}\""));
            
            await writer.WriteLineAsync(line);
        }

        return new FileInfo(filePath);
    }
}