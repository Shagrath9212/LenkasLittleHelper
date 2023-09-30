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

    public class City_Directory : City_Base
    {
        public bool IsArchived { get; }

        public City_Directory(int id, string? cityName, bool isArchived)
        {
            Id = id;
            CityName = cityName;
            IsArchived = isArchived;
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
        public int CntDoctors { get => cntDoctors; private set => SetProperty(ref cntDoctors, value); }
        public int cntDoctors;

        public int CntPharmacies { get => cntPharmacies; private set => SetProperty(ref cntPharmacies, value); }
        public int cntPharmacies;

        public int CntHospitals { get => cntHospitals; private set => SetProperty(ref cntHospitals, value); }
        public int cntHospitals;
        public ReportCity(int id, string? title, int idReportCity)
        {
            Id = id;
            CityName = title;
            IdReportCity = idReportCity;
        }

        public void UpdateCounter(int countDoctors, int countPharmacies, int countHospitals)
        {
            CntDoctors = countDoctors;
            CntPharmacies = countPharmacies;
            CntHospitals = countHospitals;
        }
    }
}