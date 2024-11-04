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

class Program
{
    //model options: "gpt-3.5-turbo", "gpt-3.5-turbo-16k", "gpt-4", "gpt-4-32k"
    const string modelToUse = "gpt-4";
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

        //var greySkiesContext = GetContextSummary("Grey Skies", resume, stories, portfolio);
        //var waveContext = GetContextSummary("Wave", resume, stories, portfolio);
        //var tcContext = GetContextSummary("Tender Claws", resume, stories, portfolio);
        //var battleBotsContext = GetContextSummary("BattleBots", resume, stories, portfolio);

        filePath = "Data/tenderclaws_bank.json";
        var tcBankJson = File.ReadAllText(filePath);
        BulletpointBank tcBank = JsonConvert.DeserializeObject<BulletpointBank>(tcBankJson);
        
        filePath = "Data/wave_bank.json";
        var waveBankJson = File.ReadAllText(filePath);
        BulletpointBank waveBank = JsonConvert.DeserializeObject<BulletpointBank>(waveBankJson);
        
        filePath = "Data/bloodsport_bank.json";
        var bloodsportBankJson = File.ReadAllText(filePath);
        BulletpointBank bloodsportBank = JsonConvert.DeserializeObject<BulletpointBank>(bloodsportBankJson);
        
        filePath = "Data/greyskies_bank.json";
        var greyskiesBankJson = File.ReadAllText(filePath);
        BulletpointBank greyskiesBank = JsonConvert.DeserializeObject<BulletpointBank>(greyskiesBankJson);

        
        List<string> tcSelectedBulletPoints = new List<string>();
        List<string> waveSelectedBulletPoints= new List<string>();
        List<string> bloodsportSelectedBulletPoints= new List<string>();
        List<string> greyskiesSelectedBulletPoints= new List<string>();
        List<Skill> skills= new List<Skill>();

        string filename = "riko_balakit_resume_";

