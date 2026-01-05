using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;

namespace TrafficMonitorApp
{
    public class VideoProcessor
    {
        private VehicleDetector? _detector;
        private VideoCapture? _capture;
        private CancellationTokenSource? _cts;
        private bool _isPaused;
        private bool _isProcessing;
        private readonly HashSet<int> _countedVehicles = new HashSet<int>();
        private readonly Dictionary<string, int> _vehicleTypeCounts = new Dictionary<string, int>();
        private readonly List<DetectionResult> _allDetections = new List<DetectionResult>();
        private readonly object _lockObject = new object();
        private bool _noCountWarningShown = false;
        private DateTime _processingStartTime = DateTime.MinValue;
        private AppConfig _config;
        private StreamWriter? _logWriter;
        // Reserved for future FPS control implementation
        #pragma warning disable CS0414
        private int _targetFPS = 30;
        #pragma warning restore CS0414
        private int _delayMs = 33; // Default ~30 FPS

        // Advanced systems
        private readonly AdvancedStatistics _advancedStats = new AdvancedStatistics();

        public event EventHandler<Mat>? FrameProcessed;
        public event EventHandler<TrafficStatistics>? StatisticsUpdated;
        public event EventHandler? ProcessingCompleted;
        public event EventHandler<DetectionResult>? VehicleDetected;
        public event EventHandler<DetectionResult>? VehicleExited; // New event for parking exit
        
        // Public access to advanced features
        public AdvancedStatistics AdvancedStats => _advancedStats;
        
        public TrafficStatistics Statistics { get; private set; } = new TrafficStatistics();
        public bool IsProcessing => _isProcessing;
        public bool IsPaused => _isPaused;
        
        public VideoProcessor()
        {
            _config = AppConfig.Load();
        }

        public async Task StartProcessingAsync(string source, VehicleDetector detector, 
            List<Point>? detectionZone = null, int skipFrames = 2, bool isCamera = false)
        {
            if (IsProcessing)
                throw new InvalidOperationException("Processing is already running");
                
            // Reload config to get latest settings
            _config = AppConfig.Load();

            // Create log file
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"traffic_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            _logWriter = new StreamWriter(logPath, true) { AutoFlush = true };
            
            var startMsg = "═══════════════════════════════════════════════════════════";
            Console.WriteLine(startMsg);
            if (_config.UseCountingLine)
            {
                var lineMsgs = new[] {
                    "[VideoProcessor] Mode: LINE ZONE (Full screen detection, count on line crossing)",
                    $"[VideoProcessor] Counting line: ({_config.CountingLineStart.X},{_config.CountingLineStart.Y}) -> ({_config.CountingLineEnd.X},{_config.CountingLineEnd.Y})",
                    $"[VideoProcessor] Threshold: {_config.CrossingThreshold}px"
                };
                foreach (var msg in lineMsgs)
                {
                    Console.WriteLine(msg);
                    _logWriter?.WriteLine(msg);
                }
            }
            else
            {
                var zoneMsgs = new[] {
                    "[VideoProcessor] Mode: BIG ZONE (Restricted area detection)",
                    $"[VideoProcessor] Detection zone: {(detectionZone?.Count ?? 0)} points"
                };
                foreach (var msg in zoneMsgs)
                {
                    Console.WriteLine(msg);
                    _logWriter?.WriteLine(msg);
                }
            }
            
            var endMsg = "═══════════════════════════════════════════════════════════";
            Console.WriteLine(endMsg);
            _logWriter?.WriteLine(endMsg);

            _detector = detector;
            _cts = new CancellationTokenSource();
            _isProcessing = true;  // Set immediately so IsProcessing returns true
            
            lock (_lockObject)
            {
                _countedVehicles.Clear();
                _vehicleTypeCounts.Clear();
                _allDetections.Clear();
            }
            
            _isPaused = false;
            _noCountWarningShown = false;
            _processingStartTime = DateTime.Now;

            Statistics = new TrafficStatistics
            {
                StartTime = DateTime.Now
            };

            await Task.Run(() => ProcessVideo(source, detectionZone, skipFrames, isCamera, _cts.Token));
        }

        public void Pause()
        {
            _isPaused = true;
            Console.WriteLine("[VideoProcessor] Paused");
        }

        public void Resume()
        {
            _isPaused = false;
            Console.WriteLine("[VideoProcessor] Resumed");
        }

