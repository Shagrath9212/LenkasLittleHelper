using NPOI.OpenXmlFormats.Dml.Diagram;

namespace LenkasLittleHelper.Models
{
    public abstract class Hospital_Base : VM_WithNotify
    {
        public int IdHospital { get; protected set; }
        public string? Title { get; set; }
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
        public string? City { get; }

        public int IdCity { get; }

        public bool IsArchived { get; }

        /// <summary>
        /// Віртуальна колонка, яка вказує чи потрібно завантажувати усіх лікарів
        /// </summary>
        public bool DoLoadAll { get; }


        public Hospital_Directories(int idHospital, string? city, int idCity, string? title, bool isArchived, bool doLoadAll = false)
        {
            IdHospital = idHospital;
            City = city;
            IdCity = idCity;
            Title = title;
            IsArchived = isArchived;
            DoLoadAll = doLoadAll;
        }
    }

    public class ReportHospital : Hospital_Base
    {
        public int IdReportHospital { get; }
        public string? TitleWithCount { get => titleWithCount; private set => SetProperty(ref titleWithCount, value); }
        private string? titleWithCount;

        public void UpdateCounter(int count)
        {
            TitleWithCount = $"{Title} ({count})";
        }

        public ReportHospital(int idReportHospital, int idHospital, string? nameHospital)
        {
            IdReportHospital = idReportHospital;
            IdHospital = idHospital;
            Title = nameHospital;
        }
    }
}