        if (aiEnabled)
        {
            tcSelectedBulletPoints = await GetBulletPointsFromBank(tcBank, job, 4, false);
            waveSelectedBulletPoints = await GetBulletPointsFromBank(waveBank, job, 4, false);
            bloodsportSelectedBulletPoints = await GetBulletPointsFromBank(bloodsportBank, job, 3, false);
            greyskiesSelectedBulletPoints = await GetBulletPointsFromBank(greyskiesBank, job, 3, false);
            skills = await GetRelevantSkills(resume, job);
            filename += await GetJobName(job);
        }
        else
        {
            tcSelectedBulletPoints.Add("Lead designer and engineer for VR game projects at Tender Claws, contributing to successful releases");
            tcSelectedBulletPoints.Add("Developed and refined gameplay mechanics, solved urgent issues, and ensured smooth operation of games. ");
            tcSelectedBulletPoints.Add("Owned and managed entire chapters of immersive VR experiences, delivering on time and without major issues. ");
            tcSelectedBulletPoints.Add("Designed and implemented reusable game engines, showcasing adaptability and innovation in game development. ");
            waveSelectedBulletPoints.Add("Led and managed the Resident DJ Program at Wave, increasing user engagement during downtime between headline concerts.");
            waveSelectedBulletPoints.Add("Spearheaded the development of advanced DJ Deck feature at Wave, balancing management's desire for simplicity with hobbyist DJs' needs");
            waveSelectedBulletPoints.Add("Advocated for social safety features at Wave, setting new standards for immersive social VR experiences.");
            waveSelectedBulletPoints.Add("Successfully advocated for user-requested fixes and features at Wave, leading to increased user satisfaction and engagement");
            bloodsportSelectedBulletPoints.Add("Led the development of a cutting-edge telemetry system for BattleBots contender, Bloodsport, ensuring real-time monitoring and broadcast of robot health.");
            bloodsportSelectedBulletPoints.Add("Utilized KiCAD to custom-design electronics for overseas manufacturing and developed a live monitoring interface in Unity for cinematic display of data.");
            bloodsportSelectedBulletPoints.Add("Successfully met high entertainment standards of BattleBots while providing crucial performance insights for live audiences and post-match analysis.");
            bloodsportSelectedBulletPoints.Add("Demonstrated expertise in embedded systems, Unity, KiCAD, and C++, delivering a visually striking telemetry UI that enhanced entertainment value.");
            greyskiesSelectedBulletPoints.Add("Designed innovative combat robots like Data Collector and Two Factor Annihilation, showcasing technical prowess and competitive edge.");
            greyskiesSelectedBulletPoints.Add("Utilized advanced electronics and firmware to create semi-autonomous controls for combat robots, demonstrating viability in a competitive environment.");
            greyskiesSelectedBulletPoints.Add("Integrated Steam Deck as primary controller for combat robot, enabling live telemetry viewing, real-time logging, and on-the-fly settings adjustments, impressing robot combat community.");
            greyskiesSelectedBulletPoints.Add("Designed and sold breakout board for Arduino Nano to simplify ESC reprogramming process, generating revenue and increasing efficiency for robot builders.");
            var sdSkill = new Skill("Software Development", new[] { "Unreal", "Unity", "Virtual Reality", "Augmented Reality", "Netcode", "Databases" });
            var programmingSkill = new Skill("Programming Languages", new[] { "C#", "C++", "C", "JavaScript", "Python" });
            var designSkill = new Skill("Design Tools", new[] { "Photoshop" });
            var prototypingSkill = new Skill("Prototyping and Manufacturing", new[] { "3D printing", "PCB Design", "CNC Machining" });
            var embeddedSystemsSkill = new Skill("Embedded Systems", new[] { "Microcontrollers", "Wireless Communication" });
            var collaborationSkill = new Skill("Collaboration Tools", new[] { "Perforce", "Git", "Slack", "JIRA", "Asana", "Trello", "Confluence", "Google Office" });
            skills.Add(sdSkill);
            skills.Add(programmingSkill);
            skills.Add(designSkill);
            skills.Add(prototypingSkill);
            skills.Add(embeddedSystemsSkill);
            skills.Add(collaborationSkill);
            filename += "test";
        }

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

                AddSkillsSection(document, skills);
                AddDivider(document, "Work Experience");
                AddWorkSection(document, "Tender Claws", "Game Developer", "Los Angeles, CA (Remote)",
                    "February 2021 - August 2024", tcSelectedBulletPoints);
                AddWorkSection(document, "Wave (Formerly TheWaveVR)", "Engineer", "Austin, TX",
                    "February 2017 - November 2020", waveSelectedBulletPoints);
                AddDivider(document, "Projects");
                AddProjectSection(document, "BattleBots - Bloodsport", bloodsportSelectedBulletPoints);
                AddProjectSection(document, "Grey Skies Automation", greyskiesSelectedBulletPoints);


