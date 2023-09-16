namespace LenkasLittleHelper.Models
{
    public abstract class Hospital_Base
    {
        public int IdHospital { get; protected set; }
        public string? Title { get; protected set; }
    }

    public class Hospital : Hospital_Base
    {
        public Hospital(int idHospital, string? hospitalName)
        {
            IdHospital = idHospital;
            Title = hospitalName;
        }
    }

    public class Hospital_Directories : Hospital_Base
    {
        public string City { get; protected set; }
        public int IdCity { get; protected set; }

        public Hospital_Directories(int idHospital, string city, int idCity, string title)
        {
            IdHospital = idHospital;
            City = city;
            IdCity = idCity;
            Title = title;
        }
    }

    public class ReportHospital : Hospital_Base
    {
        public int IdReportHospital { get; }
        public ReportHospital(int idReportHospital, int idHospital, string? nameHospital)
        {

            IdReportHospital = idReportHospital;
            IdHospital = idHospital;
            Title = nameHospital;
        }
    }
}