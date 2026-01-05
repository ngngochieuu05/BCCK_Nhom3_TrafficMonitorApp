using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;

namespace TrafficMonitorApp
{
    /// <summary>
    /// Alert and notification system for traffic monitoring
    /// </summary>
    public class AlertSystem
    {
        private readonly List<Alert> _activeAlerts = new List<Alert>();
        private readonly Queue<Alert> _alertHistory = new Queue<Alert>();
        private DateTime _lastSoundTime = DateTime.MinValue;
        private readonly TimeSpan _soundCooldown = TimeSpan.FromSeconds(5);

        public event EventHandler<Alert>? AlertTriggered;
        public event EventHandler<Alert>? AlertCleared;

        public class Alert
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public AlertType Type { get; set; }
            public AlertLevel Level { get; set; }
            public string Message { get; set; } = "";
            public DateTime Timestamp { get; set; }
            public bool IsActive { get; set; } = true;
            public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

            public Color GetColor()
            {
                return Level switch
                {
                    AlertLevel.Info => Color.FromArgb(66, 135, 245),
                    AlertLevel.Warning => Color.FromArgb(255, 159, 10),
                    AlertLevel.Critical => Color.FromArgb(255, 59, 48),
                    _ => Color.Gray
                };
            }

            public string GetIcon()
            {
                return Type switch
                {
                    AlertType.Congestion => "🚦",
                    AlertType.HighSpeed => "⚡",
                    AlertType.Violation => "⚠️",
                    AlertType.SystemHealth => "💻",
                    AlertType.LowFPS => "📉",
                    AlertType.HighTraffic => "🚗",
                    _ => "ℹ️"
                };
            }
        }

        public enum AlertType
        {
            Congestion,
            HighSpeed,
            Violation,
            SystemHealth,
            LowFPS,
            HighTraffic,
            CustomAlert
        }

        public enum AlertLevel
        {
            Info,
            Warning,
            Critical
        }

        // Configuration
        public bool EnableSoundAlerts { get; set; } = true;
        public int MaxAlertHistory { get; set; } = 100;
        public double CongestionThreshold { get; set; } = 75.0; // Percentage
        public double LowFPSThreshold { get; set; } = 15.0;
        public int HighTrafficThreshold { get; set; } = 30; // Vehicles per minute

        public int ActiveAlertsCount => _activeAlerts.Count;
        public IReadOnlyList<Alert> ActiveAlerts => _activeAlerts.AsReadOnly();
        public IReadOnlyList<Alert> AlertHistory => _alertHistory.ToList().AsReadOnly();

        /// <summary>
        /// Check congestion level and trigger alert if needed
        /// </summary>
        public void CheckCongestion(double congestionLevel)
        {
            string alertId = "congestion_alert";

            if (congestionLevel >= CongestionThreshold)
            {
                var level = congestionLevel >= 90 ? AlertLevel.Critical : AlertLevel.Warning;
                
                TriggerAlert(
                    alertId,
                    AlertType.Congestion,
                    level,
                    $"Tac duong: {congestionLevel:F1}%",
                    new Dictionary<string, object> { ["CongestionLevel"] = congestionLevel }
                );
            }
            else
            {
                ClearAlert(alertId);
            }
        }

        /// <summary>
        /// Check FPS and trigger alert if too low
        /// </summary>
        public void CheckFPS(double currentFPS)
        {
            string alertId = "low_fps_alert";

            if (currentFPS < LowFPSThreshold && currentFPS > 0)
            {
                TriggerAlert(
                    alertId,
                    AlertType.LowFPS,
                    AlertLevel.Warning,
                    $"FPS thap: {currentFPS:F1}",
                    new Dictionary<string, object> { ["FPS"] = currentFPS }
                );
            }
            else
            {
                ClearAlert(alertId);
            }
        }