                AddEducationSection(document, resume);
            }
        }

        Console.WriteLine($"PDF created at: {Path.GetFullPath(pdfPath)}");
        OpenPdf(pdfPath);
    }

    public static string GenerateTimestamp()
    {
        // Get the current local time
        DateTime now = DateTime.Now;

        // Format the timestamp as yyyyMMdd_HHmmss
        string formattedTimestamp = now.ToString("yyyyMMdd_HHmmss");

        return formattedTimestamp;
    }

    private static void AddSkillsSection(Document document, List<Skill> skills)
    {
        AddDivider(document, "Skills");

        const int fontSize = 9;
        Paragraph skillsParagraph = new Paragraph().SetFontSize(fontSize).SetMarginLeft(leftMargin);

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

    private static void AddProjectSection(Document document, string projectName, List<string> bulletPoints)
    {
        const int fontSize = 10;
        Paragraph entry = new Paragraph().SetFontSize(fontSize).SetMarginLeft(leftMargin);
        ;
        entry.Add(new Text($"{projectName}\n").SetBold());

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

    public static async Task<string> GetJobName(JobPost job)
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
                            $"Give me a filename-friendly name of this job including the company name (if known, if not known or looks like an anonymous recruiter, then put anonymous for the company name) and the role (only one word including underscores. do not give me any other text/paragraphs):\n{job.Rawtext}"
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

    public static async Task<List<string>> GetBulletPointsFromBank(BulletpointBank bank, JobPost job, int number,
        bool current = false)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var bulletpoint in bank.Bulletpoints)
        {
            sb.AppendLine($"{bulletpoint.ID}) {bulletpoint.BulletpointText}");
            sb.AppendLine();
        }

        //Console.WriteLine(sb.ToString().Trim());
        
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
                        content = $"Here is the job posting: {job.Rawtext}. Here is a bank of bullet points from a job or project I have done. Return the top {number} most relevant to that job description bullet points in JSON format, using only the bullet point numbers. Do not include any addictional text, explainations, or preambles- only return the JSON format. Desired output format: "+@"{""relevant_bullet_points"":[]}"
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

                    Console.WriteLine($"bulletPointsJson: {bulletPointsJson}");
                    
                    try
                    {
                        var deserializedResponse = JsonConvert.DeserializeObject<RelevantBulletPointsResponse>(bulletPointsJson);
                        var listOfPointNumbers = deserializedResponse.RelevantBulletPoints;

                        if (listOfPointNumbers.Count != number)
                        {
                            // throw an error to force a retry
                            throw new InvalidOperationException("The count of listOfPointNumbers does not match the expected number.");
                        }

                        return bank.GetBulletpointTextsByIds(listOfPointNumbers);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Deserialization error: {ex.Message}. Attempt {currentAttempt} of {maxRetries}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Error calling OpenAI API: {response.ReasonPhrase}. Attempt {currentAttempt} of {maxRetries}.");
                }

                // Optionally, introduce a delay before retrying
                await Task.Delay(1000); // Wait 1 second before the next attempt
            }

            throw new Exception($"Failed to get bullet points after {maxRetries} attempts.");
        }
    }
    


    public static async Task<List<string>> GetBulletPoints(ContextSummary context, JobPost job, int number, bool current = false)
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
                model = modelToUse, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $"Here is the job posting: {job.Rawtext}"
                    },
                    new
                    {
                        role = "user",
                        content = $"Here is my experience at {context.ContextName}: {context.ContextTotalText}. - Do not mix or confuse the job posting's requirements with my job experience. - Focus only on the experience and skills I provide in this message when generating bullet points.{number} short bullet points about my experience at {context} that is relevant to the job posting. without using any first-person pronouns like 'I' or 'we'. These bullet points should be appropriate for a resume. Remove in-text quotation marks, this throws off the program. Try to keep each bullet point either between 100-120 characters or 220-240 characters. Bullet points should end in punctuation like a period. {tenseString}. Highlight achievements in particular - sell the job on me. Return them as a simple JSON string array format without extra structure. - For reference, do not reference any experience requirements from the job posting when creating bullet points. Focus solely on my job description."
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
                        Console.WriteLine($"Deserialization error: {ex.Message}. Attempt {currentAttempt} of {maxRetries}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Error calling OpenAI API: {response.ReasonPhrase}. Attempt {currentAttempt} of {maxRetries}.");
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
                model = modelToUse, // or "gpt-4" if you have access
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content =
                            $"Here is a job posting: {job.Rawtext}. Here is my skills list in json format: {skillsJson}. Return back the same json but with skills both rearranged. Do not cull skills, rearrange them by relevance. Remove in-text quotation marks, this throws off the program. Do not add any extra text/reasoning/explanation, the output is being sent directly to a json parser."
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
                        return JsonConvert.DeserializeObject<List<Skill>>(skillsJsonResponse);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Deserialization error: {ex.Message}. Attempt {currentAttempt} of {maxRetries}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Error calling OpenAI API: {response.ReasonPhrase}. Attempt {currentAttempt} of {maxRetries}.");
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
        string skillsJson = JsonConvert.SerializeObject(resume.Skills, Formatting.Indented);
        return skillsJson;
    }
}