using System.Text.Json.Serialization;

namespace PfsAssessment;
public class CourseDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("creditHours")]
    public double CreditHours { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonPropertyName("grade")]
    public double Grade { get; set; }
}