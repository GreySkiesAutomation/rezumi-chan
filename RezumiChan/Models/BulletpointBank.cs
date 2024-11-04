namespace RezumiChan.Models;

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class Bulletpoint
{
    [JsonProperty("ID")] public int ID { get; set; }

    [JsonProperty("bulletpoint")] public string BulletpointText { get; set; }
}

public class BulletpointBank
{
    [JsonProperty("bulletpoints")] public List<Bulletpoint> Bulletpoints { get; set; }

    public List<string> GetBulletpointTextsByIds(List<int> ids)
    {
        List<string> bulletpointTexts = new List<string>();

        foreach (int id in ids)
        {
            // Find the bullet point with the matching ID
            Bulletpoint foundPoint;
            foreach (var bulletpoint in Bulletpoints)
            {
                if (bulletpoint.ID == id)
                {
                    bulletpointTexts.Add(bulletpoint.BulletpointText);
                }
            }
        }

        return bulletpointTexts;
    }
}