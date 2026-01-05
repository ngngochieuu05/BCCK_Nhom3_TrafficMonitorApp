using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrafficMonitorApp.Models;

namespace TrafficMonitorApp.Data
{
    /// <summary>
    /// Repository pattern for database operations
    /// Provides high-level methods for CRUD operations and queries
    /// </summary>
    public class TrafficRepository : IDisposable
    {
        private readonly TrafficDbContext _context;
        private bool _disposed = false;

        public TrafficRepository(TrafficDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region TrafficSession Operations

        /// <summary>
        /// Create a new traffic monitoring session
        /// </summary>
        public async Task<TrafficSessionDb> CreateSessionAsync(TrafficSessionDb session)
        {
            try
            {
                await _context.TrafficSessions.AddAsync(session);
                await _context.SaveChangesAsync();
                return session;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create session: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update an existing session (usually when monitoring completes)
        /// </summary>
        public async Task<TrafficSessionDb> UpdateSessionAsync(TrafficSessionDb session)
        {
            try
            {
                _context.TrafficSessions.Update(session);
                await _context.SaveChangesAsync();
                return session;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update session: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get session by ID with all detections
        /// </summary>
        public async Task<TrafficSessionDb?> GetSessionByIdAsync(int sessionId, bool includeDetections = false)
        {
            try
            {
                var query = _context.TrafficSessions.AsQueryable();

                if (includeDetections)
                {
                    query = query.Include(s => s.Detections);
                }

                return await query.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get session: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get recent sessions with pagination
        /// </summary>
        public async Task<List<TrafficSessionDb>> GetRecentSessionsAsync(int count = 10, int skip = 0)
        {
            try
            {
                return await _context.TrafficSessions
                    .OrderByDescending(s => s.StartTime)
                    .Skip(skip)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get recent sessions: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get sessions within a date range
        /// </summary>
        public async Task<List<TrafficSessionDb>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.TrafficSessions
                    .Where(s => s.StartTime >= startDate && s.StartTime <= endDate)
                    .OrderByDescending(s => s.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get sessions by date range: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete a session (will cascade delete all detections)
        /// </summary>
        public async Task<bool> DeleteSessionAsync(int sessionId)
        {
            try
            {
                var session = await _context.TrafficSessions.FindAsync(sessionId);
                if (session == null)
                    return false;

                _context.TrafficSessions.Remove(session);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete session: {ex.Message}", ex);
            }
        }

        #endregion

        #region VehicleDetection Operations

        /// <summary>
        /// Add a single vehicle detection
        /// </summary>
        public async Task<VehicleDetectionDb> AddDetectionAsync(VehicleDetectionDb detection)
        {
            try
            {
                await _context.VehicleDetections.AddAsync(detection);
                await _context.SaveChangesAsync();
                return detection;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add detection: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Add multiple vehicle detections in batch (more efficient)
        /// </summary>
        public async Task AddDetectionsBatchAsync(List<VehicleDetectionDb> detections)
        {
            try
            {
                await _context.VehicleDetections.AddRangeAsync(detections);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add detections batch: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get all detections for a specific session
        /// </summary>
        public async Task<List<VehicleDetectionDb>> GetDetectionsBySessionAsync(int sessionId)
        {
            try
            {
                return await _context.VehicleDetections
                    .Where(d => d.SessionId == sessionId)
                    .OrderBy(d => d.DetectedTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get detections: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get detections by vehicle type
        /// </summary>
        public async Task<List<VehicleDetectionDb>> GetDetectionsByTypeAsync(int sessionId, string vehicleType)
        {
            try
            {
                return await _context.VehicleDetections
                    .Where(d => d.SessionId == sessionId && d.VehicleType == vehicleType)
                    .OrderBy(d => d.DetectedTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get detections by type: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get detection count by vehicle type for a session
        /// </summary>
        public async Task<Dictionary<string, int>> GetDetectionCountByTypeAsync(int sessionId)
        {
            try
            {
                return await _context.VehicleDetections
                    .Where(d => d.SessionId == sessionId)
                    .GroupBy(d => d.VehicleType)
                    .Select(g => new { VehicleType = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.VehicleType, x => x.Count);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get detection count: {ex.Message}", ex);
            }
        }

        #endregion

        #region HourlyStatistics Operations

        /// <summary>
        /// Add or update hourly statistics
        /// </summary>
        public async Task<HourlyStatisticsDb> UpsertHourlyStatisticsAsync(HourlyStatisticsDb statistics)
        {
            try
            {
                // Check if statistics for this hour already exists
                var existing = await _context.HourlyStatistics
                    .FirstOrDefaultAsync(s => s.HourTimestamp == statistics.HourTimestamp);

                if (existing != null)
                {
                    // Update existing
                    existing.TotalVehicles = statistics.TotalVehicles;
                    existing.CarCount = statistics.CarCount;
                    existing.MotorcycleCount = statistics.MotorcycleCount;
                    existing.BusCount = statistics.BusCount;
                    existing.BicycleCount = statistics.BicycleCount;
                    existing.AverageSpeed = statistics.AverageSpeed;
                    existing.CongestionLevel = statistics.CongestionLevel;
                    
                    _context.HourlyStatistics.Update(existing);
                }
                else
                {
                    // Add new
                    await _context.HourlyStatistics.AddAsync(statistics);
                }

                await _context.SaveChangesAsync();
                return existing ?? statistics;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upsert hourly statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get hourly statistics for a date range
        /// </summary>
        public async Task<List<HourlyStatisticsDb>> GetHourlyStatisticsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.HourlyStatistics
                    .Where(s => s.HourTimestamp >= startDate && s.HourTimestamp <= endDate)
                    .OrderBy(s => s.HourTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get hourly statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get hourly statistics for today
        /// </summary>
        public async Task<List<HourlyStatisticsDb>> GetTodayStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                
                return await _context.HourlyStatistics
                    .Where(s => s.HourTimestamp >= today && s.HourTimestamp < tomorrow)
                    .OrderBy(s => s.HourTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get today's statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get peak traffic hour for a date range
        /// </summary>
        public async Task<HourlyStatisticsDb?> GetPeakTrafficHourAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.HourlyStatistics
                    .Where(s => s.HourTimestamp >= startDate && s.HourTimestamp <= endDate)
                    .OrderByDescending(s => s.TotalVehicles)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get peak traffic hour: {ex.Message}", ex);
            }
        }

        #endregion

        #region Statistics & Reporting

        /// <summary>
        /// Get total vehicle count across all sessions
        /// </summary>
        public async Task<int> GetTotalVehicleCountAsync()
        {
            try
            {
                return await _context.TrafficSessions.SumAsync(s => s.TotalVehicles);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get total vehicle count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get average FPS across all sessions
        /// </summary>
        public async Task<double> GetAverageFPSAsync()
        {
            try
            {
                var sessions = await _context.TrafficSessions.ToListAsync();
                return sessions.Any() ? sessions.Average(s => s.AverageFPS) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get average FPS: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get total processing time across all sessions
        /// </summary>
        public async Task<double> GetTotalProcessingTimeAsync()
        {
            try
            {
                return await _context.TrafficSessions.SumAsync(s => s.ProcessingTime);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get total processing time: {ex.Message}", ex);
            }
        }

        #endregion

        #region Dispose Pattern

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
