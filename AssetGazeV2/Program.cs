
using Microsoft.Extensions.DependencyInjection;

namespace AssetGazeV2;

class Program
{

    
    static void Main(string[] args)
    {
        var godService = new GodService(false);
        bool running = true;

        while (running)
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Run application");
            Console.WriteLine("q. Exit");
            Console.Write("Enter your choice: \n");


            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    godService.HandleOptionOne();
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

    public void HandleOptionOne()
    {
        Console.WriteLine("Do some stuff.");
        Console.WriteLine("Done some stuff.");
    }
}





