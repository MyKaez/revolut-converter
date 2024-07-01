using revolut_converter.Models;

namespace revolut_converter.Extractors;

public interface IExtractor
{
    public IEnumerable<CoinTracking> Extract(string file);
}