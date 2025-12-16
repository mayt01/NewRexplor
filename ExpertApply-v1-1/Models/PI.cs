namespace Rexplor.Models
{
    public class PI
    {
        public int Id { get; set; }
        public string UniversityName { get; set; }
        public string PIName { get; set; }
        public string GoogleScholarUrl { get; set; }
        public string WebsiteUrl { get; set; }

        public string Email { get; set; }
        public string ResearchField { get; set; }
        public string? ProfilePictureUrl { get; set; } // Nullable
        public PI() { }
        public PI(int id, string universityName, string professorName, string googleScholarUrl, string websiteUrl, string email, string researchField, string profilePictureUrl = null)
        {
            Id = id;
            UniversityName = universityName;
            PIName = professorName;
            GoogleScholarUrl = googleScholarUrl;
            WebsiteUrl = websiteUrl;
            Email = email;
            ResearchField = researchField;
            ProfilePictureUrl = profilePictureUrl;
        }

        // تابع برای نمایش اطلاعات
        public override string ToString()
        {
            return $"ID: {Id}\nUniversity: {UniversityName}\nProfessor: {PIName}\nGoogle Scholar: {GoogleScholarUrl}\nWebsite: {WebsiteUrl}\nEmail: {Email}\nResearch Field: {ResearchField}\nProfile Picture: {ProfilePictureUrl}";
        }
    }

}
