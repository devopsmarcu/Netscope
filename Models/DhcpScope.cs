using System;

namespace NetScope.Models
{
    public class DhcpScope
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ScopeId { get; set; } = string.Empty;
        public string ServerAddress { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public override string ToString() => $"{ScopeId} ({Name})";
    }
}
