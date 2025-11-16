using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main()
    {
        Console.Clear();
        Console.Title = "Hypixel Bed Wars Stats Viewer";

        // Read API Key
        string apiKey = LoadApiKey();
        if (apiKey == null)
        {
            Console.WriteLine("Could not read API key from api_key.txt.");
            Console.WriteLine("Create api_key.txt and write your Hypixel API key in it.");
            Wait();
            return;
        }

        Console.Write("Enter Minecraft Username: ");
        string username = Console.ReadLine();

        try
        {
            // Get UUID
            string uuid = await GetUUIDFromUsername(username);
            if (uuid == null)
            {
                Console.WriteLine("Could not get UUID. The specified username may not exist.");
                Wait();
                return;
            }

            // Bedwars (Solo) statistics acquisition
            var stats = await GetBedwarsStats(apiKey, uuid);

            Console.WriteLine($"\n\n\n{username}'s Hypixel BedWars (Solo) Stats:\n");
            Console.WriteLine($"Kills: {stats.Kills}");
            Console.WriteLine($"Deaths: {stats.Deaths}");
            Console.WriteLine($"Bed Breaks: {stats.BedsBroken}");
            Console.WriteLine($"Wins: {stats.Wins}");
            Console.WriteLine($"Losses: {stats.Losses}");
            Console.WriteLine($"Final Kills: {stats.FinalKills}");

            Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error has occurred: " + ex.Message);
            Wait();
        }
    }


    // ===============================================================
    // Read the API key from the same folder as the executable file
    // ===============================================================
    static string LoadApiKey()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api_key.txt");

        if (!File.Exists(filePath))
            return null;

        string key = File.ReadAllText(filePath).Trim();

        if (string.IsNullOrWhiteSpace(key))
            return null;

        return key;
    }


    // ===================================================
    // Get UUID from Mojang API
    // ===================================================
    static async Task<string> GetUUIDFromUsername(string username)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://api.mojang.com/users/profiles/minecraft/{username}";
            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
                return null;

            string json = await res.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);

            return obj["id"]?.ToString();
        }
    }


    // ===================================================
    // Get Bedwars Solo stats from the Hypixel API
    // ===================================================
    static async Task<BedwarsStats> GetBedwarsStats(string apiKey, string uuid)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://api.hypixel.net/player?key={apiKey}&uuid={uuid}";
            var res = await client.GetAsync(url);
            string json = await res.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            var bw = data["player"]?["stats"]?["Bedwars"];

            return new BedwarsStats
            {
                Kills = bw?["eight_one_kills_bedwars"]?.Value<int>() ?? 0,
                Deaths = bw?["eight_one_deaths_bedwars"]?.Value<int>() ?? 0,
                BedsBroken = bw?["eight_one_beds_broken_bedwars"]?.Value<int>() ?? 0,
                Wins = bw?["eight_one_wins_bedwars"]?.Value<int>() ?? 0,
                Losses = bw?["eight_one_losses_bedwars"]?.Value<int>() ?? 0,
                FinalKills = bw?["eight_one_final_kills_bedwars"]?.Value<int>() ?? 0
            };
        }
    }


    // ===================================================
    // Wait for any key
    // ===================================================
    static void Wait()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}


// =======================================================
// Bedwars Statistics Class
// =======================================================
class BedwarsStats
{
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int BedsBroken { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int FinalKills { get; set; }
}
