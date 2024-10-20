using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using RezumiChan.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using RezumiChan.Data;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

class Program
{
    const string modelToUse = "gpt-4";

    static async Task Main(string[] args)
    {
        string filePath = "Data/resume.json"; // Adjust the path if needed
        List<Message> messageHistory = new List<Message>();

        Resume resume = LoadResume(filePath);

        if (resume != null)
        {
            /*
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
            */
        }
        else
        {
            Console.WriteLine("Failed to load resume.");
        }

        filePath = "Data/portfolio.json"; // Adjust the path if needed

        Portfolio portfolio = LoadPortfolio(filePath);
        if (portfolio != null)
        {
            /*
            string portfolioText = $"Portfolio:\n";

            foreach (var project in portfolio.Projects)
            {
                portfolioText += $"{project.Title}: {project.Role} at {project.Company} ({project.Context}). This is what I did: {project.Description}. I used the following skills to do it:";
                foreach (var tech in project.Technologies)
                {
                    portfolioText += $" {tech}, ";
                }
            }

            // Call the OpenAI API to get a summary
            string summary = await GetSummaryFromOpenAI(portfolioText);
            Console.WriteLine("Summary from OpenAI:");
            Console.WriteLine(summary);
            */
        }
        else
        {
            Console.WriteLine("Failed to load portfolio.");
        }

        filePath = "Data/stories.json"; // Adjust the path if needed

        Stories stories = LoadStories(filePath);
        if (stories != null)
        {
            /*
            string storiesText = $"Stories:\n";

            Console.WriteLine($"number of stories: {stories.Starstories.Count}");
            foreach (var story in stories.Starstories)
            {
                storiesText += $"{story.Title} ({story.Context}): Situation: {story.Situation}. Task: {story.Task}. Actions:";
                foreach (var action in story.Action)
                {
                    storiesText += $" {action}\n";
                }

                storiesText += $". Result: {story.Result}";
            }

            // Call the OpenAI API to get a summary
            string summary = await GetSummaryFromOpenAI(storiesText);
            Console.WriteLine("Summary from OpenAI:");
            Console.WriteLine(summary);
            */
        }
        else
        {
            Console.WriteLine("Failed to load portfolio.");
        }

        filePath = "Data/job.txt";

        JobPost job = LoadJobPost(filePath);

        var waveContext = GetContextSummary("Wave", resume, stories, portfolio);
        var tcContext = GetContextSummary("Tender Claws", resume, stories, portfolio);
        var battleBotsContext = GetContextSummary("BattleBots", resume, stories, portfolio);

        /*
        List<string> summary = await GetBulletPoints(waveContext, job);
        Console.WriteLine($"Wave bullet points ({summary.Count}): ");
        foreach (var line in summary)
        {
            Console.WriteLine($" - {line}");
        }

        summary = await GetBulletPoints(tcContext, job);
        Console.WriteLine($"Tender Claws bullet points ({summary.Count}): ");
        foreach (var line in summary)
        {
            Console.WriteLine($" - {line}");
        }

        summary = await GetBulletPoints(battleBotsContext, job);
        Console.WriteLine($"BattleBots bullet points ({summary.Count}): ");
        foreach (var line in summary)
        {
            Console.WriteLine($" - {line}");
        }
        */

        var skills = await GetRelevantSkills(resume, job);

        string pdfPath = "example.pdf";

        using (PdfWriter writer = new PdfWriter(pdfPath))
        {
            using (PdfDocument pdf = new PdfDocument(writer))
            {
                Document document = new Document(pdf);
                AddHeader(document, "Riko Balakit", "riko@balak.it", "(210) 508-8774", "Austin, TX");

                AddHeadline(document, "Destroying a better future (PLACEHOLDER).");
                AddSkillsSection(document, skills);
                AddWorkSection(document, resume, job, stories, portfolio);
                AddProjectsSection(document, resume, stories, portfolio);
                AddEducationSection(document, resume);
            }
        }

        Console.WriteLine($"PDF created at: {Path.GetFullPath(pdfPath)}");
        OpenPdf(pdfPath);
    }

