using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

public class CatFact
{
    public string? Fact { get; set; }
    public int Length { get; set; }
    public string DateTime { get; set; }
}

public class MySQLConnector
{
    static async Task<List<CatFact>> GetCatFacts()
    {
        // Create Http Client
        using HttpClient client = new HttpClient();

        // Create a list to store cat facts
        List<CatFact> facts = new List<CatFact>();

        // Send a GET request 5 times
        for (int i = 0; i < 5; i++)
        {
            try
            {
                // Get date and time of API call
                DateTime now = DateTime.Now;
                int year = now.Year;
                int month = now.Month;
                int day = now.Day;
                int hour = now.Hour;
                int minute = now.Minute;
                int second = now.Second;
                string monthString = now.ToString("MM");
                string dayString = now.ToString("dd");
                string timeString = now.ToString("HH:mm:ss");

                // Convert to proper date time format
                string datetime = $"{year}-{monthString}-{dayString} {timeString}";

                // Send GET request
                HttpResponseMessage response = await client.GetAsync("https://catfact.ninja/fact");

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var fact = JsonSerializer.Deserialize<CatFact>(content, options);

                // Add the fact to the facts list
                facts.Add(new CatFact { Fact = fact.Fact, Length = fact.Length, DateTime = datetime });
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }
        }

        return facts;
    }
    
    static async Task Main()
    {
        // Set login info
        string connectionString = "Server=laria-net.com;User ID=flaria_doctumstestusr;Password=Oy.udN8]t*iC;Database=flaria_doctumstest;";

        // Create connection
        var connection = new MySqlConnection(connectionString);

        // Get list of 5 cat facts
        List<CatFact> catFacts = await GetCatFacts();

        // Try to connect to database
        try
        {
            connection.Open();
            Console.WriteLine("Connection opened successfully.");

            // Query to insert into CatFact table
            string query = "INSERT INTO CatFacts (Fact, FactLength, Timestamp) VALUES (@Fact, @FactLength, @Timestamp)";

            // Insert each fact into the CatFact table
            foreach (var fact in catFacts)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Fact", fact.Fact);
                cmd.Parameters.AddWithValue("@FactLength", fact.Length);
                cmd.Parameters.AddWithValue("@Timestamp", fact.DateTime);
                int rowCount = cmd.ExecuteNonQuery();
                if (rowCount > 0)
                {
                    Console.WriteLine("Insertion successful");
                }
                else
                {
                    Console.WriteLine("Insertion unsuccessful");
                }
            }

        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Error connecting to MySQL: {ex.Message}");
        }
        finally
        {
            connection.Close();
            Console.WriteLine("Connection closed.");
        }
    }
}