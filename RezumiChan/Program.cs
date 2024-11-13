using Newtonsoft.Json;
using System.Diagnostics;
using RezumiChan.Models;
using System.Net.Http.Headers;
using System.Text;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using RezumiChan.Data;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Pdf.Canvas.Parser;

class Program
{
    //model options: "gpt-3.5-turbo", "gpt-3.5-turbo-16k", "gpt-4", "gpt-4-32k"
    const string modelToUseIntense = "gpt-4";
    const string modelToUseCheap = "gpt-4";
    private const int leftMargin = 10;
    private const bool aiEnabled = true; // used so I can generate PDFs without using up tokens.

    static async Task Main(string[] args)
    {
        string filePath = "Data/resume.json"; // Adjust the path if needed

        Resume resume = LoadResume(filePath);

        if (resume != null)
        {
            Console.WriteLine("Resume loaded successfully");
        }
        else
        {
            Console.WriteLine("Failed to load resume.");
        }

        filePath = "Data/portfolio.json"; // Adjust the path if needed

        Portfolio portfolio = LoadPortfolio(filePath);
        if (portfolio != null)
        {
            Console.WriteLine("Porfolio loaded successfully");
        }
        else
        {
            Console.WriteLine("Failed to load portfolio.");
        }

        filePath = "Data/stories.json"; // Adjust the path if needed

        Stories stories = LoadStories(filePath);
        if (stories != null)
        {
            Console.WriteLine("Stories loaded successfully");
        }
        else
        {
            Console.WriteLine("Failed to load portfolio.");
        }

        filePath = "Data/job.txt";

        JobPost job = LoadJobPost(filePath);
        
        
        Console.WriteLine($"Determining job requirements...");
        job.Summary = await GetJobSummary(job);
        Console.WriteLine("Summary: " + job.Summary);
        Console.WriteLine("Determining name of the job from the posting...");
        job.JobNameAndTitle = await GetJobName(job);
        Console.WriteLine("Job Name: " + job.JobNameAndTitle);

        //var greySkiesContext = GetContextSummary("Grey Skies", resume, stories, portfolio);
        //var waveContext = GetContextSummary("Wave", resume, stories, portfolio);
        //var tcContext = GetContextSummary("Tender Claws", resume, stories, portfolio);
        //var battleBotsContext = GetContextSummary("BattleBots", resume, stories, portfolio);

        filePath = "Data/Bulletpoints/tenderclaws_bank.json";
        var tcBankJson = File.ReadAllText(filePath);
        BulletpointBank tcBank = JsonConvert.DeserializeObject<BulletpointBank>(tcBankJson);

        filePath = "Data/Bulletpoints/wave_bank.json";
        var waveBankJson = File.ReadAllText(filePath);
        BulletpointBank waveBank = JsonConvert.DeserializeObject<BulletpointBank>(waveBankJson);

        filePath = "Data/Bulletpoints/bloodsport_bank.json";
        var bloodsportBankJson = File.ReadAllText(filePath);
        BulletpointBank bloodsportBank = JsonConvert.DeserializeObject<BulletpointBank>(bloodsportBankJson);

        filePath = "Data/Bulletpoints/greyskies_bank.json";
        var greyskiesBankJson = File.ReadAllText(filePath);
        BulletpointBank greyskiesBank = JsonConvert.DeserializeObject<BulletpointBank>(greyskiesBankJson);

        Console.WriteLine("Bulletpoint banks loaded");

        List<string> tcSelectedBulletPoints = new List<string>();
        List<string> waveSelectedBulletPoints = new List<string>();
        List<string> bloodsportSelectedBulletPoints = new List<string>();
        List<string> greyskiesSelectedBulletPoints = new List<string>();
        List<Skill> skills = new List<Skill>();

        string filename = "riko_balakit_resume_";

        Console.WriteLine("Calculating most relevant bullet points from Tender Claws...");
        tcSelectedBulletPoints = await GetBulletPointsFromBank(tcBank, job, 4, false);
        Console.WriteLine("Calculating most relevant bullet points from Wave...");
        waveSelectedBulletPoints = await GetBulletPointsFromBank(waveBank, job, 4, false);
        Console.WriteLine("Calculating most relevant bullet points from Bloodsport...");
        bloodsportSelectedBulletPoints = await GetBulletPointsFromBank(bloodsportBank, job, 3, false);
        Console.WriteLine("Calculating most relevant bullet points from Grey Skies Automation...");
        greyskiesSelectedBulletPoints = await GetBulletPointsFromBank(greyskiesBank, job, 3, false);
        Console.WriteLine("Calculating most relevant skill order...");
        skills = await GetRelevantSkills(resume, job);
        filename += job.JobNameAndTitle;
        Console.WriteLine("Building the resume PDF");


        filename += $"_{GenerateTimestamp()}.pdf";


        string pdfPath = filename;

        using (PdfWriter writer = new PdfWriter(pdfPath))
        {
            using (PdfDocument pdf = new PdfDocument(writer))
            {
                Document document = new Document(pdf);
                PdfFont helveticaFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                document.SetFont(helveticaFont);

                AddHeader(document, "Riko Balakit", "riko@balak.it", "(210) 508-8774", "Austin, TX");

                AddSkillsSection(document, skills, resume.Skills);
                AddDivider(document, "Work Experience");
                AddWorkSection(document, "Tender Claws", "Game Developer", "Los Angeles, CA (Remote)",
                    "February 2021 - August 2024", tcSelectedBulletPoints);
                AddWorkSection(document, "Wave (Formerly TheWaveVR)", "Engineer", "Austin, TX",
                    "February 2017 - November 2020", waveSelectedBulletPoints);
                AddDivider(document, "Projects");
                AddProjectSection(document, "BattleBots - Bloodsport", "Telemetry Specialist",
                    bloodsportSelectedBulletPoints);
                AddProjectSection(document, "Grey Skies Automation", "Founder, Owner, Operator",
                    greyskiesSelectedBulletPoints);


                AddEducationSection(document, resume);
            }
        }

        OpenPdf(pdfPath);

        //var resumeText = ReadPdf(pdfPath);
        //await EvaluateResume(job, resumeText);
    }

