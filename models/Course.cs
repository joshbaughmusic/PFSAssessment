using System.Text.Json.Serialization;

namespace PfsAssessment;

public class Course
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("creditHours")]
    public double CreditHours { get; set; }
    public List<Student> EnrolledStudents { get; set; }

}