        /// <summary>
        /// Check traffic volume and trigger alert if too high
        /// </summary>
        public void CheckTrafficVolume(double vehiclesPerMinute)
        {
            string alertId = "high_traffic_alert";

            if (vehiclesPerMinute >= HighTrafficThreshold)
            {
                TriggerAlert(
                    alertId,
                    AlertType.HighTraffic,
                    AlertLevel.Info,
                    $"Giao thong dong: {vehiclesPerMinute:F1} xe/phut",
                    new Dictionary<string, object> { ["VehiclesPerMinute"] = vehiclesPerMinute }
                );
            }
            else
            {
                ClearAlert(alertId);
            }
        }

        /// <summary>
        /// Trigger a new alert
        /// </summary>
        public void TriggerAlert(string id, AlertType type, AlertLevel level, string message, 
            Dictionary<string, object>? data = null)
        {
            // Check if alert already exists
            var existing = _activeAlerts.FirstOrDefault(a => a.Id == id);
            if (existing != null)
            {
                // Update existing alert
                existing.Message = message;
                existing.Timestamp = DateTime.Now;
                if (data != null)
                    existing.Data = data;
                return;
            }

            var alert = new Alert
            {
                Id = id,
                Type = type,
                Level = level,
                Message = message,
                Timestamp = DateTime.Now,
                Data = data ?? new Dictionary<string, object>()
            };

            _activeAlerts.Add(alert);
            AddToHistory(alert);

            Console.WriteLine($"[Alert] {alert.GetIcon()} {alert.Level}: {alert.Message}");

            // Play sound for critical alerts
            if (level == AlertLevel.Critical && EnableSoundAlerts)
            {
                PlayAlertSound();
            }

            AlertTriggered?.Invoke(this, alert);
        }

        /// <summary>
        /// Clear an active alert
        /// </summary>
        public void ClearAlert(string id)
        {
            var alert = _activeAlerts.FirstOrDefault(a => a.Id == id);
            if (alert != null)
            {
                alert.IsActive = false;
                _activeAlerts.Remove(alert);
                AlertCleared?.Invoke(this, alert);
            }
        }

        /// <summary>
        /// Clear all active alerts
        /// </summary>
        public void ClearAllAlerts()
        {
            var alerts = _activeAlerts.ToList();
            _activeAlerts.Clear();

            foreach (var alert in alerts)
            {
                alert.IsActive = false;
                AlertCleared?.Invoke(this, alert);
            }
        }

        /// <summary>
        /// Add alert to history
        /// </summary>
        private void AddToHistory(Alert alert)
        {
            _alertHistory.Enqueue(alert);

            while (_alertHistory.Count > MaxAlertHistory)
            {
                _alertHistory.Dequeue();
            }
        }

        /// <summary>
        /// Play alert sound with cooldown
        /// </summary>
        private void PlayAlertSound()
        {
            var now = DateTime.Now;
            if (now - _lastSoundTime < _soundCooldown)
                return;

            try
            {
                SystemSounds.Exclamation.Play();
                _lastSoundTime = now;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Alert] Failed to play sound: {ex.Message}");
            }
        }

        /// <summary>
        /// Get alert summary text
        /// </summary>
        public string GetSummary()
        {
            if (_activeAlerts.Count == 0)
                return "✅ Khong co canh bao";

            var critical = _activeAlerts.Count(a => a.Level == AlertLevel.Critical);
            var warning = _activeAlerts.Count(a => a.Level == AlertLevel.Warning);
            var info = _activeAlerts.Count(a => a.Level == AlertLevel.Info);

            var parts = new List<string>();
            if (critical > 0) parts.Add($"{critical} nghiem trong");
            if (warning > 0) parts.Add($"{warning} canh bao");
            if (info > 0) parts.Add($"{info} thong tin");

            return $"⚠️ {string.Join(", ", parts)}";
        }

        /// <summary>
        /// Get latest alert message
        /// </summary>
        public string GetLatestAlertMessage()
        {
            var latest = _activeAlerts.OrderByDescending(a => a.Timestamp).FirstOrDefault();
            return latest != null ? $"{latest.GetIcon()} {latest.Message}" : "";
        }
    }
}
