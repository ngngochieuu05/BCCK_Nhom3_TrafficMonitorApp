using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace TrafficMonitorApp
{
    /// <summary>
    /// Lưu trữ và quản lý lịch sử giao thông
    /// </summary>
    public class TrafficHistoryManager
    {
        private readonly string _historyFilePath;
        private List<TrafficSession> _sessions;

        public TrafficHistoryManager(string historyPath = "traffic_history.json")
        {
            _historyFilePath = historyPath;
            _sessions = LoadHistory();
        }

        public class TrafficSession
        {
            public string SessionId { get; set; } = "";
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public int TotalVehicles { get; set; }
            public Dictionary<string, int> VehicleCounts { get; set; } = new Dictionary<string, int>();
            public double AverageFPS { get; set; }
            public int ProcessedFrames { get; set; }
            public string SourceType { get; set; } = ""; // video, camera, image
            public string SourcePath { get; set; } = "";
            public List<HourlyData> HourlyBreakdown { get; set; } = new List<HourlyData>();
            public double CongestionLevel { get; set; }
            public string BusiestPeriod { get; set; } = "";
            public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        }

        public class HourlyData
        {
            public int Hour { get; set; }
            public int VehicleCount { get; set; }
            public Dictionary<string, int> VehicleTypes { get; set; } = new Dictionary<string, int>();
        }

        /// <summary>
        /// Lưu session mới vào lịch sử
        /// </summary>
        public void SaveSession(TrafficStatistics stats, AdvancedStatistics advStats, string sourceType, string sourcePath)
        {
            var session = new TrafficSession
            {
                SessionId = Guid.NewGuid().ToString(),
                StartTime = stats.StartTime,
                EndTime = stats.EndTime,
                TotalVehicles = stats.TotalVehicles,
                VehicleCounts = new Dictionary<string, int>(stats.VehicleCounts),
                AverageFPS = stats.AverageFPS,
                ProcessedFrames = stats.ProcessedFrames,
                SourceType = sourceType,
                SourcePath = sourcePath,
                CongestionLevel = advStats.GetCongestionLevel(),
                BusiestPeriod = advStats.GetBusiestPeriod(),
                Metadata = new Dictionary<string, object>
                {
                    { "Duration", (stats.EndTime - stats.StartTime).TotalMinutes },
                    { "VehiclesPerMinute", advStats.GetVehiclesPerMinute() }
                }
            };

            // Get hourly breakdown
            var hourlyData = advStats.GetHourlyData();
            foreach (var kvp in hourlyData)
            {
                session.HourlyBreakdown.Add(new HourlyData
                {
                    Hour = int.Parse(kvp.Key),
                    VehicleCount = kvp.Value
                });
            }

            _sessions.Add(session);
            SaveHistory();

            Console.WriteLine($"[History] Saved session: {session.SessionId} ({session.TotalVehicles} vehicles)");
        }

        /// <summary>
        /// Lấy sessions theo ngày
        /// </summary>
        public List<TrafficSession> GetSessionsByDate(DateTime date)
        {
            return _sessions.Where(s => s.StartTime.Date == date.Date).ToList();
        }

        /// <summary>
        /// Lấy sessions trong khoảng thời gian
        /// </summary>
        public List<TrafficSession> GetSessionsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _sessions.Where(s => s.StartTime.Date >= startDate.Date && s.StartTime.Date <= endDate.Date).ToList();
        }

        /// <summary>
        /// Thống kê tổng hợp theo ngày
        /// </summary>
        public Dictionary<string, object> GetDailySummary(DateTime date)
        {
            var sessions = GetSessionsByDate(date);
            if (sessions.Count == 0)
                return new Dictionary<string, object> { { "HasData", false } };

            var totalVehicles = sessions.Sum(s => s.TotalVehicles);
            var avgCongestion = sessions.Average(s => s.CongestionLevel);
            
            var vehicleTypeTotals = new Dictionary<string, int>();
            foreach (var session in sessions)
            {
                foreach (var kvp in session.VehicleCounts)
                {
                    if (!vehicleTypeTotals.ContainsKey(kvp.Key))
                        vehicleTypeTotals[kvp.Key] = 0;
                    vehicleTypeTotals[kvp.Key] += kvp.Value;
                }
            }

            return new Dictionary<string, object>
            {
                { "HasData", true },
                { "Date", date.ToString("dd/MM/yyyy") },
                { "TotalVehicles", totalVehicles },
                { "SessionCount", sessions.Count },
                { "AverageCongestion", avgCongestion },
                { "VehicleTypes", vehicleTypeTotals },
                { "FirstSession", sessions.First().StartTime.ToString("HH:mm:ss") },
                { "LastSession", sessions.Last().EndTime.ToString("HH:mm:ss") }
            };
        }

        /// <summary>
        /// Tìm giờ cao điểm trong lịch sử
        /// </summary>
        public List<KeyValuePair<int, int>> GetHistoricalPeakHours(DateTime? date = null)
        {
            var sessions = date.HasValue ? GetSessionsByDate(date.Value) : _sessions;
            
            var hourlyTotals = new Dictionary<int, int>();
            foreach (var session in sessions)
            {
                foreach (var hourly in session.HourlyBreakdown)
                {
                    if (!hourlyTotals.ContainsKey(hourly.Hour))
                        hourlyTotals[hourly.Hour] = 0;
                    hourlyTotals[hourly.Hour] += hourly.VehicleCount;
                }
            }

            return hourlyTotals.OrderByDescending(x => x.Value).Take(5).ToList();
        }

        /// <summary>
        /// So sánh giữa các ngày
        /// </summary>
        public Dictionary<string, object> CompareDate(DateTime date1, DateTime date2)
        {
            var summary1 = GetDailySummary(date1);
            var summary2 = GetDailySummary(date2);

            if (!(bool)summary1["HasData"] || !(bool)summary2["HasData"])
                return new Dictionary<string, object> { { "Error", "Khong du du lieu de so sanh" } };

            int total1 = (int)summary1["TotalVehicles"];
            int total2 = (int)summary2["TotalVehicles"];
            double change = total1 > 0 ? ((total2 - total1) * 100.0 / total1) : 0;

            return new Dictionary<string, object>
            {
                { "Date1", summary1 },
                { "Date2", summary2 },
                { "ChangePercent", change },
                { "ChangeDirection", change > 0 ? "Tang" : (change < 0 ? "Giam" : "Khong doi") }
            };
        }

        /// <summary>
        /// Lấy xu hướng theo tuần
        /// </summary>
        public List<Dictionary<string, object>> GetWeeklyTrend()
        {
            var result = new List<Dictionary<string, object>>();
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-6);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var summary = GetDailySummary(date);
                result.Add(summary);
            }

            return result;
        }

        /// <summary>
        /// Tìm kiếm sessions
        /// </summary>
        /// <summary>
        /// Thống kê tổng hợp tất cả
        /// </summary>
        public Dictionary<string, object> GetOverallStatistics()
        {
            if (_sessions.Count == 0)
                return new Dictionary<string, object> { { "HasData", false } };

            var totalVehicles = _sessions.Sum(s => s.TotalVehicles);
            var avgCongestion = _sessions.Average(s => s.CongestionLevel);
            
            var vehicleTypeTotals = new Dictionary<string, int>();
            foreach (var session in _sessions)
            {
                foreach (var kvp in session.VehicleCounts)
                {
                    if (!vehicleTypeTotals.ContainsKey(kvp.Key))
                        vehicleTypeTotals[kvp.Key] = 0;
                    vehicleTypeTotals[kvp.Key] += kvp.Value;
                }
            }

            return new Dictionary<string, object>
            {
                { "HasData", true },
                { "TotalSessions", _sessions.Count },
                { "TotalVehicles", totalVehicles },
                { "AverageCongestion", avgCongestion },
                { "VehicleTypes", vehicleTypeTotals },
                { "FirstRecordDate", _sessions.Min(s => s.StartTime).ToString("dd/MM/yyyy") },
                { "LastRecordDate", _sessions.Max(s => s.StartTime).ToString("dd/MM/yyyy") },
                { "MostActiveDate", GetMostActiveDate() }
            };
        }

        private string GetMostActiveDate()
        {
            if (_sessions.Count == 0) return "N/A";

            var dateGroups = _sessions.GroupBy(s => s.StartTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Sum(s => s.TotalVehicles) })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            return dateGroups?.Date.ToString("dd/MM/yyyy") ?? "N/A";
        }

        private List<TrafficSession> LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    string json = File.ReadAllText(_historyFilePath);
                    return JsonConvert.DeserializeObject<List<TrafficSession>>(json) ?? new List<TrafficSession>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[History] Error loading: {ex.Message}");
            }

            return new List<TrafficSession>();
        }

        private void SaveHistory()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_sessions, Formatting.Indented);
                File.WriteAllText(_historyFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[History] Error saving: {ex.Message}");
            }
        }

        public void ClearHistory()
        {
            _sessions.Clear();
            SaveHistory();
        }

        public int GetTotalSessions() => _sessions.Count;
    }
}
