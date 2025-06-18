using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Net;
using CsvHelper;
using System.Text.Json;

public class CatFact
{
    public string? Fact { get; set; }
    public int Length { get; set; }
}

public class Program
{
    static async Task Main()
    {
        // Create a single HttpClient instance to reuse
        using HttpClient client = new HttpClient();

        // Create a list to store data
        List<CatFact> facts = new List<CatFact>();

        // Send a GET request 20 times
        for (int i = 0; i < 20; i++)
        {
            try
            {
                // Send GET request
                HttpResponseMessage response = await client.GetAsync("https://catfact.ninja/fact");

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var fact = JsonSerializer.Deserialize<CatFact>(content, options);

                // Add the fact to the facts list
                facts.Add(new CatFact { Fact = fact.Fact, Length = fact.Length });
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }
        }

        // Get date for the CSV file name
        DateTime now = DateTime.Now;
        int year = now.Year;
        int month = now.Month;
        int day = now.Day;
        int hour = now.Hour;
        int minute = now.Minute;
        int second = now.Second;
        string monthString = now.ToString("MM");
        string dayString = now.ToString("dd");
        string timeString = now.ToString("HHmmss");

        string filename = $"{year}-{monthString}-{dayString}-{timeString}-catfacts.csv";

        // Write data to CSV
        using (var stream = File.OpenWrite(filename))
        using (var writer = new StreamWriter(stream))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(facts);
        }

        // POSTing the CSV file to the FTP server

        string ftpHost = "ftp://laria-net.com";
        string ftpUsername = "doctums.dev.tests@laria-net.com";
        string ftpPassword = "pN7[Fv2oIoTa";

        string uploadUrl = $"{ftpHost}/{Path.GetFileName(filename)}";

        // Reading file
        byte[] fileContents = await File.ReadAllBytesAsync(filename);

        // Make FTP request
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uploadUrl);
        request.Method = WebRequestMethods.Ftp.UploadFile;

        // Set credentials
        request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

        // Write to request stream
        using (Stream requestStream = await request.GetRequestStreamAsync())
        {
            await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
        }

        // Get response
        using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
        {
            Console.WriteLine($"Upload complete: {response.StatusDescription}");
        }

    }
    
}
