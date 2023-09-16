namespace LenkasLittleHelper.Models
{
    public abstract class City_Base
    {
        public int Id { get; protected set; }
        public string? CityName { get; protected set; }
    }

    public class City : City_Base
    {
        public City(int id, string? title)
        {
            Id = id;
            CityName = title;
        }
    }
    public class ReportCity : City_Base
    {
        public int IdReportCity { get; }
        public ReportCity(int id, string? title, int idReportCity)
        {
            Id = id;
            CityName = title;
            IdReportCity = idReportCity;
        }
    }
}