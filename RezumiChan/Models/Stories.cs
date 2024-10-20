namespace RezumiChan.Models
{
    public class Stories
    {
        public List<Story> Starstories { get; set; }
    }

    public class Story
    {
        public string Title { get; set; }
        public string Context { get; set; } // Can be "Wave", "Tender Claws", "BattleBots", or "Personal"
        public string Situation { get; set; }
        public string Task { get; set; }
        public List<string> Action { get; set; }
        public string Result { get; set; }
        public List<string> Categories { get; set; }
    }
}