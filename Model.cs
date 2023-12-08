using Microsoft.EntityFrameworkCore;
using PfsAssessment;

public class PfsAssessmentContext : DbContext
{
    public DbSet<StudentDTO> StudentDTOs { get; set; }

    public string DbPath { get; }

    public PfsAssessmentContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "pfsAssessment.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // cleared out personal connection string details, will need to fill in username and password 
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=<yourUserName>;Password=<yourPassword>;Database=PfsAssessment");
    }

}