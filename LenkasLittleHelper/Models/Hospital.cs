namespace LenkasLittleHelper.Models
{
    public class Hospital
    {
        public int IdHospital { get; protected set; }
        public string City { get; protected set; }
        public int IdCity { get; protected set; }
        public string Title { get; protected set; }

        public Hospital(int idHospital, string city, int idCity, string title)
        {
            IdHospital = idHospital;
            City = city;
            IdCity = idCity;
            Title = title;
        }
    }
}