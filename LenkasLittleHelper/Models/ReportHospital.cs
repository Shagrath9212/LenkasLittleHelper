namespace LenkasLittleHelper.Models
{
    internal class ReportHospital
    {
        public int IdReportHospital { get; }
        public int IdReportDay { get; }
        public int IdHospital { get; }
        public string NameHospital { get; }

        public ReportHospital(int idReportHospital, int idReportDay, int idHospital, string nameHospital)
        {
            IdReportHospital = idReportHospital;
            IdReportDay = idReportDay;
            IdHospital = idHospital;
            NameHospital = nameHospital;
        }
    }
}