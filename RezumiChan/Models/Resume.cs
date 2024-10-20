namespace RezumiChan.Models;

public class Resume
{
    public string Name { get; set; }
    public string Summary { get; set; }
    public Contact Contact { get; set; }
    public List<Experience> Experience { get; set; }
    public List<Project> Projects { get; set; }
    public List<Skill> Skills { get; set; }
    public List<Education> Education { get; set; }
}

public class Contact
{
    public string Location { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }
}

public class Experience
{
    public string Company { get; set; }
    public string Context { get; set; }
    public string Location { get; set; }
    public string Title { get; set; }
    public string Duration { get; set; }
    public List<string> Description { get; set; } // List of bullet points for descriptions
}

public class Project
{
    public string Name { get; set; }
    public string Context { get; set; }
    public List<string> Description { get; set; } // List of bullet points for project descriptions
}

public class Skill
{
    public string Category { get; set; }
    public List<string> Skills { get; set; } // List of skills under this category
}

public class Education
{
    public string Institution { get; set; }
    public string SubInstitution { get; set; }
    public string Degree { get; set; }
    public string Years { get; set; } // E.g., "2011-2016, 2023"
}