    public static string GenerateTimestamp()
    {
        // Get the current local time
        DateTime now = DateTime.Now;

        // Format the timestamp as yyyyMMdd_HHmmss
        string formattedTimestamp = now.ToString("yyyyMMdd_HHmmss");

        return formattedTimestamp;
    }

    public static async Task<string> GetJobSummary(JobPost job)
    {
        var apiKey = LoadApiKey();
        var endpoint = "https://api.openai.com/v1/chat/completions";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUseIntense, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content =
                            $"Please provide a concise summary of the following job posting: {job.Rawtext}. Include the name of the company and the role/title. Summarize the following job posting, ensuring the summary includes every skill, skillset, requirement, and keyword mentioned. Also include what the job is about and the things you do on the job, and the products/services the company offers. Focus exclusively on job responsibilities, essential skills, qualifications, and experience requirements necessary for the role. Omit any information about benefits, compensation, equal opportunity statements, or other non-essential details. The summary should be clear, concise, and suitable for crafting a tailored resume or ATS optimization."
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            int maxRetries = 5; // Maximum number of retries
            int currentAttempt = 0;

            while (currentAttempt < maxRetries)
            {
                currentAttempt++;

                var response = await client.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string jobSummary = result.choices[0].message.content.ToString();

                    return jobSummary;
                }
                else
                {
                    Console.WriteLine(
                        $"Error calling OpenAI API: {response.ReasonPhrase}. Attempt {currentAttempt} of {maxRetries}.");
                }

                // Optionally, introduce a delay before retrying
                await Task.Delay(1000); // Wait 1 second before the next attempt
            }

