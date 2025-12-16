namespace Rexplor.Models.Main_Page
{
    public class HomeViewModel
    {
        public List<DataFile> LatestFiles { get; set; }
        public List<DataFile> PopularFiles { get; set; }
        public List<CategoryWithCount> Categories { get; set; }
        public AchievementsAndFeedbacks AchievementsAndFeedbacks { get; set; }  
    }

    public class CategoryWithCount
    {
        public DataFileCategory Category { get; set; }
        public int FileCount { get; set; }
    }
}
