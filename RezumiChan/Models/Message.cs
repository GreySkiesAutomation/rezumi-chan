namespace RezumiChan.Models;

public class Message
{
    public string Role { get; set; } // "user" or "assistant"
    public string Content { get; set; }
}