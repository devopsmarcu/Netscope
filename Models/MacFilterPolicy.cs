using System;

namespace NetScope.Models
{
    public class MacFilterPolicy
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public override string ToString() => Name;
    }
}
