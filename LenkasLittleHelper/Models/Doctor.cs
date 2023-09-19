using LenkasLittleHelper.Database;

namespace LenkasLittleHelper.Models
{
    public abstract class Doctor_Base : AddressBase
    {
        public int IdDoctor { get; protected set; }
        public string? FullName { get; protected set; }

        public string? Speciality { get; protected set; }
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
        public int IdSpeciality { get; }
        public int IdCategory { get; }
        public int IdAddress { get; }
        public string? City { get; }
        public string? NameHospital { get; }

        public int GetIDHospital()
        {
            string sql = $"SELECT A.ID_HOSPITAL FROM DOCTORS D LEFT JOIN ADDRESSES A ON D.ID_ADDRESS=A.ID_ADDRESS WHERE D.ID_DOCTOR={IdDoctor}";

            int idHospital = -1;
            DBHelper.ExecuteReader(sql, e =>
            {
                if (e.Read())
                {
                    idHospital = e.GetValueOrDefault<int>("ID_HOSPITAL");
                }
            });

            return idHospital;
        }

        public Doctor_Directory(int idDoctor, string fullName, string speciality, string phoneNum, string street, string buildNumber, string category, bool visitable, int idSpeciality, int idCategory, int idAddress, string? city, string? nameHospital)
        {
            IdDoctor = idDoctor;
            FullName = fullName;
            Speciality = speciality;
            Street = street;
            PhoneNum = phoneNum;
            BuildNumber = buildNumber;
            Category = category;
            Visitable = visitable;
            IdSpeciality = idSpeciality;
            IdCategory = idCategory;
            IdAddress = idAddress;
            City = city;
            NameHospital = nameHospital;
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