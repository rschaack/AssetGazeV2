using System.Net.Http.Json;
using System.Text.Json.Serialization;

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
            Console.WriteLine("1. Run application");
            Console.WriteLine("q. Exit");
            Console.Write("Enter your choice: \n");


            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Please enter the code you want to get a price for. (e.g. USSC)");
                    var fetchCode = Console.ReadLine();
                    await godService.HandleOptionOne(fetchCode);
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
        Console.WriteLine("Fetching Price.");
        var price = new Price();
        var priceVal = await price.Fetch(fetchCode);
        Console.WriteLine($"The price for {fetchCode} is {priceVal}.");
    }
}

public class Price()
{
    public async Task<decimal> Fetch(string? fetchCode)
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

public class LseApiResponse
{
    public LseApiResponse(decimal? bid)
    {
        Bid = bid;
    }

    [JsonPropertyName("bid")]
    public decimal? Bid { get; }
}





