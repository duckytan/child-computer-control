using System;
using System.Collections.Generic;
using System.Diagnostics;
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

                if (IsFileLocked(historyPath)) return;

                var lastChecked = _lastCheckedTime.ContainsKey(historyPath)
                    ? _lastCheckedTime[historyPath]
                    : DateTime.MinValue;

                var fileInfo = new FileInfo(historyPath);
                if (fileInfo.LastWriteTime <= lastChecked) return;

                byte[] buffer = new byte[4096];
                using (var fs = new FileStream(historyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }

                string content = System.Text.Encoding.UTF8.GetString(buffer);
                foreach (var blockedSite in _config.BlockedSites)
                {
                    if (content.Contains(blockedSite, StringComparison.OrdinalIgnoreCase))
                    {
                        EventLog.WriteEntry($"Blocked site accessed: {blockedSite}", EventLogEntryType.Warning);
                        RecordWebAccess($"*{blockedSite}*", blockedSite);
                    }
                }

                _lastCheckedTime[historyPath] = DateTime.Now;
            }
            catch { }
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
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
                        CheckFirefoxHistoryFile(historyPath);
                    }
                }
            }
            catch { }
        }

        private void CheckFirefoxHistoryFile(string historyPath)
        {
            try
            {
                if (IsFileLocked(historyPath)) return;

                var lastChecked = _lastCheckedTime.ContainsKey(historyPath)
                    ? _lastCheckedTime[historyPath]
                    : DateTime.MinValue;

                var fileInfo = new FileInfo(historyPath);
                if (fileInfo.LastWriteTime <= lastChecked) return;

                byte[] buffer = new byte[8192];
                using (var fs = new FileStream(historyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }

                string content = System.Text.Encoding.UTF8.GetString(buffer);
                foreach (var blockedSite in _config.BlockedSites)
                {
                    if (content.Contains(blockedSite, StringComparison.OrdinalIgnoreCase))
                    {
                        EventLog.WriteEntry($"Blocked site accessed: {blockedSite}", EventLogEntryType.Warning);
                        RecordWebAccess($"*{blockedSite}*", blockedSite);
                    }
                }

                _lastCheckedTime[historyPath] = DateTime.Now;
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

                string logPath = Path.Combine(@"C:\ProgramData\ChildPCGuard\logs",
                    $"web_{DateTime.Today:yyyy-MM-dd}.json");

                List<WebUsageRecord> existingRecords = new List<WebUsageRecord>();
                if (File.Exists(logPath))
                {
                    var json = File.ReadAllText(logPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var logs = JsonSerializer.Deserialize<List<WebUsageLog>>(json, options);
                    if (logs != null && logs.Count > 0)
                    {
                        existingRecords = logs[0].Records;
                    }
                }

                existingRecords.Add(record);

                var log = new WebUsageLog
                {
                    Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    Records = existingRecords
                };

                var options2 = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
                File.WriteAllText(logPath, JsonSerializer.Serialize(new[] { log }, options2));
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
                if (IsFileLocked(historyPath)) return false;

                byte[] buffer = new byte[4096];
                using (var fs = new FileStream(historyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }

                string content = System.Text.Encoding.UTF8.GetString(buffer);
                foreach (var blockedSite in _config.BlockedSites)
                {
                    if (content.Contains(blockedSite, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
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
