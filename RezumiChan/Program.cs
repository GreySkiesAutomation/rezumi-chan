using Newtonsoft.Json;
using System;
using System.IO;
using RezumiChan.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        string filePath = "Data/resume.json"; // Adjust the path if needed
        Resume resume = LoadResume(filePath);

        if (resume != null)
        {
            // Combine the relevant information into a single string for the API
            string resumeText = $"{resume.Name}\n{resume.Summary}\nContact: {resume.Contact.Email}, {resume.Contact.Phone}\n";

            foreach (var experience in resume.Experience)
            {
                resumeText += $"Experience: {experience.Title} at {experience.Company} ({experience.Duration})\n";
            }

            // Call the OpenAI API to get a summary
            string summary = await GetSummaryFromOpenAI(resumeText);
            Console.WriteLine("Summary from OpenAI:");
            Console.WriteLine(summary);
        }
        else
        {
            Console.WriteLine("Failed to load resume.");
        }
    }

    static Resume LoadResume(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            Resume resume = JsonConvert.DeserializeObject<Resume>(json);
            return resume;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading resume: {ex.Message}");
            return null;
        }
    }
    
    public static async Task<string> GetSummaryFromOpenAI(string resumeText)
    {
        var apiKey = "YOUR_API_KEY"; // Replace with your actual API key
        var endpoint = "https://api.openai.com/v1/chat/completions";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-3.5-turbo", // or "gpt-4" if you have access
                messages = new[]
                {
                    new { role = "user", content = $"Please summarize the following resume:\n{resumeText}" }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                return result.choices[0].message.content.ToString();
            }
            else
            {
                throw new Exception($"Error calling OpenAI API: {response.ReasonPhrase}");
            }
        }
    }
}