using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OpenCvSharp;

namespace TrafficMonitorApp
{
    public class AppConfig
    {
        public string ModelPath { get; set; } = "";
        public string VideoPath { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public double ConfidenceThreshold { get; set; } = 0.25;  // Lower default for better detection
        public double IouThreshold { get; set; } = 0.45;
        public int SkipFrames { get; set; } = 2;
        public int CameraIndex { get; set; } = 0;
        public string ExportPath { get; set; } = "";
        public bool ShowCoordinates { get; set; } = true;
        public List<Point> DetectionZone { get; set; } = new List<Point>();
        
        // Counting line settings (alternative to polygon zone)
        public bool UseCountingLine { get; set; } = false;
        public Point CountingLineStart { get; set; } = new Point(0, 0);
        public Point CountingLineEnd { get; set; } = new Point(0, 0);
        public int CountingLineThickness { get; set; } = 3;
        public int CrossingThreshold { get; set; } = 50; // Pixels from line to count (tang len cho xe bus/tai)
        
        // Vehicle refinement settings
        public bool EnableVehicleRefinement { get; set; } = true;
        public int SmallVehicleAreaThreshold { get; set; } = 6000;  // Decreased from 8000 for bicycle
        public int LargeVehicleAreaThreshold { get; set; } = 35000; // Increased from 30000 for motorcycle
        public double MinAspectRatio { get; set; } = 0.4;
        public double MaxAspectRatio { get; set; } = 3.0;
        
        // Line crossing settings
        public bool EnableLineCrossing { get; set; } = true;
        public int CrossingLinePosition { get; set; } = 50; // Percentage from top (0-100)
        
        // Advanced Settings
        public int MaxDetections { get; set; } = 100;
        public int FrameSkip { get; set; } = 0;
        public int TargetFPS { get; set; } = 30;
        public bool SaveDetectedFrames { get; set; } = false;
        
        // Alert Settings
        public bool EnableAlerts { get; set; } = false;
        public int AlertThreshold { get; set; } = 50;
        public bool EnableEmailAlerts { get; set; } = false;
        public string? EmailTo { get; set; }
        
        // Display Settings
        public bool ShowBoundingBoxes { get; set; } = true;
        public bool ShowConfidence { get; set; } = true;
        public bool ShowFPS { get; set; } = true;
        
        // GPU/CPU Settings - Execution Provider
        public string ExecutionProvider { get; set; } = "cuda"; // "cpu" or "cuda" or "tensorrt"
        public bool EnableGPU { get; set; } = true;

        private static readonly string ConfigFilePath = "traffic_config.json";

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
            }
            return new AppConfig();
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }

    public class VehicleType
    {
        public const string Car = "car";
        public const string Motorcycle = "motorcycle";
        public const string Bus = "bus";
        public const string Truck = "truck";
        public const string Bicycle = "bicycle";

        public static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            { Car, "O to" },
            { Motorcycle, "Xe may" },
            { Bus, "Xe buyt" },
            { Truck, "Xe tai" },
            { Bicycle, "Xe dap" }
        };

        public static readonly Dictionary<string, Scalar> Colors = new Dictionary<string, Scalar>
        {
            { Car, new Scalar(0, 255, 0) },      // Green
            { Motorcycle, new Scalar(255, 0, 0) }, // Blue
            { Bus, new Scalar(0, 255, 255) },     // Yellow
            { Truck, new Scalar(0, 165, 255) },   // Orange
            { Bicycle, new Scalar(255, 0, 255) }  // Magenta
        };
    }

    public class DetectionResult
    {
        public int TrackerId { get; set; }
        public string VehicleType { get; set; } = "";
        public float Confidence { get; set; }
        public Rect BoundingBox { get; set; }
        public Point Center { get; set; }
        public DateTime DetectedTime { get; set; }
    }

    public class TrafficStatistics
    {
        public Dictionary<string, int> VehicleCounts { get; set; } = new Dictionary<string, int>();
        public int TotalVehicles { get; set; }
        public int ProcessedFrames { get; set; }
        public double ProcessingTime { get; set; }
        public double AverageFPS { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<DetectionResult> DetailedResults { get; set; } = new List<DetectionResult>();
    }
}