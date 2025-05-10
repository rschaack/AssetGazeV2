using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;

namespace AssetGazeV2;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var godService = new GodService(false);
        var running = true;

        while (running)
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Get a price from the LSE Service");
            Console.WriteLine("2. Get a price frok the FT.com scraper");
            Console.WriteLine("q. Exit");
            Console.Write("Enter your choice: \n");


            var choice = Console.ReadLine();
            var fetchCode = "";
            
            switch (choice)
            {
                case "1":
                    Console.WriteLine("Please enter the code you want to get a price for. (e.g. USSC)");
                    fetchCode = Console.ReadLine();
                    await godService.HandleOptionOne(fetchCode);
                    break;
                case "2":
                    Console.WriteLine("Please enter the ISIN for the asset you want to get a price for. (e.g. USSC)");
                    var isin = Console.ReadLine();
                    Console.WriteLine("Please enter the ftcode. (e.g. AMZN:NSQ)");
                    fetchCode = Console.ReadLine();
                    Console.WriteLine("Please enter the ft asset type.");
                    Console.WriteLine("b for Bonds.");
                    Console.WriteLine("e for Etfs.");
                    Console.WriteLine("s for Equities.");
                    Console.WriteLine("f for Funds.");
                    var assetTypeInput = Console.ReadLine();
                    IPublicTradeable asset1;
                    switch (assetTypeInput)
                    {
                        case "b":
                            asset1 = new Bond(isin, "ft", fetchCode);
                            await godService.HandleOptionTwo(asset1);
                            break;
                        case "e":
                            asset1 = new Etf(isin, "ft", fetchCode);
                            await godService.HandleOptionTwo(asset1);
                            break;
                        case "s":
                            asset1 = new Equity(isin, "ft", fetchCode);
                            await godService.HandleOptionTwo(asset1);
                            break;
                        case "f":
                            asset1 = new Equity(isin, "ft", fetchCode);
                            await godService.HandleOptionTwo(asset1);
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break; 
                    }
                    
                    
                    break;
                case "q": // Allow 'q' or 'Q' as an alternative exit
                case "Q":
                    running = false;
                    Console.WriteLine("Exiting application...");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
        Console.WriteLine("Application closed.");
    }
}

public class GodService
{
    public GodService(bool isTestMode)
    {
    }

    public async Task HandleOptionOne(string? fetchCode)
    {
        Console.WriteLine("Fetching LseService.");
        var price = new LseService();
        var priceVal = await price.FetchPrice(fetchCode);
        Console.WriteLine($"The price for {fetchCode} is {priceVal}.");
    }

    public async Task HandleOptionTwo(IPublicTradeable asset)
    {
        Console.WriteLine("Fetching FTScraper.");
        var price = new FtScraper();
        var priceVal = await price.FetchPrice(asset);
        Console.WriteLine($"The price for {asset.ISIN} is {priceVal}.");
    }
}

public class LseService()
{
    public async Task<decimal> FetchPrice(string? fetchCode)
    {
        var httpClient = new HttpClient();
        var price = 0m;
        try
        {
            var ticker = fetchCode;
            var url = $"https://api.londonstockexchange.com/api/gw/lse/instruments/alldata/{ticker}";
            var query = new Dictionary<string, string?>
            {
                { "region", "US" },
                { "lang", "en" },
                { "symbols", ticker }
            };

// Build URL with query parameters
            var fullUrl = url + "?" + string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

// Make API call
            var response = await httpClient.GetFromJsonAsync<LseApiResponse>(fullUrl);
            if(response != null)
                price = response.Bid ?? 0m;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message, "Mesage Error");
        }
        return price;
    }
}


public class FtScraper()
{
    public async Task<decimal> FetchPrice(IPublicTradeable asset)
    {
        var httpClient = new HttpClient();
        string fullUrl = BuildUrl(asset);
        var price = 0m;
        try
        {
            var response = await httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();
        
            var htmlContent = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);
            
  
            var priceNode = htmlDocument.DocumentNode.SelectSingleNode("//*[contains(@class, 'mod-ui-data-list__value')]") ??
                            htmlDocument.DocumentNode.SelectSingleNode("//*[contains(@class, 'price')]"); // Fallback
            if (priceNode == null)
            {
                Console.WriteLine($"Debug: HTML content: {htmlDocument.DocumentNode.OuterHtml.Substring(0, 500)}...");
            }
            if (priceNode != null && decimal.TryParse(priceNode.InnerText.Trim(), out decimal priceVal))
            {
                price = priceVal;
            }
            else
            {
                Console.WriteLine($"Warning: Could not find price for {asset.Name}");
                price = decimal.Zero;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching data for {asset.Name} from {fullUrl}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing data for {asset.Name}: {ex.Message}");
        }
        return price;
    }
    
    public string BuildUrl(IPublicTradeable asset)
    {
        string baseUrl = "https://markets.ft.com/data/";
        string category = asset.FtLinkType();
        string priceSourceCode = $"s={asset.PriceSourceCode}"; // Assuming prices are in GBP
        string fullUrl = $"{baseUrl}{category}/tearsheet/summary?{priceSourceCode}";
        return fullUrl;
    }
}



public class LseApiResponse
{
    public LseApiResponse(decimal? bid)
    {
        Bid = bid;
    }

    [JsonPropertyName("bid")]
    public decimal? Bid { get; }
}



public interface IPublicTradeable
{
    string? Name { get; set;  }
    string ISIN { get; set; }
    string? Sedol { get; set; }
    string PriceSource { get; set; }
    string PriceSourceCode { get; set; }
    string? IncomeTreatment { get; set; }
    decimal? TotalExpenseRatio { get; set; }
    string? Denomination { get; set; }
    public string FtLinkType();
}

public abstract class BaseAsset : IPublicTradeable
{
    public string? Name { get; set; }
    public string ISIN { get; set; }
    public string? Sedol { get; set; }
    public string PriceSource { get; set; }
    public string PriceSourceCode { get; set; }
    public string? IncomeTreatment { get; set; }
    public decimal? TotalExpenseRatio { get; set; }
    public string? Denomination { get; set; }
    
    public BaseAsset(string isin, string priceSource, string priceSourceCode)
    {
        ISIN = isin;
        PriceSource = priceSource;
        PriceSourceCode = priceSourceCode;
    }

    public abstract string FtLinkType();
}

public class Bond : BaseAsset
{
    public Bond(string isin, string priceSource, string priceSourceCode) : base(isin, priceSource, priceSourceCode)
    {
    }
    
    public override string FtLinkType()
    {
        return "bond";
    }
}

public class Etf : BaseAsset
{
    public Etf(string isin, string priceSource, string priceSourceCode) : base(isin, priceSource, priceSourceCode)
    {
    }

    public override string FtLinkType()
    {
        return "etf";
    }
}

public class Equity : BaseAsset
{
    public Equity(string isin, string priceSource, string priceSourceCode) : base(isin, priceSource, priceSourceCode)
    {
    }

    public override string FtLinkType()
    {
        return "equities";
    }
}

public class Fund : BaseAsset
{
    public Fund(string isin, string priceSource, string priceSourceCode) : base(isin, priceSource, priceSourceCode)
    {
    }

    public override string FtLinkType()
    {
        return "funds";
    }
}