        public void Stop()
        {
            Console.WriteLine("[VideoProcessor] Stopping...");
            _isProcessing = false;
            _cts?.Cancel();
            _capture?.Release();
            _capture?.Dispose();
            _capture = null;
        }

        private void ProcessVideo(string source, List<Point>? detectionZone, int skipFrames, 
            bool isCamera, CancellationToken token)
        {
            Mat? currentFrame = null;
            
            try
            {
                // Open video source
                if (isCamera)
                {
                    _capture = new VideoCapture(int.Parse(source));
                }
                else
                {
                    _capture = new VideoCapture(source);
                }

                if (!_capture.IsOpened())
                {
                    throw new Exception("Cannot open video source");
                }

                double sourceFps = _capture.Fps;
                if (sourceFps <= 0 || sourceFps > 120)
                {
                    sourceFps = isCamera ? 30 : 25;
                }

                Console.WriteLine($"[VideoProcessor] Video opened: FPS={sourceFps}");

                int frameDelay = (int)(1000.0 / sourceFps);
                int frameCount = 0;
                int processedFrames = 0;
                
                var stopwatch = Stopwatch.StartNew();
                var fpsTimer = Stopwatch.StartNew();
                int fpsFrameCount = 0;
                double currentFps = 0;

                currentFrame = new Mat();

                while (!token.IsCancellationRequested)
                {
                    while (_isPaused && !token.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }

                    if (token.IsCancellationRequested)
                        break;

                    var frameReadStart = Stopwatch.StartNew();
                    bool ret = _capture.Read(currentFrame);
                    
                    if (!ret || currentFrame.Empty())
                    {
                        if (!isCamera)
                        {
                            Console.WriteLine("[VideoProcessor] Video ended");
                            break;
                        }
                        Thread.Sleep(10);
                        continue;
                    }

                    bool shouldProcess = (frameCount % (skipFrames + 1)) == 0;
                    
                    if (shouldProcess)
                    {
                        Console.WriteLine($"[VideoProcessor] ══════ Processing frame {frameCount} ══════");
                        // Line Zone mode: scan full screen (null zone), count only on line crossing
                        // Big Zone mode: scan only within zone, count when inside zone
                        var actualZone = _config.UseCountingLine ? null : detectionZone;
                        ProcessFrame(currentFrame, actualZone);
                        processedFrames++;
                        fpsFrameCount++;
                    }

                    frameCount++;

                    if (fpsTimer.ElapsedMilliseconds >= 1000)
                    {
                        currentFps = fpsFrameCount / (fpsTimer.ElapsedMilliseconds / 1000.0);
                        fpsTimer.Restart();
                        fpsFrameCount = 0;
                        
                        UpdateStatistics(processedFrames, stopwatch.Elapsed.TotalSeconds, currentFps);
                        
                        // Check if no vehicles counted after 10 seconds
                        CheckNoCountWarning();
                    }

                    frameReadStart.Stop();
                    int processingTime = (int)frameReadStart.ElapsedMilliseconds;
                    
                    // Use configured target FPS delay
                    int targetDelay = _delayMs > 0 ? _delayMs : frameDelay;
                    int sleepTime = Math.Max(1, targetDelay - processingTime);
                    
                    if (!shouldProcess && skipFrames > 0)
                    {
                        sleepTime = sleepTime / (skipFrames + 1);
                    }
                    
                    Thread.Sleep(sleepTime);
                }

                stopwatch.Stop();

                lock (_lockObject)
                {
                    Statistics.ProcessedFrames = processedFrames;
                    Statistics.ProcessingTime = stopwatch.Elapsed.TotalSeconds;
                    Statistics.AverageFPS = processedFrames / Math.Max(0.001, Statistics.ProcessingTime);
                    Statistics.EndTime = DateTime.Now;
                    Statistics.DetailedResults = new List<DetectionResult>(_allDetections);
                }

                Console.WriteLine("[VideoProcessor] ═══════════════════════════════════════");
                Console.WriteLine($"[VideoProcessor] Processing complete!");
                Console.WriteLine($"[VideoProcessor] Total frames: {frameCount}");
                Console.WriteLine($"[VideoProcessor] Processed: {processedFrames}");
                Console.WriteLine($"[VideoProcessor] Total vehicles: {_countedVehicles.Count}");
                Console.WriteLine("[VideoProcessor] ═══════════════════════════════════════");

                StatisticsUpdated?.Invoke(this, Statistics);
                ProcessingCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VideoProcessor] ✗ ERROR: {ex.Message}");
                Console.WriteLine($"[VideoProcessor] Stack: {ex.StackTrace}");
            }
            finally
            {
                _isProcessing = false;  // Always clear flag when done
                currentFrame?.Dispose();
                _capture?.Release();
                _capture?.Dispose();
                _capture = null;
            }
        }

