using System.Collections.Generic;

namespace LenkasLittleHelper.Models
{
    internal class ReportBuilding
    {
        public readonly string? Street;
        public readonly string? NumBuilding;

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        public IEnumerable<(string, string)>? Doctors => _doctors;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

        private readonly List<(string?, string?)> _doctors = new();

        public ReportBuilding(string? street, string? numBuilding)
        {
            Street = street;
            NumBuilding = numBuilding;
        }

        public void AddDoctor(string? nameDoctor, string? speciality)
        {
            _doctors.Add((nameDoctor, speciality));
        }
    }
}