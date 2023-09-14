namespace LenkasLittleHelper.Models
{
    public class Doctor
    {
        public int IdDoctor { get; }
        public string FullName { get; }
        public string Speciality { get; }
        public string PhoneNum { get; }
        public string Street { get; }
        public string BuildNumber { get; }
        public string Address
        {
            get { return $"{Street},{BuildNumber}"; }
        }
        public string Category { get; }
        public bool Visitable { get; }
        public Doctor(int idDoctor, string fullName, string speciality, string phoneNum, string street, string buildNumber, string category, bool visitable)
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
}