namespace LenkasLittleHelper.Models
{
    internal class ReportCategory
    {
        public int IdCategory { get; }
        public string? Title { get; }
        public ReportCategory(int idCategory, string? title)
        {
            IdCategory = idCategory;
            Title = title;
        }
    }
}