        private void ProcessFrame(Mat frame, List<Point>? detectionZone)
        {
            if (_detector == null || frame.Empty())
                return;

            try
            {
                var detections = _detector.Detect(frame, detectionZone);

                lock (_lockObject)
                {
                    foreach (var detection in detections)
                    {
                        // Check if this vehicle has already been counted
                        if (!_detector.IsVehicleCounted(detection.TrackerId))
                        {
                            bool shouldCount = false;
                            string countingMethod = "NONE";
                            
                            // LINE ZONE MODE: Scan full screen, count only on line crossing
                            if (_config.UseCountingLine && 
                                _config.CountingLineStart.X != 0 && 
                                _config.CountingLineEnd.X != 0)
                            {
                                shouldCount = _detector.CheckLineCrossing(
                                    detection.TrackerId,
                                    _config.CountingLineStart,
                                    _config.CountingLineEnd,
                                    _config.CrossingThreshold
                                );
                                countingMethod = "LINE_CROSSING";
                                
                                // Debug log for line crossing detection
                                if (shouldCount)
                                {
                                    var msg = $"[VideoProcessor] ✓ Line crossing detected for ID={detection.TrackerId}, DetectedType={detection.VehicleType}";
                                    Console.WriteLine(msg);
                                    _logWriter?.WriteLine(msg);
                                }
                            }
                            // BIG ZONE MODE: Scan only in zone, count when first detected (already filtered by detection zone)
                            else if (detectionZone != null && detectionZone.Count >= 3)
                            {
                                // Vehicle is already in detection zone (filtered by VehicleDetector.Detect)
                                // Count it once when first seen with this tracker ID
                                shouldCount = true;
                                countingMethod = "ZONE_DETECTION";
                            }
                            
                            if (shouldCount)
                            {
                                // Get the current (possibly updated) vehicle type
                                string finalType = _detector.GetVehicleFinalType(detection.TrackerId);
                                
                                var countMsg1 = $"[VideoProcessor] 🎯 COUNTING Vehicle: ID={detection.TrackerId}, DetectedType={detection.VehicleType}, FinalType={finalType}, Method={countingMethod}";
                                var countMsg2 = $"[VideoProcessor]   └─> Before count: Total={_countedVehicles.Count}, Bus={(_vehicleTypeCounts.ContainsKey(VehicleType.Bus) ? _vehicleTypeCounts[VehicleType.Bus] : 0)}";
                                Console.WriteLine(countMsg1);
                                Console.WriteLine(countMsg2);
                                _logWriter?.WriteLine(countMsg1);
                                _logWriter?.WriteLine(countMsg2);
                                
                                // Mark as counted to prevent duplicate counting
                                _detector.MarkVehicleAsCounted(detection.TrackerId);
                                _countedVehicles.Add(detection.TrackerId);
                                
                                if (!_vehicleTypeCounts.ContainsKey(finalType))
                                    _vehicleTypeCounts[finalType] = 0;
                                _vehicleTypeCounts[finalType]++;
                                
                                var afterMsg = $"[VideoProcessor]   └─> After count: Total={_countedVehicles.Count}, Bus={(_vehicleTypeCounts.ContainsKey(VehicleType.Bus) ? _vehicleTypeCounts[VehicleType.Bus] : 0)}";
                                Console.WriteLine(afterMsg);
                                _logWriter?.WriteLine(afterMsg);
                                
                                // Store detection with final type
                                var finalDetection = new DetectionResult
                                {
                                    TrackerId = detection.TrackerId,
                                    VehicleType = finalType,
                                    Confidence = detection.Confidence,
                                    BoundingBox = detection.BoundingBox,
                                    Center = detection.Center,
                                    DetectedTime = DateTime.Now
                                };
                                _allDetections.Add(finalDetection);
                                
                                var successMsg = $"[VideoProcessor] ✓✓ COUNTED: ID={detection.TrackerId}, Type={finalType}, Method={countingMethod}";
                                Console.WriteLine(successMsg);
                                _logWriter?.WriteLine(successMsg);
                                
                                // Fire VehicleDetected event for realtime database update
                                VehicleDetected?.Invoke(this, finalDetection);
                                
                                // Fire VehicleExited event if using line crossing mode (for parking management)
                                if (countingMethod == "LINE_CROSSING")
                                {
                                    VehicleExited?.Invoke(this, finalDetection);
                                }
                                
                                // Update statistics immediately
                                Statistics.TotalVehicles = _countedVehicles.Count;
                                Statistics.VehicleCounts = new Dictionary<string, int>(_vehicleTypeCounts);
                                StatisticsUpdated?.Invoke(this, Statistics);
                            }
                        }
                    }
                }

                Mat annotatedFrame = AnnotateFrame(frame.Clone(), detections, detectionZone);
                FrameProcessed?.Invoke(this, annotatedFrame);
            }
            catch (Exception ex)
            {
                var errMsg = $"[ProcessFrame] ✗ ERROR: {ex.Message}";
                Console.WriteLine(errMsg);
                _logWriter?.WriteLine(errMsg);
            }
        }

