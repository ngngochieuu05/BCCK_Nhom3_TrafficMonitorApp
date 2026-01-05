using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenCvSharp;

namespace TrafficMonitorApp
{
    /// <summary>
    /// Optimizes frame processing with adaptive FPS, motion detection, and buffering
    /// </summary>
    public class FrameOptimizer
    {
        private readonly Queue<Mat> _frameBuffer = new Queue<Mat>();
        private Mat? _previousFrame;
        private readonly Stopwatch _fpsTimer = new Stopwatch();
        private readonly Queue<double> _fpsHistory = new Queue<double>();
        private int _frameCounter = 0;
        private double _currentFPS = 0;

        // Configuration
        public int MaxBufferSize { get; set; } = 5;
        public double MotionThreshold { get; set; } = 2000; // Pixels changed threshold
        public bool EnableMotionDetection { get; set; } = true;
        public bool EnableAdaptiveFPS { get; set; } = true;
        public int MinSkipFrames { get; set; } = 1;
        public int MaxSkipFrames { get; set; } = 5;

        // Statistics
        public double CurrentFPS => _currentFPS;
        public int CurrentSkipFrames { get; private set; } = 2;
        public int BufferedFrames => _frameBuffer.Count;
        public double AverageFPS => _fpsHistory.Count > 0 ? _fpsHistory.Average() : 0;

        public FrameOptimizer()
        {
            _fpsTimer.Start();
        }

        /// <summary>
        /// Check if frame has significant motion compared to previous frame
        /// </summary>
        public bool HasSignificantMotion(Mat currentFrame)
        {
            if (!EnableMotionDetection || _previousFrame == null)
            {
                _previousFrame?.Dispose();
                _previousFrame = currentFrame.Clone();
                return true;
            }

            try
            {
                // Convert to grayscale
                using var gray1 = new Mat();
                using var gray2 = new Mat();
                Cv2.CvtColor(_previousFrame, gray1, ColorConversionCodes.BGR2GRAY);
                Cv2.CvtColor(currentFrame, gray2, ColorConversionCodes.BGR2GRAY);

                // Calculate difference
                using var diff = new Mat();
                Cv2.Absdiff(gray1, gray2, diff);

                // Threshold
                using var thresh = new Mat();
                Cv2.Threshold(diff, thresh, 25, 255, ThresholdTypes.Binary);

                // Count non-zero pixels
                int changedPixels = Cv2.CountNonZero(thresh);

                // Update previous frame
                _previousFrame?.Dispose();
                _previousFrame = currentFrame.Clone();

                bool hasMotion = changedPixels > MotionThreshold;
                
                if (!hasMotion)
                {
                    Console.WriteLine($"[FrameOptimizer] No motion detected ({changedPixels} pixels changed)");
                }

                return hasMotion;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FrameOptimizer] Motion detection error: {ex.Message}");
                return true; // Process frame on error
            }
        }

        /// <summary>
        /// Update FPS counter and calculate adaptive skip frames
        /// </summary>
        public void UpdateFPS()
        {
            _frameCounter++;

            if (_fpsTimer.ElapsedMilliseconds >= 1000)
            {
                _currentFPS = _frameCounter * 1000.0 / _fpsTimer.ElapsedMilliseconds;
                
                // Track FPS history
                _fpsHistory.Enqueue(_currentFPS);
                if (_fpsHistory.Count > 10)
                    _fpsHistory.Dequeue();

                _frameCounter = 0;
                _fpsTimer.Restart();

                // Adaptive FPS adjustment
                if (EnableAdaptiveFPS)
                {
                    AdjustSkipFrames();
                }
            }
        }

        /// <summary>
        /// Dynamically adjust frame skip based on system performance
        /// </summary>
        private void AdjustSkipFrames()
        {
            // If FPS is low, skip more frames
            if (_currentFPS < 15)
            {
                CurrentSkipFrames = Math.Min(CurrentSkipFrames + 1, MaxSkipFrames);
                Console.WriteLine($"[FrameOptimizer] ⚠ Low FPS ({_currentFPS:F1}), increasing skip to {CurrentSkipFrames}");
            }
            // If FPS is high, process more frames
            else if (_currentFPS > 25 && CurrentSkipFrames > MinSkipFrames)
            {
                CurrentSkipFrames = Math.Max(CurrentSkipFrames - 1, MinSkipFrames);
                Console.WriteLine($"[FrameOptimizer] ✓ Good FPS ({_currentFPS:F1}), decreasing skip to {CurrentSkipFrames}");
            }
        }

        /// <summary>
        /// Add frame to buffer for processing queue
        /// </summary>
        public void BufferFrame(Mat frame)
        {
            if (_frameBuffer.Count >= MaxBufferSize)
            {
                // Remove oldest frame
                var oldFrame = _frameBuffer.Dequeue();
                oldFrame?.Dispose();
            }

            _frameBuffer.Enqueue(frame.Clone());
        }

        /// <summary>
        /// Get next frame from buffer
        /// </summary>
        public Mat? GetBufferedFrame()
        {
            return _frameBuffer.Count > 0 ? _frameBuffer.Dequeue() : null;
        }

        /// <summary>
        /// Clear frame buffer
        /// </summary>
        public void ClearBuffer()
        {
            while (_frameBuffer.Count > 0)
            {
                var frame = _frameBuffer.Dequeue();
                frame?.Dispose();
            }
        }

        /// <summary>
        /// Optimize frame for faster processing (resize if too large)
        /// </summary>
        public Mat OptimizeFrame(Mat frame, int maxWidth = 1920, int maxHeight = 1080)
        {
            if (frame.Width <= maxWidth && frame.Height <= maxHeight)
                return frame;

            // Calculate scale to fit within max dimensions
            double scaleX = (double)maxWidth / frame.Width;
            double scaleY = (double)maxHeight / frame.Height;
            double scale = Math.Min(scaleX, scaleY);

            int newWidth = (int)(frame.Width * scale);
            int newHeight = (int)(frame.Height * scale);

            var resized = new Mat();
            Cv2.Resize(frame, resized, new Size(newWidth, newHeight), 0, 0, InterpolationFlags.Linear);

            Console.WriteLine($"[FrameOptimizer] Resized frame from {frame.Width}x{frame.Height} to {newWidth}x{newHeight}");

            return resized;
        }

        /// <summary>
        /// Get optimization statistics
        /// </summary>
        public string GetStatistics()
        {
            return $"FPS: {_currentFPS:F1} | Avg: {AverageFPS:F1} | Skip: {CurrentSkipFrames} | Buffer: {BufferedFrames}/{MaxBufferSize}";
        }

        public void Dispose()
        {
            _previousFrame?.Dispose();
            ClearBuffer();
        }
    }
}
