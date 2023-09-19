namespace LenkasLittleHelper.Models
{
    public abstract class AddressBase
    {
        public string? Street { get; protected set; }
        public string? BuildNumber { get; protected set; }
        public string? Address
        {
            get { return $"{Street},{BuildNumber}"; }
        }
    }

    public class Address : AddressBase
    {
        public int IdAddress { get; }
        public Address(int idAddress, string? street, string? buildNumber)
        {
            IdAddress = idAddress;
            Street = street;
            BuildNumber = buildNumber;
        }
    }
}