        private Mat AnnotateFrame(Mat frame, List<DetectionResult> detections, List<Point>? detectionZone)
        {
            try
            {
                // Draw counting line if enabled
                if (_config.UseCountingLine && 
                    _config.CountingLineStart.X != 0 && 
                    _config.CountingLineEnd.X != 0)
                {
                    Point lineStart = _config.CountingLineStart;
                    Point lineEnd = _config.CountingLineEnd;
                    
                    // Draw main counting line (thick, bright)
                    Cv2.Line(frame, lineStart, lineEnd, new Scalar(0, 0, 255), _config.CountingLineThickness * 2);
                    
                    // Draw threshold zone (semi-transparent)
                    double angle = Math.Atan2(lineEnd.Y - lineStart.Y, lineEnd.X - lineStart.X);
                    double perpAngle = angle + Math.PI / 2;
                    int threshold = _config.CrossingThreshold;
                    
                    Point offset = new Point(
                        (int)(Math.Cos(perpAngle) * threshold),
                        (int)(Math.Sin(perpAngle) * threshold)
                    );
                    
                    Point[] zonePoints = new Point[]
                    {
                        new Point(lineStart.X + offset.X, lineStart.Y + offset.Y),
                        new Point(lineEnd.X + offset.X, lineEnd.Y + offset.Y),
                        new Point(lineEnd.X - offset.X, lineEnd.Y - offset.Y),
                        new Point(lineStart.X - offset.X, lineStart.Y - offset.Y)
                    };
                    
                    Mat overlay = frame.Clone();
                    Cv2.FillPoly(overlay, new[] { zonePoints }, new Scalar(0, 0, 255, 80));
                    Cv2.AddWeighted(frame, 0.8, overlay, 0.2, 0, frame);
                    overlay.Dispose();
                    
                    // Draw label
                    Cv2.PutText(frame, "COUNTING LINE", 
                        new Point(lineStart.X + 10, lineStart.Y - 10),
                        HersheyFonts.HersheySimplex, 0.9, new Scalar(0, 0, 255), 2);
                }
                // Draw detection zone if not using counting line
                else if (detectionZone != null && detectionZone.Count >= 3)
                {
                    var points = detectionZone.ToArray();
                    
                    // Draw filled semi-transparent zone
                    Mat overlay = frame.Clone();
                    Cv2.FillPoly(overlay, new[] { points }, new Scalar(0, 255, 255, 50));
                    Cv2.AddWeighted(frame, 0.7, overlay, 0.3, 0, frame);
                    overlay.Dispose();
                    
                    // Draw zone boundary (thicker)
                    Cv2.Polylines(frame, new[] { points }, true, new Scalar(0, 255, 255), 4);
                    
                    // Draw zone label
                    Cv2.PutText(frame, "DETECTION ZONE", 
                        new Point(points[0].X + 10, points[0].Y - 10),
                        HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 255, 255), 2);
                }

                // Draw each detection
                foreach (var detection in detections)
                {
                    var color = VehicleType.Colors.ContainsKey(detection.VehicleType) 
                        ? VehicleType.Colors[detection.VehicleType] 
                        : new Scalar(255, 255, 255);

                    // Draw bounding box (thicker)
                    Cv2.Rectangle(frame, detection.BoundingBox, color, 3);

                    // Draw center point
                    Cv2.Circle(frame, detection.Center, 5, color, -1);
                    Cv2.Circle(frame, detection.Center, 7, new Scalar(255, 255, 255), 2);

                    // Prepare label with ALL information
                    string vehicleName = VehicleType.DisplayNames.ContainsKey(detection.VehicleType)
                        ? VehicleType.DisplayNames[detection.VehicleType]
                        : detection.VehicleType;
                    
                    // Format: "ID:X Ô tô 87.5%" - Simplified for better visibility
                    string label = $"ID:{detection.TrackerId} {vehicleName} {detection.Confidence * 100:F1}%";
                    
                    // Calculate label size for background (larger font)
                    int baseLine;
                    var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheySimplex, 0.8, 2, out baseLine);
                    
                    // Position label above box
                    int labelX = detection.BoundingBox.X;
                    int labelY = detection.BoundingBox.Y - 10;
                    
                    // If too close to top, put it below
                    if (labelY < textSize.Height + 10)
                    {
                        labelY = detection.BoundingBox.Y + detection.BoundingBox.Height + textSize.Height + 10;
                    }

                    // Draw label background (semi-transparent)
                    var labelRect = new Rect(
                        labelX - 2,
                        labelY - textSize.Height - 5,
                        textSize.Width + 4,
                        textSize.Height + baseLine + 10
                    );

                    // Ensure label is within frame
                    labelRect.X = Math.Max(0, Math.Min(labelRect.X, frame.Width - labelRect.Width));
                    labelRect.Y = Math.Max(0, Math.Min(labelRect.Y, frame.Height - labelRect.Height));

                    // Draw solid background
                    Cv2.Rectangle(frame, labelRect, color, -1);
                    
                    // Draw text (larger font: 0.8 instead of 0.6)
                    Cv2.PutText(frame, label, 
                        new Point(labelRect.X + 2, labelRect.Y + textSize.Height + 2),
                        HersheyFonts.HersheySimplex, 0.8, new Scalar(255, 255, 255), 2);

                    // Draw additional info: Bounding box dimensions
                    string sizeInfo = $"{detection.BoundingBox.Width}x{detection.BoundingBox.Height}px";
                    Cv2.PutText(frame, sizeInfo,
                        new Point(detection.BoundingBox.X, detection.BoundingBox.Y + detection.BoundingBox.Height + 20),
                        HersheyFonts.HersheySimplex, 0.5, color, 1);
                }