    private static void AddSkillsSection(Document document, List<Skill> skills)
    {
        AddDivider(document, "Skills");
        
        const int fontSize = 10;
        foreach (var skill in skills)
        {
            string skillLine = $" ";
            foreach (var singleSkill in skill.Skills)
            {
                skillLine += $"{singleSkill}, ";
            }

            if (skillLine.Length == 1)
            {
                continue;
            }
            
            skillLine = skillLine.Substring(0, skillLine.Length - 2);
            skillLine += ".";
            
            Paragraph skillEntry = new Paragraph()
                .SetFontSize(fontSize) // Set font size for header
                .Add(new Text($"{skill.Category}:").SetUnderline()); // Add name
            skillEntry.Add(skillLine);

            document.Add(skillEntry);
        }
    }

    private static void AddWorkSection(Document document, Resume resume, JobPost job, Stories stories,
        Portfolio portfolio)
    {
        AddDivider(document, "Work Experience");
    }

    private static void AddProjectsSection(Document document, Resume resume, Stories stories, Portfolio portfolio)
    {
        AddDivider(document, "Projects");
    }

    private static void AddEducationSection(Document document, Resume resume)
    {
        AddDivider(document, "Education");
        const int fontSize = 10;
        foreach (var education in resume.Education)
        {
            // Create a table with 2 columns
            Table educationTable = new Table(2);
            educationTable.SetWidth(UnitValue.CreatePercentValue(100)); // Make the table full width

            // Remove borders from the table
            educationTable.SetBorder(Border.NO_BORDER);

            // Add institution and sub-institution (left-aligned)
            Cell institutionCell = new Cell()
                .Add(new Paragraph($"{education.Institution}, {education.SubInstitution}").SetFontSize(fontSize));
            institutionCell.SetBorder(Border.NO_BORDER); // Remove border from this cell
            educationTable.AddCell(institutionCell);

            Cell yearsCell = new Cell()
                .Add(new Paragraph($"{education.Years}").SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(fontSize)); // Right-aligned years
            yearsCell.SetBorder(Border.NO_BORDER); // Remove border from this cell
            educationTable.AddCell(yearsCell);

            // Add the degree (left-aligned)
            Cell degreeCell = new Cell()
                .Add(new Paragraph($"{education.Degree}").SetFontSize(fontSize));
            degreeCell.SetBorder(Border.NO_BORDER); // Remove border from this cell
            educationTable.AddCell(degreeCell);

            // Add an empty cell to balance the row
            educationTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)); // Remove border from this empty cell

