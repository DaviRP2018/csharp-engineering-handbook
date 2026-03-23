using System.Globalization;

namespace AsyncProgramming;

public class StockPrice
{
    public required string Identifier { get; set; }
    public DateTime TradeDate { get; set; }
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? Close { get; set; }
    public int Volume { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }

    public string ValueToString()
    {
        return $"Stock: {Identifier} | Trade: {TradeDate} | Change: {Change} | Volume: {Volume}";
    }

    public static StockPrice FromCsv(string text)
    {
        var segments = text.Split(",");

        for (var i = 0; i < segments.Length; i++) segments[i] = segments[i].Trim('\'', '"');

        var price = new StockPrice
        {
            Identifier = segments[0],
            TradeDate = DateTime.ParseExact(segments[1], "M/d/yyyy h:mm:ss tt",
                CultureInfo.InvariantCulture),
            Volume = Convert.ToInt32(segments[6], CultureInfo.InvariantCulture),
            Change = Convert.ToDecimal(segments[7], CultureInfo.InvariantCulture),
            ChangePercent = Convert.ToDecimal(segments[8], CultureInfo.InvariantCulture)
        };

        return price;
    }
}
