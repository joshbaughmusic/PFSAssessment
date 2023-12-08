using System.Net.Http.Headers;
using System.Text.Json;
using PfsAssessment;

class Program
{
    private const string ApiBaseUrl = "http://university.pinnstrat.com:8888";
    private static readonly HttpClient Client = new HttpClient();

    static async Task Main()
    {
        Console.WriteLine("Welcome!");


        while (true)
        {
            Console.WriteLine("Options:");
            Console.WriteLine("1. Get all students and add them to the database");
            Console.WriteLine("2. Automatic-Drop for full-time students with low grades");
            Console.WriteLine("3. Validate");
            Console.WriteLine("4. Exit");

            Console.Write("Enter your choice (1-4): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await GetAllStudentsAndAddToDatabase();
                    break;
                case "2":
                    await AutomaticDropForFullTimeStudents();
                    break;
                case "3":
                    await Validate();
                    break;
                case "4":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private static async Task GetAllStudentsAndAddToDatabase()
    {
        using var dbContext = new PfsAssessmentContext();

        Client.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var studentsRes = await Client.GetAsync($"{ApiBaseUrl}/student");

        if (studentsRes.IsSuccessStatusCode)
        {
            var studentsJson = await studentsRes.Content.ReadAsStreamAsync();
            var studentsData = await JsonSerializer.DeserializeAsync<List<Student>>(studentsJson);

            List<StudentDTO> studentDTOList = new List<StudentDTO>();

            foreach (Student student in studentsData)
            {
                var studentDetailsRes = await Client.GetAsync($"{ApiBaseUrl}/student/{student.Id}");

                if (studentDetailsRes.IsSuccessStatusCode)
                {
                    var studentDetailsJson = await studentDetailsRes.Content.ReadAsStreamAsync();
                    var studentDetailsData = await JsonSerializer.DeserializeAsync<StudentDetails>(studentDetailsJson);

                    if (studentDetailsData.Id != 0)
                    {
                        double totalGradePoints = studentDetailsData.Courses
                        .Where(course => course.Status == "Enrolled")
                        .Sum(course => course.Grade * course.CreditHours);

                        double totalCreditHours = studentDetailsData.Courses
                            .Where(course => course.Status == "Enrolled")
                            .Sum(course => course.CreditHours);

                        double semesterGPA = totalCreditHours > 0
                            ? totalGradePoints / totalCreditHours
                            : 0;

                        double convertedGPA = Math.Round(semesterGPA / 100 * 4, 2);

                        studentDTOList.Add(new StudentDTO
                        {
                            Id = studentDetailsData.Id,
                            Name = studentDetailsData.Name,
                            Email = studentDetailsData.Email,
                            TotalCreditHours = totalCreditHours,
                            AvgSemesterGpa = convertedGPA
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Error fetching details for student {student.Id}");
                    }
                }
                else
                {
                    Console.WriteLine($"Error fetching details for student {student.Id}");
                }
            }

            dbContext.StudentDTOs.AddRange(studentDTOList);
            dbContext.SaveChanges();

            Console.WriteLine("All students have been added to the database.");
        }
        else
        {
            Console.WriteLine($"Error fetching students.");
        }
    }

    private static async Task AutomaticDropForFullTimeStudents()
    {
        var reset = await Client.PostAsync($"{ApiBaseUrl}/reset", null);
        if (reset.IsSuccessStatusCode)
        {
            Console.WriteLine($"Successfully reset endpoint");
        }
        else
        {
            Console.WriteLine($"Failed to reset endpoint");
        }

        using var dbContext = new PfsAssessmentContext();

        var studentsRes = await Client.GetAsync($"{ApiBaseUrl}/student");

        if (studentsRes.IsSuccessStatusCode)
        {
            var studentsJson = await studentsRes.Content.ReadAsStreamAsync();
            var studentsData = await JsonSerializer.DeserializeAsync<List<Student>>(studentsJson);

            foreach (Student student in studentsData)
            {
                var studentDetailsRes = await Client.GetAsync($"{ApiBaseUrl}/student/{student.Id}");

                if (studentDetailsRes.IsSuccessStatusCode)
                {
                    var studentDetailsJson = await studentDetailsRes.Content.ReadAsStreamAsync();
                    var studentDetailsData = await JsonSerializer.DeserializeAsync<StudentDetails>(studentDetailsJson);

                    bool isFullTime = studentDetailsData.Courses.Where(course => course.Status == "Enrolled").Sum(course => course.CreditHours) > 10;

                    List<CourseDetails> lowGradeCourses = studentDetailsData.Courses
                            .Where(course => course.Status == "Enrolled" && course.Grade < 40)
                            .OrderBy(course => course.Grade)
                            .ToList();

                    if (isFullTime && lowGradeCourses.Count > 0)
                    {

                        double remainingCreditHours = studentDetailsData.Courses
                            .Where(course => course.Status == "Enrolled")
                            .Sum(course => course.CreditHours);

                        foreach (var course in lowGradeCourses)
                        {
                            if (remainingCreditHours - course.CreditHours >= 10)
                            {
                                remainingCreditHours -= course.CreditHours;

                                await DropCourse(course.Id, student.Id);

                                Console.WriteLine($"Dropped course {course.Id} for student {student.Id}");
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error fetching details for student {student.Id}");
                }
            }

            Console.WriteLine("Automatic drop for full-time students with low grades completed.");
        }
        else
        {
            Console.WriteLine($"Error fetching students.");
        }
    }

    private static async Task DropCourse(int courseId, int studentId)
    {
        var dropCourseEndpoint = $"{ApiBaseUrl}/Course/{courseId}/Drop/{studentId}";

        var response = await Client.PostAsync(dropCourseEndpoint, null);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Course dropped successfully for student {studentId} in course {courseId}.");
        }
        else
        {
            Console.WriteLine($"Error dropping course for student {studentId} in course {courseId}. Status code: {response.StatusCode}");
        }
    }

    private static async Task Validate()
    {
        var validateRes = await Client.GetAsync($"{ApiBaseUrl}/validate");

        if (validateRes.IsSuccessStatusCode)
        {
            Console.WriteLine($"Success!");
        }
        else
        {
            Console.WriteLine($"Failure.");
        }
    }
}
