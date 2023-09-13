namespace LenkasLittleHelper.Models
{
    public class City
    {
        public int Id { get; }
        public string CityName { get; }
        public City(int id, string title)
        {
            Id = id;
            CityName = title;
        }
    }
}