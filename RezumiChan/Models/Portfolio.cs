namespace RezumiChan.Models
{
    public class Portfolio
    {
        public List<PortfolioProject> Projects { get; set; }
    }

    public class PortfolioProject
    {
        public string Title { get; set; }
        public string Context { get; set; }
        public string Role { get; set; }
        public string Company { get; set; } // Optional, if not all projects have a company
        public string Description { get; set; }
        public List<string> Technologies { get; set; } // Technologies used in the project
    }
}