            // Add the table to the document
            document.Add(educationTable);
            document.Add(new Paragraph()); // This creates a line break
        }
    }

    private static void AddDivider(Document document, string sectionName)
    {
        Paragraph header = new Paragraph()
            .SetFontSize(12) // Set font size for header
            .SetBold() // Make the header bold
            .Add(sectionName); // Add name

        document.Add(header);
    }

    static void OpenPdf(string pdfPath)
    {
        // Check if the file exists
        if (File.Exists(pdfPath))
        {
            // Start the default PDF viewer
            Process.Start(new ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true // This is required to open the file with the default application
            });

            Console.WriteLine($"PDF created at: {Path.GetFullPath(pdfPath)} and opened.");
        }
        else
        {
            Console.WriteLine("PDF file not found.");
        }
    }

    static void AddHeader(Document document, string name, string email, string phone, string city)
    {
        // Create the header paragraph
        Paragraph header = new Paragraph()
            .SetFontSize(14) // Set font size for header
            .SetBold() // Make the header bold
            .Add(name + "\n"); // Add name
        Paragraph header2 = new Paragraph()
            .SetFontSize(12) // Set font size for header
            .Add(email + " | " + phone + " | " + city); // Add contact information

        // Add the header to the document
        document.Add(header);
        document.Add(header2);
    }

    static void AddHeadline(Document document, string headline)
    {
        Paragraph header = new Paragraph()
            .SetFontSize(12) // Set font size for header
            .SetBold() // Make the header bold
            .Add(headline); // Add name

        document.Add(header);
    }

    static ContextSummary GetContextSummary(string contextName, Resume resume, Stories stories, Portfolio portfolio)
    {
        string totalString = "The following is the text about this job/role on the resume: \n";

        foreach (var experience in resume.Experience)
        {
            if (experience.Context == contextName)
            {
                totalString +=
                    $"Job: {experience.Title} at {experience.Company} ({experience.Duration})\n Description: {experience.Description}";
            }
        }

        foreach (var project in resume.Projects)
        {
            if (project.Context == contextName)
            {
                totalString += $"Project: {project.Name}. Description: ({project.Description})\n";
            }
        }

        totalString += "The following is some deeper details about the projects done at the mentioned role: \n";

        foreach (var project in portfolio.Projects)
        {
            if (project.Context == contextName)
            {
                totalString +=
                    $"{project.Title}: {project.Role} at {project.Company} ({project.Context}). This is what I did: {project.Description}. I used the following skills to do it:";
                foreach (var tech in project.Technologies)
                {
                    totalString += $" {tech}, ";
                }
            }
        }

        totalString += "The following are STAR formatted stories about decisive things I did at the role: \n";

        foreach (var story in stories.Starstories)
        {
            if (story.Context == contextName)
            {
                totalString +=
                    $"{story.Title} ({story.Context}): Situation: {story.Situation}. Task: {story.Task}. Actions:";
                foreach (var action in story.Action)
                {
                    totalString += $" {action}\n";
                }

                totalString += $". Result: {story.Result}";
            }
        }

        var newContextSummary = new ContextSummary();
        newContextSummary.ContextName = contextName;
        newContextSummary.ContextTotalText = totalString;
        return newContextSummary;
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

    static Portfolio LoadPortfolio(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            Portfolio portfolio = JsonConvert.DeserializeObject<Portfolio>(json);
            return portfolio;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading resume: {ex.Message}");
            return null;
        }
    }

    static Stories LoadStories(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            Stories stories = JsonConvert.DeserializeObject<Stories>(json);
            return stories;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading stories: {ex.Message}");
            return null;
        }
    }

    static JobPost LoadJobPost(string filePath)
    {
        try
        {
            JobPost jobPost = new JobPost();
            jobPost.Rawtext = File.ReadAllText(filePath);
            return jobPost;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading stories: {ex.Message}");
            return null;
        }
    }

    public static string LoadApiKey()
    {
        var json = File.ReadAllText("config.json");
        var settings = JsonConvert.DeserializeObject<AppSettings>(json);
        return settings.OpenAI.ApiKey;
    }

    public static async Task<string> GetSummaryFromOpenAI(string resumeText)
    {
        var apiKey = LoadApiKey();
        var endpoint = "https://api.openai.com/v1/chat/completions";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUse, // or "gpt-4" if you have access
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

    public static async Task<string> StartFreshWithOpenAI(string newContext)
    {
        var apiKey = LoadApiKey();
        var endpoint = "https://api.openai.com/v1/chat/completions";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUse, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = "Forget all previous contexts. I want to start a new discussion about:\n" + newContext
                    }
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

    public static async Task<List<string>> GetBulletPoints(ContextSummary context, JobPost job)
    {
        var apiKey = LoadApiKey();
        var endpoint = "https://api.openai.com/v1/chat/completions";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUse, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content =
                            $"Here is a job posting: {job.Rawtext}. Here is my experience at {context.ContextName} : {context.ContextTotalText}. 4 short bullet points about my experience at {context} that is relevant to the job posting. These bullet points should be appropriate for a resume. Return them as a simple json string array format without extra strcuture"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                return JsonConvert.DeserializeObject<List<string>>(result.choices[0].message.content.ToString());
            }
            else
            {
                throw new Exception($"Error calling OpenAI API: {response.ReasonPhrase}");
            }
        }
    }

    public static async Task<List<Skill>> GetRelevantSkills(Resume resume, JobPost job)
    {
        var apiKey = LoadApiKey();
        var endpoint = "https://api.openai.com/v1/chat/completions";

        var skillsJson = GetSkillsJson(resume);

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUse, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content =
                            $"Here is a job posting: {job.Rawtext}. Here is my skills list in json format: {skillsJson}. Return back the same json but with skills both rearranged and possibly culled of unrelated skills for the job posting. Do not add any extra text/reasoning/explaination, the output is being sent directly to a json parser"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                Console.WriteLine(result.choices[0].message.content.ToString());
                return JsonConvert.DeserializeObject<List<Skill>>(result.choices[0].message.content.ToString());
            }
            else
            {
                throw new Exception($"Error calling OpenAI API: {response.ReasonPhrase}");
            }
        }
    }

    public static string GetSkillsJson(Resume resume)
    {
        // Check if the resume object is not null
        if (resume == null || resume.Skills == null)
        {
            return "{}"; // Return empty JSON object if no skills are found
        }

        // Serialize the Skills property to JSON
        string skillsJson = JsonConvert.SerializeObject(resume.Skills, Formatting.Indented);
        return skillsJson;
    }
}