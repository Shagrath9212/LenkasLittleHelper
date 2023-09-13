namespace LenkasLittleHelper.Models
{
    public class Hospital
    {
        public int IdHospital { get; }
        public string City { get; }
        public int IdCity { get; }
        public string Title { get; }

        public Hospital(int idHospital, string city, int idCity, string title)
        {
            IdHospital = idHospital;
            City = city;
            IdCity = idCity;
            Title = title;
        }
    }
}