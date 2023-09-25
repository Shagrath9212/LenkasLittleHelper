namespace LenkasLittleHelper.Models
{
    public class Pharmacy
    {
        public int IdPharmacy { get; }
        public string? City { get; }
        public string? FullName { get; }
        public string? Street { get; }
        public string? BuildNum { get; }
        public bool IsArchived { get; }
        public string Address => GetAddress();

        private string GetAddress()
        {
            return $"{Street}, {BuildNum}";
        }

        public Pharmacy(int idPharmacy, string? city, string? fullName, string? street, string? buildNum, bool isArchived)
        {
            IdPharmacy = idPharmacy;
            City = city;
            FullName = fullName;
            Street = street;
            BuildNum = buildNum;
            IsArchived = isArchived;
        }
    }
}