            throw new Exception($"Failed to get job summary after {maxRetries} attempts.");
        }
    }


    private static void AddSkillsSection(Document document, List<Skill> skills, List<Skill> resumeSkills)
    {
        AddDivider(document, "Skills");

        const int fontSize = 9;
        Paragraph skillsParagraph = new Paragraph().SetFontSize(fontSize).SetMarginLeft(leftMargin);

        foreach (var skill in skills)
        {
            string skillLine = $" ";

            foreach (var skillset in resumeSkills)
            {
                if (skillset.Category == skill.Category)
                {
                    foreach (var singleSkill in skillset.TopSkills)
                    {
                        skillLine += $"{singleSkill}, ";
                    }
                }
            }

            foreach (var singleSkill in skill.Skills)
            {
                skillLine += $"{singleSkill}, ";
            }

            if (skillLine.Length == 1)
            {
                continue;
            }

            skillLine = skillLine.Substring(0, skillLine.Length - 2);
            skillLine += ".\n";

            skillsParagraph.Add(new Text($"{skill.Category}:").SetUnderline()); // Add name
            skillsParagraph.Add(skillLine);
        }

        document.Add(skillsParagraph);
    }

    private static void AddWorkSection(Document document, string companyName, string title, string location,
        string duration, List<string> bulletPoints)
    {
        const int fontSize = 10;
        Paragraph entry = new Paragraph().SetFontSize(fontSize).SetMarginLeft(leftMargin);
        ;
        entry.Add(new Text($"{companyName}").SetBold());
        entry.Add(new Text($", {location} - "));
        entry.Add(new Text($"{title}").SetUnderline());
        entry.Add(new Text($", {duration}\n"));
        const int bulletPointSize = 9;
        foreach (var point in bulletPoints)
        {
            entry.Add(new Text($" - {point}\n").SetFontSize(bulletPointSize));
        }

        document.Add(entry);
    }

    private static void AddProjectSection(Document document, string projectName, string projectTitle,
        List<string> bulletPoints)
    {
        const int fontSize = 10;
        Paragraph entry = new Paragraph().SetFontSize(fontSize).SetMarginLeft(leftMargin);
        ;
        entry.Add(new Text($"{projectName} - ").SetBold());
        entry.Add(new Text($"{projectTitle}\n").SetUnderline());

        const int bulletPointSize = 9;
        foreach (var point in bulletPoints)
        {
            entry.Add(new Text($" - {point}\n").SetFontSize(bulletPointSize));
        }

        document.Add(entry);
    }

    private static void AddEducationSection(Document document, Resume resume)
    {
        AddDivider(document, "Education");
        const int fontSize = 9;
        Paragraph educationEntry = new Paragraph().SetFontSize(fontSize).SetMarginLeft(leftMargin);
        ;

        for (int i = 0; i < resume.Education.Count; i++)
        {
            educationEntry.Add(new Text($"{resume.Education[i].Institution}").SetFontSize(fontSize).SetBold());
            educationEntry.Add(new Text($", {resume.Education[i].SubInstitution} - ").SetFontSize(fontSize));
            educationEntry.Add(new Text($"{resume.Education[i].Degree}").SetFontSize(fontSize).SetItalic());
            educationEntry.Add(new Text($", {resume.Education[i].Years}").SetFontSize(fontSize));

            if (i < (resume.Education.Count - 1))
            {
                educationEntry.Add(new Text($"\n").SetFontSize(fontSize));
            }
        }

        document.Add(educationEntry);
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
        Paragraph header = new Paragraph();
        header.Add(new Text(name + "\n")
            .SetFontSize(16) // Set font size for header
            .SetBold()); // Make the header bold
        header.Add(new Text(email + " | " + phone + " | " + city).SetFontSize(10));

        // Add the header to the document
        document.Add(header);
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
                    $"Job: {experience.Title} at {experience.Company} ({experience.Duration})\n Description: {string.Join(" ", experience.Description)}";
            }
        }

        foreach (var project in resume.Projects)
        {
            if (project.Context == contextName)
            {
                totalString += $"Project: {project.Name}. Description: ({string.Join(" ", project.Description)})\n";
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


        string filePath = Path.Combine(Environment.CurrentDirectory, $"{contextName}.txt");

        // Save the string to "beans.txt"
        File.WriteAllText(filePath, totalString);

        // Print the path to the console
        Console.WriteLine("File saved at: " + filePath);

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
                model = modelToUseIntense, // or "gpt-4" if you have access
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

    public static async Task<string> GetJobName(JobPost job)
    {
        var apiKey = LoadApiKey();
        var endpoint = "https://api.openai.com/v1/chat/completions";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUseCheap, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content =
                            $"Give me a filename-friendly name of this job including the company name (if known, if not known or looks like an anonymous recruiter, then put anonymous for the company name) and the role (only one word including underscores. do not give me any other text/paragraphs):\n{job.Summary}"
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

    public static async Task<string> StartFreshWithOpenAI(string newContext)
    {
        var apiKey = LoadApiKey();
        var endpoint = "https://api.openai.com/v1/chat/completions";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUseIntense, // or "gpt-4" if you have access
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

    public static async Task<List<string>> GetBulletPointsFromBank(BulletpointBank bank, JobPost job, int totalNumber,
        bool current = false)
    {
        var number = totalNumber;
        if (!aiEnabled)
        {
            var examplePoints = new List<string>();
            int pointCounter = 0;
            foreach (var point in bank.RequiredBulletpoints)
            {
                if (pointCounter != number)
                {
                    examplePoints.Add(point.BulletpointText);
                    pointCounter++;
                }
            }

            foreach (var point in bank.Bulletpoints)
            {
                if (pointCounter != number)
                {
                    examplePoints.Add(point.BulletpointText);
                    pointCounter++;
                }
            }

            return examplePoints;
        }

        number = totalNumber - bank.RequiredBulletpoints.Count;

        StringBuilder sb = new StringBuilder();

        foreach (var bulletpoint in bank.Bulletpoints)
        {
            sb.AppendLine($"{bulletpoint.ID}) {bulletpoint.BulletpointText}");
            sb.AppendLine();
        }

        //Console.WriteLine(sb.ToString().Trim());

        var apiKey = LoadApiKey();
        var endpoint = "https://api.openai.com/v1/chat/completions";
        string contentText =
            $"Here is the job posting: {job.Summary}. Here is a bank of bullet points from a job or project I have done: {sb.ToString().Trim()}. Return the top {number} most relevant to that job description bullet points in JSON format, using only the bullet point numbers. Do not include any addictional text, explainations, or preambles- only return the JSON format. Desired output format: " +
            @"{""relevant_bullet_points"":[]}";

        //Console.WriteLine(contentText);
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUseIntense, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = contentText
                    },
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            int maxRetries = 5; // Maximum number of retries
            int currentAttempt = 0;

            while (currentAttempt < maxRetries)
            {
                currentAttempt++;

                var response = await client.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string bulletPointsJson = result.choices[0].message.content.ToString();

                    //Console.WriteLine($"bulletPointsJson: {bulletPointsJson}");

                    try
                    {
                        var deserializedResponse =
                            JsonConvert.DeserializeObject<RelevantBulletPointsResponse>(bulletPointsJson);
                        var listOfPointNumbers = deserializedResponse.RelevantBulletPoints;

                        if (listOfPointNumbers.Count != number)
                        {
                            // throw an error to force a retry
                            throw new InvalidOperationException(
                                "The count of listOfPointNumbers does not match the expected number.");
                        }

                        if (!bank.ValidateBulletpointNumbers(listOfPointNumbers))
                        {
                            // throw an error to force a retry
                            throw new InvalidOperationException(
                                "the list of numbers includes nonexistent bulletpoints, bro wtf");
                        }

                        var returnPoints = new List<string>();
                        int pointCounter = 0;
                        foreach (var point in bank.RequiredBulletpoints)
                        {
                            if (pointCounter != number)
                            {
                                returnPoints.Add(point.BulletpointText);
                                pointCounter++;
                            }
                        }

                        returnPoints.AddRange(bank.GetBulletpointTextsByIds(listOfPointNumbers));

                        return returnPoints;
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine(
                            $"Deserialization error: {ex.Message}. Attempt {currentAttempt} of {maxRetries}.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"Validation error: {ex.Message}. Attempt {currentAttempt} of {maxRetries}.");
                    }
                }
                else
                {
                    Console.WriteLine(
                        $"Error calling OpenAI API: {response.ReasonPhrase}. Attempt {currentAttempt} of {maxRetries}.");
                }

                // Optionally, introduce a delay before retrying
                await Task.Delay(1000); // Wait 1 second before the next attempt
            }

            throw new Exception($"Failed to get bullet points after {maxRetries} attempts.");
        }
    }


    public static async Task<List<string>> GetBulletPoints(ContextSummary context, JobPost job, int number,
        bool current = false)
    {
        var apiKey = LoadApiKey();
        var endpoint = "https://api.openai.com/v1/chat/completions";

        string tenseString = "make everything past tense, such as wrote or developed or designed.";
        if (current)
        {
            tenseString = "make everything current tense, such as leading or developing or demonstrating or designed.";
        }

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUseIntense, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $"Here is the job posting: {job.Summary}"
                    },
                    new
                    {
                        role = "user",
                        content =
                            $"Here is my experience at {context.ContextName}: {context.ContextTotalText}. - Do not mix or confuse the job posting's requirements with my job experience. - Focus only on the experience and skills I provide in this message when generating bullet points.{number} short bullet points about my experience at {context} that is relevant to the job posting. without using any first-person pronouns like 'I' or 'we'. These bullet points should be appropriate for a resume. Remove in-text quotation marks, this throws off the program. Try to keep each bullet point either between 100-120 characters or 220-240 characters. Bullet points should end in punctuation like a period. {tenseString}. Highlight achievements in particular - sell the job on me. Return them as a simple JSON string array format without extra structure. - For reference, do not reference any experience requirements from the job posting when creating bullet points. Focus solely on my job description."
                    },
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            int maxRetries = 5; // Maximum number of retries
            int currentAttempt = 0;

            while (currentAttempt < maxRetries)
            {
                currentAttempt++;

                var response = await client.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string bulletPointsJson = result.choices[0].message.content.ToString();

                    try
                    {
                        return JsonConvert.DeserializeObject<List<string>>(bulletPointsJson);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine(
                            $"Deserialization error: {ex.Message}. Attempt {currentAttempt} of {maxRetries}.");
                    }
                }
                else
                {
                    Console.WriteLine(
                        $"Error calling OpenAI API: {response.ReasonPhrase}. Attempt {currentAttempt} of {maxRetries}.");
                }

                // Optionally, introduce a delay before retrying
                await Task.Delay(1000); // Wait 1 second before the next attempt
            }

            throw new Exception($"Failed to get bullet points after {maxRetries} attempts.");
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
                model = modelToUseIntense, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content =
                            $"Here is a job posting: {job.Summary}. Here is my skills list in json format: {skillsJson}. Return back the same json but with skills both rearranged. Do not cull any items in the skill list, rearrange them by relevance. Remove in-text quotation marks, this throws off the program. Do not add any extra text/reasoning/explanation, the output is being sent directly to a json parser."
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            int maxRetries = 5; // Maximum number of retries
            int currentAttempt = 0;

            while (currentAttempt < maxRetries)
            {
                currentAttempt++;

                var response = await client.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string skillsJsonResponse = result.choices[0].message.content.ToString();

                    try
                    {
                        var newSkills = JsonConvert.DeserializeObject<List<Skill>>(skillsJsonResponse);

                        if (newSkills.Count != resume.Skills.Count)
                        {
                            throw new InvalidOperationException(
                                "The number of deserialized skills does not match the expected count.");
                        }

                        return newSkills;
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine(
                            $"Deserialization error: {ex.Message}. Attempt {currentAttempt} of {maxRetries}.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"Operation error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine(
                        $"Error calling OpenAI API: {response.ReasonPhrase}. Attempt {currentAttempt} of {maxRetries}.");
                }

                // Optionally, introduce a delay before retrying
                await Task.Delay(1000); // Wait 1 second before the next attempt
            }

            throw new Exception($"Failed to get relevant skills after {maxRetries} attempts.");
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
        var skillsWithoutTopSkills = new List<Skill>();
        foreach (var skillset in resume.Skills)
        {
            skillsWithoutTopSkills.Add(new Skill(skillset.Category, skillset.Skills.ToArray(), null));
        }

        string skillsJson = JsonConvert.SerializeObject(skillsWithoutTopSkills, Formatting.Indented);
        return skillsJson;
    }

    private static string ReadPdf(string pathToPdf)
    {
        StringBuilder resumeText = new StringBuilder();

        using (PdfReader pdfReader = new PdfReader(pathToPdf))
        using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
        {
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                // Extract text from each page
                resumeText.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
            }
        }

        return resumeText.ToString();
    }

    public static async Task<string> EvaluateResume(JobPost job, string resumeText)
    {
        var apiKey = LoadApiKey(); // Replace with a method that retrieves your OpenAI API key
        var endpoint = "https://api.openai.com/v1/chat/completions";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelToUseIntense, // Or another model if applicable
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are an assistant that evaluates resume and job description matches."
                    },
                    new
                    {
                        role = "user",
                        content = $"Evaluate how well this resume matches the job description on a scale of 1 to 10, " +
                                  $"where 10 is a perfect match. Provide a one-sentence response with the score. " +
                                  $"Job Description: {job.Summary}\nResume Text: {resumeText}"
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
                var resultText = result.choices[0].message.content.ToString();
                Console.WriteLine(resultText);
                return resultText;
            }
            else
            {
                throw new Exception($"Error calling OpenAI API: {response.ReasonPhrase}");
            }
        }
    }
}