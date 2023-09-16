namespace LenkasLittleHelper.Models
{
    public abstract class Doctor_Base
    {
        public int IdDoctor { get; protected set; }
        public string? FullName { get; protected set; }
        public string? Speciality { get; protected set; }
        public string? Street { get; protected set; }
        public string? BuildNumber { get; protected set; }
        public string? Address
        {
            get { return $"{Street},{BuildNumber}"; }
        }
    }

    public class Doctor : Doctor_Base
    {
        public string? DoctorTitle
        {
            get { return $"{FullName}, {Speciality}, {Address}"; }
        }

        public Doctor(int idDoctor, string? fullName, string? speciality, string? street, string? buildNumber)
        {
            IdDoctor = idDoctor;
            FullName = fullName;
            Speciality = speciality;
            Street = street;
            BuildNumber = buildNumber;
        }
    }

    public class Doctor_Directory : Doctor_Base
    {
        public string PhoneNum { get; }
        public string Category { get; }
        public bool Visitable { get; }
        public Doctor_Directory(int idDoctor, string fullName, string speciality, string phoneNum, string street, string buildNumber, string category, bool visitable)
        {
            IdDoctor = idDoctor;
            FullName = fullName;
            Speciality = speciality;
            Street = street;
            PhoneNum = phoneNum;
            BuildNumber = buildNumber;
            Category = category;
            Visitable = visitable;
        }
    }

    /// <summary>
    /// Модель для лікаря, яка використовується при формуванні звіту
    /// </summary>
    public class Doctor_Report : Doctor_Base
    {
        public int IdReportDoctor { get; }
        public Doctor_Report(int idReportDoctor, int idDoctor, string? fullName, string? speciality, string? street, string? buildNumber)
        {
            IdReportDoctor = idReportDoctor;
            IdDoctor = idDoctor;
            FullName = fullName;
            Speciality = speciality;
            Street = street;
            BuildNumber = buildNumber;
        }
    }
}