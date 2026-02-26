using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NetScope.Helpers
{
    public static class ValidationHelper
    {
        private static readonly Regex HostnameRegex = new Regex(@"^[a-zA-Z0-9.-]{1,255}$", RegexOptions.Compiled);
        private static readonly Regex IpRegex = new Regex(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$", RegexOptions.Compiled);
        private static readonly Regex MacRegex = new Regex(@"^([0-9A-Fa-f]{2}[:-]?){5}([0-9A-Fa-f]{2})$", RegexOptions.Compiled);
        private static readonly char[] ForbiddenDescriptionChars = { ';', '|', '&', '$', '(', ')', '{', '}', '<', '>', '\r', '\n', '`' };

        public static bool IsValidHostname(string hostname)
        {
            if (string.IsNullOrWhiteSpace(hostname)) return false;
            return HostnameRegex.IsMatch(hostname);
        }

        public static bool IsValidIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;
            return IpRegex.IsMatch(ip);
        }

        public static bool IsValidMac(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac)) return false;
            return MacRegex.IsMatch(mac);
        }

        public static string SanitizeDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return string.Empty;
            
            string sanitized = new string(description.Where(c => !ForbiddenDescriptionChars.Contains(c)).ToArray());
            return sanitized.Length > 200 ? sanitized.Substring(0, 200) : sanitized;
        }

        public static string NormalizeMac(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac)) return string.Empty;
            return Regex.Replace(mac.ToUpper(), "[^A-F0-9]", "");
        }
    }
}
