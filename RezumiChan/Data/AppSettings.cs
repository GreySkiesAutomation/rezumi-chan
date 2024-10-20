namespace RezumiChan.Data;

public class AppSettings
{
    public OpenAISettings OpenAI { get; set; }
}

public class OpenAISettings
{
    public string ApiKey { get; set; }
}