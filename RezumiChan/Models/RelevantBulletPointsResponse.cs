using Newtonsoft.Json;

namespace RezumiChan.Models;

public class RelevantBulletPointsResponse
{
    [JsonProperty("relevant_bullet_points")]
    public List<int> RelevantBulletPoints { get; set; }
}
