using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace TrafficMonitorApp
{
    public class ReportExporter
    {
        public static void ExportToJson(TrafficStatistics statistics, string filePath, AppConfig config)
        {
            var report = new
            {
                Metadata = new
                {
                    ExportTime = DateTime.Now,
                    Software = "TrafficMonitorApp",
                    Version = "1.0"
                },
                SourceInfo = new
                {
                    ModelPath = config.ModelPath,
                    VideoPath = config.VideoPath,
                    ConfidenceThreshold = config.ConfidenceThreshold,
                    IouThreshold = config.IouThreshold
                },
                Results = new
                {
                    TotalVehicles = statistics.TotalVehicles,
                    ProcessedFrames = statistics.ProcessedFrames,
                    ProcessingTimeSeconds = Math.Round(statistics.ProcessingTime, 2),
                    AverageFPS = Math.Round(statistics.AverageFPS, 2),
                    StartTime = statistics.StartTime,
                    EndTime = statistics.EndTime
                },
                VehicleCounts = statistics.VehicleCounts.Select(kvp => new
                {
                    VehicleType = VehicleType.DisplayNames.ContainsKey(kvp.Key) 
                        ? VehicleType.DisplayNames[kvp.Key] 
                        : kvp.Key,
                    Count = kvp.Value,
                    Percentage = statistics.TotalVehicles > 0 
                        ? Math.Round((double)kvp.Value / statistics.TotalVehicles * 100, 1) 
                        : 0
                }),
                DetailedDetections = statistics.DetailedResults.Select(d => new
                {
                    TrackerId = d.TrackerId,
                    VehicleType = VehicleType.DisplayNames.ContainsKey(d.VehicleType) 
                        ? VehicleType.DisplayNames[d.VehicleType] 
                        : d.VehicleType,
                    Confidence = Math.Round(d.Confidence, 3),
                    Center = new { X = d.Center.X, Y = d.Center.Y },
                    BoundingBox = new 
                    { 
                        X = d.BoundingBox.X, 
                        Y = d.BoundingBox.Y, 
                        Width = d.BoundingBox.Width, 
                        Height = d.BoundingBox.Height 
                    },
                    DetectedTime = d.DetectedTime
                })
            };

            string json = JsonConvert.SerializeObject(report, Formatting.Indented);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public static void ExportToText(TrafficStatistics statistics, string filePath, AppConfig config)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("=".PadRight(70, '='));
            sb.AppendLine("           BAO CAO GIAM SAT GIAO THONG");
            sb.AppendLine("=".PadRight(70, '='));
            sb.AppendLine();
            sb.AppendLine($"Thoi gian: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            // Source info
            sb.AppendLine("THONG TIN NGUON:");
            sb.AppendLine("-".PadRight(70, '-'));
            sb.AppendLine($"Model: {Path.GetFileName(config.ModelPath)}");
            sb.AppendLine($"Video: {Path.GetFileName(config.VideoPath)}");
            sb.AppendLine($"Nguong tin cay: {config.ConfidenceThreshold:F2}");
            sb.AppendLine($"Nguong IOU: {config.IouThreshold:F2}");
            sb.AppendLine();
            
            // Statistics
            sb.AppendLine("THONG KE XU LY:");
            sb.AppendLine("-".PadRight(70, '-'));
            sb.AppendLine($"Tong so xe: {statistics.TotalVehicles}");
            sb.AppendLine($"Khung hinh da xu ly: {statistics.ProcessedFrames}");
            sb.AppendLine($"Thoi gian xu ly: {statistics.ProcessingTime:F2} giay");
            sb.AppendLine($"FPS trung binh: {statistics.AverageFPS:F2}");
            sb.AppendLine($"Bat dau: {statistics.StartTime:HH:mm:ss}");
            sb.AppendLine($"Ket thuc: {statistics.EndTime:HH:mm:ss}");
            sb.AppendLine();
            
            // Vehicle counts
            sb.AppendLine("PHAN LOAI PHUONG TIEN:");
            sb.AppendLine("-".PadRight(70, '-'));
            sb.AppendLine($"{"Loai xe",-20} {"So luong",10} {"Ty le %",10}");
            sb.AppendLine("-".PadRight(70, '-'));
            
            foreach (var kvp in statistics.VehicleCounts.OrderByDescending(x => x.Value))
            {
                string displayName = VehicleType.DisplayNames.ContainsKey(kvp.Key) 
                    ? VehicleType.DisplayNames[kvp.Key] 
                    : kvp.Key;
                double percentage = statistics.TotalVehicles > 0 
                    ? (double)kvp.Value / statistics.TotalVehicles * 100 
                    : 0;
                
                sb.AppendLine($"{displayName,-20} {kvp.Value,10} {percentage,9:F1}%");
            }
            
            sb.AppendLine();
            sb.AppendLine("=".PadRight(70, '='));
            
            // Detailed detections
            if (statistics.DetailedResults.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("CHI TIET PHAT HIEN:");
                sb.AppendLine("=".PadRight(70, '='));
                sb.AppendLine($"{"ID",-6} {"Loai xe",-15} {"Tin cay",-10} {"Toa do",-20} {"Thoi gian",-20}");
                sb.AppendLine("-".PadRight(70, '-'));
                
                foreach (var detection in statistics.DetailedResults.Take(100))
                {
                    string displayName = VehicleType.DisplayNames.ContainsKey(detection.VehicleType) 
                        ? VehicleType.DisplayNames[detection.VehicleType] 
                        : detection.VehicleType;
                    
                    sb.AppendLine($"{detection.TrackerId,-6} {displayName,-15} " +
                        $"{detection.Confidence,-10:F3} " +
                        $"({detection.Center.X},{detection.Center.Y})".PadRight(20) +
                        $"{detection.DetectedTime:HH:mm:ss}");
                }
                
                if (statistics.DetailedResults.Count > 100)
                {
                    sb.AppendLine($"... va {statistics.DetailedResults.Count - 100} phat hien khac");
                }
            }
            
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public static void ExportToExcel(TrafficStatistics statistics, string filePath, AppConfig config)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                // Summary sheet
                var summarySheet = package.Workbook.Worksheets.Add("Tong quan");
                
                summarySheet.Cells["A1"].Value = "BAO CAO GIAM SAT GIAO THONG";
                summarySheet.Cells["A1:D1"].Merge = true;
                summarySheet.Cells["A1"].Style.Font.Size = 16;
                summarySheet.Cells["A1"].Style.Font.Bold = true;
                summarySheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                int row = 3;
                summarySheet.Cells[$"A{row}"].Value = "Thoi gian xuat bao cao:";
                summarySheet.Cells[$"B{row}"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                row += 2;
                summarySheet.Cells[$"A{row}"].Value = "THONG KE TONG QUAT";
                summarySheet.Cells[$"A{row}"].Style.Font.Bold = true;
                
                row++;
                summarySheet.Cells[$"A{row}"].Value = "Tong so xe:";
                summarySheet.Cells[$"B{row}"].Value = statistics.TotalVehicles;
                
                row++;
                summarySheet.Cells[$"A{row}"].Value = "Khung hinh xu ly:";
                summarySheet.Cells[$"B{row}"].Value = statistics.ProcessedFrames;
                
                row++;
                summarySheet.Cells[$"A{row}"].Value = "Thoi gian xu ly (giay):";
                summarySheet.Cells[$"B{row}"].Value = Math.Round(statistics.ProcessingTime, 2);
                
                row++;
                summarySheet.Cells[$"A{row}"].Value = "FPS trung binh:";
                summarySheet.Cells[$"B{row}"].Value = Math.Round(statistics.AverageFPS, 2);
                
                // Vehicle counts sheet
                var countsSheet = package.Workbook.Worksheets.Add("Phan loai");
                
                countsSheet.Cells["A1"].Value = "Loai xe";
                countsSheet.Cells["B1"].Value = "So luong";
                countsSheet.Cells["C1"].Value = "Ty le %";
                countsSheet.Cells["A1:C1"].Style.Font.Bold = true;
                countsSheet.Cells["A1:C1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                countsSheet.Cells["A1:C1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                
                row = 2;
                foreach (var kvp in statistics.VehicleCounts.OrderByDescending(x => x.Value))
                {
                    string displayName = VehicleType.DisplayNames.ContainsKey(kvp.Key) 
                        ? VehicleType.DisplayNames[kvp.Key] 
                        : kvp.Key;
                    double percentage = statistics.TotalVehicles > 0 
                        ? (double)kvp.Value / statistics.TotalVehicles * 100 
                        : 0;
                    
                    countsSheet.Cells[$"A{row}"].Value = displayName;
                    countsSheet.Cells[$"B{row}"].Value = kvp.Value;
                    countsSheet.Cells[$"C{row}"].Value = percentage;
                    countsSheet.Cells[$"C{row}"].Style.Numberformat.Format = "0.00%";
                    row++;
                }
                
                countsSheet.Cells[countsSheet.Dimension.Address].AutoFitColumns();
                
                // Detections sheet
                if (statistics.DetailedResults.Count > 0)
                {
                    var detailsSheet = package.Workbook.Worksheets.Add("Chi tiet");
                    
                    detailsSheet.Cells["A1"].Value = "ID";
                    detailsSheet.Cells["B1"].Value = "Loai xe";
                    detailsSheet.Cells["C1"].Value = "Tin cay";
                    detailsSheet.Cells["D1"].Value = "Toa do X";
                    detailsSheet.Cells["E1"].Value = "Toa do Y";
                    detailsSheet.Cells["F1"].Value = "Thoi gian";
                    detailsSheet.Cells["A1:F1"].Style.Font.Bold = true;
                    detailsSheet.Cells["A1:F1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    detailsSheet.Cells["A1:F1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                    
                    row = 2;
                    foreach (var detection in statistics.DetailedResults)
                    {
                        string displayName = VehicleType.DisplayNames.ContainsKey(detection.VehicleType) 
                            ? VehicleType.DisplayNames[detection.VehicleType] 
                            : detection.VehicleType;
                        
                        detailsSheet.Cells[$"A{row}"].Value = detection.TrackerId;
                        detailsSheet.Cells[$"B{row}"].Value = displayName;
                        detailsSheet.Cells[$"C{row}"].Value = detection.Confidence;
                        detailsSheet.Cells[$"D{row}"].Value = detection.Center.X;
                        detailsSheet.Cells[$"E{row}"].Value = detection.Center.Y;
                        detailsSheet.Cells[$"F{row}"].Value = detection.DetectedTime.ToString("HH:mm:ss.fff");
                        row++;
                    }
                    
                    detailsSheet.Cells[detailsSheet.Dimension.Address].AutoFitColumns();
                }
                
                package.SaveAs(new FileInfo(filePath));
            }
        }
    }
}