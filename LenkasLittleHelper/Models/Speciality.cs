namespace LenkasLittleHelper.Models
{
    public abstract class Speciality_Base
    {
        public int IdSpeciality { get; protected set; }
        public string? Name { get; protected set; }
    }
    internal class Speciality : Speciality_Base
    {
        public Speciality(int idSpeciality, string? name)
        {
            IdSpeciality = idSpeciality;
            Name = name;
        }
    }
    public class Speciality_Directory : Speciality_Base
    {
        public bool IsArchived { get; }
        public Speciality_Directory(int idSpeciality, string? name, bool isArchived)
        {
            IdSpeciality = idSpeciality;
            Name = name;
            IsArchived = isArchived;
        }
    }
}