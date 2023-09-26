namespace LenkasLittleHelper.Models
{
    public abstract class PharmacyBase
    {
        public int IdPharmacy { get; protected set; }
        public string? FullName { get; protected set; }
        public string? Street { get; protected set; }
        public string? BuildNum { get; protected set; }
        public string Address => GetAddress();
        protected string GetAddress()
        {
            return $"{Street}, {BuildNum}";
        }
    }

    public class Pharmacy : PharmacyBase
    {
        public bool IsChecked { get; set; }
        public Pharmacy(int idPharmacy, string? fullName, string? street, string? buildNum)
        {
            IdPharmacy = idPharmacy;
            FullName = fullName;
            Street = street;
            BuildNum = buildNum;
        }
    }

    public class PharmacyDirectory : PharmacyBase
    {
        public string? City { get; }

        public bool IsArchived { get; }

        public PharmacyDirectory(int idPharmacy, string? city, string? fullName, string? street, string? buildNum, bool isArchived)
        {
            IdPharmacy = idPharmacy;
            City = city;
            FullName = fullName;
            Street = street;
            BuildNum = buildNum;
            IsArchived = isArchived;
        }
    }

    public class PharmacyReport : PharmacyBase
    {
        public readonly int IdReportPharmacy;

        public PharmacyReport(int idReportPharmacy, int idPharmacy, string? fullName, string? street, string? buildNum)
        {
            IdReportPharmacy = idReportPharmacy;
            IdPharmacy = idPharmacy;
            FullName = fullName;
            Street = street;
            BuildNum = buildNum;
        }
    }
}