using System;
using System.Collections.Generic;
using NetScope.Models;

namespace NetScope.Models
{
    public class Settings
    {
        public string Language { get; set; } = "auto"; // Default to auto detection
    }

    public class AppData
    {
        public List<DhcpServer> Servers { get; set; } = new List<DhcpServer>();
        public List<MacFilterPolicy> Policies { get; set; } = new List<MacFilterPolicy>();
        public List<DhcpScope> Scopes { get; set; } = new List<DhcpScope>();
        public Settings Settings { get; set; } = new Settings();
    }
}
