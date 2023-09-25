namespace LenkasLittleHelper.Models
{
    public abstract class City_Base : VM_WithNotify
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

    public class City_Pharmacies : City_Base
    {
        public string? CityTitleWithCount { get => cityTitleWithCount; private set => SetProperty(ref cityTitleWithCount, value); }
        public string? cityTitleWithCount;
        public City_Pharmacies(int id, string? title)
        {
            Id = id;
            CityName = title;
        }

        public void UpdateCounter(int count)
        {
            CityTitleWithCount = $"{CityName} ({count})";
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