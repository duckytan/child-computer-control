using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class WebMonitor
    {
        private readonly AppConfiguration _config;
        private readonly string[] _browserPaths = new string[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\History"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Edge\User Data\Default\History"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Mozilla\Firefox\Profiles")
        };

        private readonly Dictionary<string, DateTime> _lastCheckedTime = new Dictionary<string, DateTime>();

        public WebMonitor(AppConfiguration config)
        {
            _config = config;
        }

        public void CheckCurrentBrowsers()
        {
            if (_config.BlockedSites == null || _config.BlockedSites.Count == 0) return;

            foreach (var browserPath in _browserPaths)
            {
                if (browserPath.Contains("Firefox"))
                {
                    CheckFirefoxHistory();
                }
                else if (File.Exists(browserPath))
                {
                    CheckChromeOrEdgeHistory(browserPath);
                }
            }
        }

        private void CheckChromeOrEdgeHistory(string historyPath)
        {
            try
            {
                if (!File.Exists(historyPath)) return;

                string[] lines = { };
                try
                {
                    lines = File.ReadAllLines(historyPath);
                }
                catch
                {
                    return;
                }

                foreach (var line in lines)
                {
                    if (line.Contains("youtube.com") || line.Contains("youtube"))
                    {
                        CheckBlockedSitesInUrl(line);
                    }
                }
            }
            catch { }
        }

        private void CheckFirefoxHistory()
        {
            try
            {
                string firefoxPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Mozilla\Firefox\Profiles");

                if (!Directory.Exists(firefoxPath)) return;

                var profiles = Directory.GetDirectories(firefoxPath);
                foreach (var profile in profiles)
                {
                    string historyPath = Path.Combine(profile, "places.sqlite");
                    if (File.Exists(historyPath))
                    {
                    }
                }
            }
            catch { }
        }

        private void CheckBlockedSitesInUrl(string url)
        {
            if (_config.BlockedSites == null || _config.BlockedSites.Count == 0) return;

            foreach (var blockedSite in _config.BlockedSites)
            {
                if (url.Contains(blockedSite, StringComparison.OrdinalIgnoreCase))
                {
                    EventLog.WriteEntry($"Blocked site accessed: {blockedSite}", EventLogEntryType.Warning);
                    RecordWebAccess(url, blockedSite);
                }
            }
        }

        private void RecordWebAccess(string url, string domain)
        {
            try
            {
                var record = new WebUsageRecord
                {
                    Timestamp = DateTime.Now,
                    Url = url,
                    Domain = domain,
                    Title = domain
                };

                var log = new WebUsageLog
                {
                    Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    Records = new List<WebUsageRecord> { record }
                };

                string logPath = Path.Combine(@"C:\ProgramData\ChildPCGuard\logs",
                    $"web_{DateTime.Today:yyyy-MM-dd}.json");

                List<WebUsageLog> existingLogs = new List<WebUsageLog>();
                if (File.Exists(logPath))
                {
                    var json = File.ReadAllText(logPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    existingLogs = JsonSerializer.Deserialize<List<WebUsageLog>>(json, options) ?? new List<WebUsageLog>();
                }

                existingLogs.Add(log);

                var options2 = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
                File.WriteAllText(logPath, JsonSerializer.Serialize(existingLogs, options2));
            }
            catch { }
        }

        public bool IsBlockedSiteAccessed()
        {
            if (_config.BlockedSites == null || _config.BlockedSites.Count == 0) return false;

            foreach (var browserPath in _browserPaths)
            {
                if (browserPath.Contains("Firefox"))
                {
                    if (CheckFirefoxBlockedSites())
                        return true;
                }
                else if (File.Exists(browserPath))
                {
                    if (CheckChromeOrEdgeBlockedSites(browserPath))
                        return true;
                }
            }

            return false;
        }

        private bool CheckChromeOrEdgeBlockedSites(string historyPath)
        {
            try
            {
                if (!File.Exists(historyPath)) return false;

                string[] lines = { };
                try
                {
                    lines = File.ReadAllLines(historyPath);
                }
                catch
                {
                    return false;
                }

                foreach (var line in lines)
                {
                    foreach (var blockedSite in _config.BlockedSites)
                    {
                        if (line.Contains(blockedSite, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private bool CheckFirefoxBlockedSites()
        {
            return false;
        }
    }
}
