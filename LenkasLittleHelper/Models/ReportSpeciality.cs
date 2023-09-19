namespace LenkasLittleHelper.Models
{
    internal class ReportSpeciality
    {
        public int IdSpeciality { get; }
        public string? Name { get; }

        public ReportSpeciality(int idSpeciality, string? name)
        {
            IdSpeciality = idSpeciality;
            Name = name;
        }
    }
}