                // Draw statistics overlay
                DrawStatisticsOverlay(frame);

                return frame;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnnotateFrame] ✗ ERROR: {ex.Message}");
                return frame;
            }
        }

        private void DrawStatisticsOverlay(Mat frame)
        {
            try
            {
                int totalCount;
                Dictionary<string, int> counts;
                
                lock (_lockObject)
                {
                    totalCount = _countedVehicles.Count;
                    counts = new Dictionary<string, int>(_vehicleTypeCounts);
                }

                // Semi-transparent background
                Mat overlay = frame.Clone();
                int overlayHeight = 50 + (Math.Max(1, counts.Count) * 35) + 20;
                Cv2.Rectangle(overlay, new Point(10, 10), new Point(420, overlayHeight), 
                    new Scalar(0, 0, 0), -1);
                Cv2.AddWeighted(frame, 0.65, overlay, 0.35, 0, frame);
                overlay.Dispose();

                int y = 40;
                
                // Title
                Cv2.PutText(frame, "=== TRAFFIC STATISTICS ===", 
                    new Point(20, y), HersheyFonts.HersheySimplex, 0.7, 
                    new Scalar(0, 255, 255), 2);
                
                y += 35;
                
                // Total count with larger font
                Cv2.PutText(frame, $"TOTAL VEHICLES: {totalCount}", 
                    new Point(20, y), HersheyFonts.HersheySimplex, 0.8, 
                    new Scalar(0, 255, 255), 2);
                
                y += 40;
                
                // Vehicle type breakdown
                if (counts.Count > 0)
                {
                    foreach (var kvp in counts.OrderByDescending(x => x.Value))
                    {
                        string displayName = VehicleType.DisplayNames.ContainsKey(kvp.Key) 
                            ? VehicleType.DisplayNames[kvp.Key] 
                            : kvp.Key;
                        
                        var color = VehicleType.Colors.ContainsKey(kvp.Key) 
                            ? VehicleType.Colors[kvp.Key] 
                            : new Scalar(255, 255, 255);

                        // Draw colored box indicator
                        Cv2.Rectangle(frame, 
                            new Point(20, y - 18), 
                            new Point(40, y - 3),
                            color, -1);
                        
                        Cv2.Rectangle(frame, 
                            new Point(20, y - 18), 
                            new Point(40, y - 3),
                            new Scalar(255, 255, 255), 1);

                        // Calculate percentage
                        double percentage = totalCount > 0 ? (kvp.Value * 100.0 / totalCount) : 0;
                        
                        string text = $"{displayName}: {kvp.Value} ({percentage:F1}%)";
                        Cv2.PutText(frame, text, 
                            new Point(50, y), HersheyFonts.HersheySimplex, 0.65, 
                            new Scalar(255, 255, 255), 2);
                        y += 35;
                    }
                }
                else
                {
                    Cv2.PutText(frame, "No vehicles detected yet...", 
                        new Point(20, y), HersheyFonts.HersheySimplex, 0.6, 
                        new Scalar(200, 200, 200), 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawOverlay] ✗ ERROR: {ex.Message}");
            }
        }

        private void CheckNoCountWarning()
        {
            // Only show warning once, after 10 seconds, if no vehicles counted
            if (!_noCountWarningShown && 
                _countedVehicles.Count == 0 && 
                (DateTime.Now - _processingStartTime).TotalSeconds >= 10)
            {
                _noCountWarningShown = true;
                Console.WriteLine("\n" + new string('═', 80));
                Console.WriteLine("⚠️  WARNING: No vehicles counted after 10 seconds!");
                Console.WriteLine(new string('═', 80));
                Console.WriteLine("Possible reasons:");
                Console.WriteLine("  1. Detection zone not covering vehicle paths");
                Console.WriteLine("     → Draw zone where vehicles actually pass");
                Console.WriteLine("  2. Zone set but vehicles outside");
                Console.WriteLine("     → Redraw zone to include vehicle areas");
                Console.WriteLine("  3. Want to count all vehicles?");
                Console.WriteLine("     → DON'T draw zone (skip 'Select Zone' step)");
                Console.WriteLine("     → System will auto-count all detected vehicles");
                Console.WriteLine(new string('═', 80) + "\n");
            }
        }

        private void UpdateStatistics(int processedFrames, double elapsedSeconds, double currentFps)
        {
            lock (_lockObject)
            {
                Statistics.TotalVehicles = _countedVehicles.Count;
                Statistics.VehicleCounts = new Dictionary<string, int>(_vehicleTypeCounts);
                Statistics.ProcessedFrames = processedFrames;
                Statistics.ProcessingTime = elapsedSeconds;
                Statistics.AverageFPS = currentFps > 0 ? currentFps : (processedFrames / Math.Max(0.001, elapsedSeconds));
            }

            StatisticsUpdated?.Invoke(this, Statistics);
        }

        public Mat? ProcessSingleImage(string imagePath, List<Point>? detectionZone = null)
        {
            if (_detector == null)
                throw new InvalidOperationException("Detector not initialized");

            Console.WriteLine($"[ProcessImage] Loading: {imagePath}");

            using (Mat frame = Cv2.ImRead(imagePath))
            {
                if (frame.Empty())
                {
                    Console.WriteLine("[ProcessImage] ✗ Cannot read image");
                    return null;
                }

                Console.WriteLine($"[ProcessImage] Image size: {frame.Width}x{frame.Height}");

                var detections = _detector.Detect(frame, detectionZone);

                lock (_lockObject)
                {
                    _countedVehicles.Clear();
                    _vehicleTypeCounts.Clear();
                    _allDetections.Clear();

                    foreach (var detection in detections)
                    {
                        _countedVehicles.Add(detection.TrackerId);
                        
                        if (!_vehicleTypeCounts.ContainsKey(detection.VehicleType))
                        {
                            _vehicleTypeCounts[detection.VehicleType] = 0;
                        }
                        _vehicleTypeCounts[detection.VehicleType]++;
                        
                        _allDetections.Add(detection);
                    }

                    Statistics = new TrafficStatistics
                    {
                        TotalVehicles = _countedVehicles.Count,
                        VehicleCounts = new Dictionary<string, int>(_vehicleTypeCounts),
                        ProcessedFrames = 1,
                        DetailedResults = new List<DetectionResult>(_allDetections),
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now,
                        ProcessingTime = 0,
                        AverageFPS = 0
                    };
                }

                Console.WriteLine($"[ProcessImage] ✓ Found {detections.Count} vehicles");

                return AnnotateFrame(frame.Clone(), detections, detectionZone);
            }
        }

        public void Dispose()
        {
            Stop();
            _detector?.Dispose();
        }
    }
}