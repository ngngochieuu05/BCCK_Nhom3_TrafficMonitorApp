using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace TrafficMonitorApp
{
    public class VehicleDetector
    {
        private Net? _net;
        private InferenceSession? _onnxSession;
        private readonly float _confThreshold;
        private readonly float _nmsThreshold;
        private readonly Dictionary<int, TrackerInfo> _trackers = new Dictionary<int, TrackerInfo>();
        private int _nextTrackerId = 1;
        private Size _inputSize = new Size(640, 640);
        private string[] _classNames = new string[0];

        private class TrackerInfo
        {
            public int Id { get; set; }
            public Point LastCenter { get; set; }
            public Rect LastBoundingBox { get; set; }
            public string VehicleType { get; set; } = "";
            public string InitialVehicleType { get; set; } = "";  // Type when first detected
            public int FramesLost { get; set; }
            public int FramesSeen { get; set; }
            public DateTime LastSeen { get; set; }
            public float LastConfidence { get; set; }
            public int TypeChanges { get; set; } = 0;  // Count how many times type changed
            public bool IsCounted { get; set; } = false;  // Already counted in statistics
            public bool HasCrossedLine { get; set; } = false;  // For line crossing detection
            public int LinePosition { get; set; } = 0;  // -1: before line, 0: on line, 1: after line
        }

        public VehicleDetector(string modelPath, double confThreshold = 0.15, double nmsThreshold = 0.45)
        {
            _confThreshold = (float)confThreshold;
            _nmsThreshold = (float)nmsThreshold;

            try
            {
                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.WriteLine($"[VehicleDetector] Initializing...");
                Console.WriteLine($"[VehicleDetector] Model path: {modelPath}");
                Console.WriteLine($"[VehicleDetector] Confidence threshold: {confThreshold}");
                Console.WriteLine($"[VehicleDetector] NMS threshold: {nmsThreshold}");
                
                // Load YOLO model
                if (modelPath.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        _net = CvDnn.ReadNetFromOnnx(modelPath);
                        Console.WriteLine("[VehicleDetector] ✓ Loaded ONNX model via OpenCV DNN");
                    }
                    catch (Exception exOpencv)
                    {
                        // Some ONNX ops are not supported by OpenCV DNN. Fall back to ONNX Runtime.
                        Console.WriteLine($"[VehicleDetector] ⚠ OpenCV failed to load ONNX: {exOpencv.Message}");
                        try
                        {
                            _onnxSession = new InferenceSession(modelPath);
                            _net = null;
                            Console.WriteLine("[VehicleDetector] ✓ Loaded ONNX model via ONNX Runtime fallback");
                            
                            // Print input/output info
                            foreach (var input in _onnxSession.InputMetadata)
                            {
                                Console.WriteLine($"[VehicleDetector] Input: {input.Key}, Shape: [{string.Join(",", input.Value.Dimensions)}]");
                            }
                            foreach (var output in _onnxSession.OutputMetadata)
                            {
                                Console.WriteLine($"[VehicleDetector] Output: {output.Key}, Shape: [{string.Join(",", output.Value.Dimensions)}]");
                            }
                        }
                        catch (Exception exOnnx)
                        {
                            throw new Exception($"Failed to load ONNX model with both OpenCV and ONNX Runtime: {exOnnx.Message}");
                        }
                    }
                }
                else if (modelPath.EndsWith(".pt", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("PyTorch .pt files not supported. Export to ONNX first!");
                }
                else
                {
                    string cfgPath = modelPath.Replace(".weights", ".cfg");
                    _net = CvDnn.ReadNetFromDarknet(cfgPath, modelPath);
                    Console.WriteLine("[VehicleDetector] ✓ Loaded Darknet model");
                }

                if (_net == null && _onnxSession == null)
                {
                    throw new Exception("Failed to load model: neither OpenCV DNN nor ONNX Runtime succeeded");
                }

                if (_net != null && _net.Empty())
                {
                    throw new Exception("Network loaded but is empty");
                }

                // Set backend: attempt CUDA and fall back to CPU if not available (only if using OpenCV DNN)
                if (_net != null)
                {
                    try
                    {
                        _net.SetPreferableBackend(Backend.CUDA);
                        _net.SetPreferableTarget(Target.CUDA);
                        Console.WriteLine("[VehicleDetector] ✓ Using CUDA GPU acceleration");
                    }
                    catch (Exception)
                    {
                        _net.SetPreferableBackend(Backend.OPENCV);
                        _net.SetPreferableTarget(Target.CPU);
                        Console.WriteLine("[VehicleDetector] ✓ Using CPU (consider using GPU for better performance)");
                    }
                }

                if (_net != null)
                {
                    // Get output layer info
                    var outNames = _net.GetUnconnectedOutLayersNames();
                    Console.WriteLine($"[VehicleDetector] Output layers: {string.Join(", ", outNames)}");
                }
                
                Console.WriteLine("[VehicleDetector] ✓ Initialization complete!");
                Console.WriteLine("═══════════════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehicleDetector] ✗ ERROR: {ex.Message}");
                throw;
            }
        }

        public List<DetectionResult> Detect(Mat frame, List<Point>? detectionZone = null)
        {
            if ((_net == null && _onnxSession == null) || frame.Empty())
            {
                Console.WriteLine("[Detect] ✗ No model loaded or frame is empty");
                return new List<DetectionResult>();
            }

            var results = new List<DetectionResult>();

            try
            {
                int origWidth = frame.Width;
                int origHeight = frame.Height;
                // If ONNX Runtime was used to load the model, run inference here via ONNX Runtime
                if (_onnxSession != null)
                {
                    Console.WriteLine($"[Detect][ONNX] Processing frame: {origWidth}x{origHeight}");
                    Console.WriteLine($"[Detect][ONNX] Confidence threshold: {_confThreshold}");

                    // Preprocess: resize to _inputSize, BGR->RGB, normalize
                    var resized = new Mat();
                    Cv2.Resize(frame, resized, _inputSize, 0, 0, InterpolationFlags.Linear);
                    Cv2.CvtColor(resized, resized, ColorConversionCodes.BGR2RGB);
                    var floatMat = new Mat();
                    resized.ConvertTo(floatMat, MatType.CV_32F, 1.0 / 255.0);

                    int targetW = _inputSize.Width;
                    int targetH = _inputSize.Height;
                    int channels = 3;
                    var tensor = new DenseTensor<float>(new[] { 1, channels, targetH, targetW });
                    
                    // Convert HWC to CHW format (YOLOv8 expects channels-first)
                    for (int c = 0; c < channels; c++)
                        for (int y = 0; y < targetH; y++)
                            for (int x = 0; x < targetW; x++)
                                tensor[0, c, y, x] = floatMat.At<Vec3f>(y, x)[c];

                    resized.Dispose();
                    floatMat.Dispose();

                    var inputName = _onnxSession.InputMetadata.Keys.First();
                    Console.WriteLine($"[Detect][ONNX] Input name: {inputName}");
                    
                    var onnxInputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, tensor) };
                    using var onnxResults = _onnxSession.Run(onnxInputs);

                    var first = onnxResults.First();
                    var outTensor = first.AsTensor<float>();
                    var dims = outTensor.Dimensions.ToArray();
                    Console.WriteLine($"[Detect][ONNX] Output dims: [{string.Join(',', dims)}]");
                    var outArr = outTensor.ToArray();

                    if (dims.Length == 3)
                    {
                        int dim1 = dims[1];  // Should be 84 for YOLOv8 (4 + 80 classes) or your num classes
                        int dim2 = dims[2];  // Should be 8400 for 640x640 input
                        int numClasses = dim1 - 4;  // Subtract bbox coordinates
                        
                        Console.WriteLine($"[Detect][ONNX] Detected {numClasses} classes, {dim2} predictions");
                        
                        var onnxRawDetections = new List<RawDetection>();
                        int detectionCount = 0;

                        for (int i = 0; i < dim2; i++)
                        {
                            // YOLOv8 format: [cx, cy, w, h, class1_score, class2_score, ...]
                            float cx = outArr[0 * (dim1 * dim2) + 0 * dim2 + i];
                            float cy = outArr[0 * (dim1 * dim2) + 1 * dim2 + i];
                            float w = outArr[0 * (dim1 * dim2) + 2 * dim2 + i];
                            float h = outArr[0 * (dim1 * dim2) + 3 * dim2 + i];

                            // Find max class score
                            float maxScore = 0f; 
                            int maxClassId = -1;
                            for (int c = 0; c < numClasses; c++)
                            {
                                float s = outArr[0 * (dim1 * dim2) + (4 + c) * dim2 + i];
                                if (s > maxScore) 
                                { 
                                    maxScore = s; 
                                    maxClassId = c; 
                                }
                            }

                            // Debug: Print first few detections with high confidence
                            if (detectionCount < 10 && maxScore > 0.01f)
                            {
                                Console.WriteLine($"[Detect][ONNX] Detection {i}: cx={cx:F2}, cy={cy:F2}, w={w:F2}, h={h:F2}, class={maxClassId}, score={maxScore:F3}");
                                detectionCount++;
                            }

                            if (maxScore < _confThreshold) continue;

                            // Convert coordinates from normalized [0,1] to absolute pixels
                            float x, y, widthF, heightF;
                            
                            // YOLOv8 outputs are in normalized format [0, 1] relative to input size
                            // Scale back to original image size
                            float scaleX = (float)origWidth / targetW;
                            float scaleY = (float)origHeight / targetH;
                            
                            x = (cx - w / 2f) * scaleX;
                            y = (cy - h / 2f) * scaleY;
                            widthF = w * scaleX;
                            heightF = h * scaleY;

                            // Clamp to valid range
                            x = Math.Max(0, Math.Min(x, origWidth - 1));
                            y = Math.Max(0, Math.Min(y, origHeight - 1));
                            widthF = Math.Max(1, Math.Min(widthF, origWidth - x));
                            heightF = Math.Max(1, Math.Min(heightF, origHeight - y));

                            onnxRawDetections.Add(new RawDetection 
                            { 
                                Box = new Rect2d(x, y, widthF, heightF), 
                                Confidence = maxScore, 
                                ClassId = maxClassId 
                            });
                        }

                        Console.WriteLine($"[Detect][ONNX] Parsed {onnxRawDetections.Count} raw detections before NMS (threshold: {_confThreshold})");

                        if (onnxRawDetections.Count == 0)
                        {
                            Console.WriteLine($"[Detect][ONNX] ⚠ WARNING: No detections found!");
                            Console.WriteLine($"[Detect][ONNX] Try lowering confidence threshold (current: {_confThreshold})");
                            return results;
                        }

                        // Apply NMS
                        var onnxBoxes = onnxRawDetections.Select(d => d.Box).ToList();
                        var onnxConfs = onnxRawDetections.Select(d => d.Confidence).ToList();
                        CvDnn.NMSBoxes(onnxBoxes, onnxConfs, _confThreshold, _nmsThreshold, out int[] onnxIndices);
                        Console.WriteLine($"[Detect][ONNX] After NMS: {onnxIndices.Length} detections");

                        foreach (int idx in onnxIndices)
                        {
                            var detection = onnxRawDetections[idx];
                            var rect = new Rect((int)detection.Box.X, (int)detection.Box.Y, (int)detection.Box.Width, (int)detection.Box.Height);
                            var center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                            
                            // Check if detection is inside zone (if zone is set)
                            if (detectionZone != null && detectionZone.Count >= 3)
                            {
                                if (!IsPointInPolygon(center, detectionZone))
                                {
                                    Console.WriteLine($"[Detect][ONNX] Detection at ({center.X},{center.Y}) outside zone, skipping");
                                    continue;
                                }
                            }
                            
                            // Get vehicle type first
                            string vehicleType = GetVehicleTypeFromClassId(detection.ClassId);
                            
                            if (vehicleType == "unknown") 
                            {
                                Console.WriteLine($"[Detect][ONNX] ⚠ Skipping unknown class {detection.ClassId}");
                                continue;
                            }
                            
                            // Refine vehicle type using size/aspect ratio
                            vehicleType = RefineVehicleType(vehicleType, rect, detection.Confidence);
                            
                            // Get tracker ID with improved matching
                            int trackerId = GetOrCreateTrackerId(center, rect, vehicleType, detection.Confidence);
                            
                            results.Add(new DetectionResult 
                            { 
                                TrackerId = trackerId, 
                                VehicleType = vehicleType, 
                                Confidence = detection.Confidence, 
                                BoundingBox = rect, 
                                Center = center, 
                                DetectedTime = DateTime.Now 
                            });
                            
                            Console.WriteLine($"[Detect][ONNX] ✓ ID={trackerId}, Type={vehicleType}, Conf={detection.Confidence:F3} ({detection.Confidence*100:F1}%), Pos=({center.X},{center.Y})");
                        }

                        CleanupOldTrackers();
                        Console.WriteLine($"[Detect][ONNX] ═══ Returning {results.Count} final detections ═══");
                        return results;
                    }
                    else
                    {
                        Console.WriteLine($"[Detect][ONNX] ✗ ERROR: Unexpected output dimensions: {dims.Length}");
                        return results;
                    }
                }

                Console.WriteLine($"[Detect] Processing frame: {origWidth}x{origHeight}");

                // Create input blob
                Mat blob = CvDnn.BlobFromImage(
                    frame,
                    1.0 / 255.0,           // Scale to [0,1]
                    _inputSize,
                    new Scalar(0, 0, 0),   // No mean subtraction
                    swapRB: true,          // BGR to RGB
                    crop: false            // No cropping
                );

                Console.WriteLine($"[Detect] Blob created: {blob.Size(0)}x{blob.Size(1)}x{blob.Size(2)}x{blob.Size(3)}");

                _net?.SetInput(blob);

                // Forward pass
                var outNames = _net?.GetUnconnectedOutLayersNames();
                var outs = outNames?.Select(_ => new Mat()).ToArray() ?? Array.Empty<Mat>();
                
                Console.WriteLine($"[Detect] Running inference...");
                // outNames may be annotated nullable; assert non-null here because GetUnconnectedOutLayersNames() returns an array when network is valid
                _net?.Forward(outs, outNames!);
                Console.WriteLine($"[Detect] ✓ Inference complete, got {outs.Length} output(s)");

                // Parse outputs
                var rawDetections = new List<RawDetection>();
                
                foreach (var output in outs)
                {
                    var dims = Enumerable.Range(0, output.Dims).Select(i => output.Size(i)).ToArray();
                    Console.WriteLine($"[Detect] Output shape: [{string.Join("x", dims)}]");
                    
                    var parsed = ParseYoloOutput(output, origWidth, origHeight);
                    rawDetections.AddRange(parsed);
                }

                Console.WriteLine($"[Detect] Parsed {rawDetections.Count} raw detections before NMS");

                if (rawDetections.Count == 0)
                {
                    Console.WriteLine("[Detect] ⚠ No detections found! Try:");
                    Console.WriteLine("  - Lower confidence threshold (current: {0})", _confThreshold);
                    Console.WriteLine("  - Check if frame contains target objects");
                    Console.WriteLine("  - Verify model is trained properly");
                }

                // Apply NMS
                var boxes = rawDetections.Select(d => d.Box).ToList();
                var confidences = rawDetections.Select(d => d.Confidence).ToList();
                var classIds = rawDetections.Select(d => d.ClassId).ToList();

                CvDnn.NMSBoxes(boxes, confidences, _confThreshold, _nmsThreshold, out int[] indices);
                
                Console.WriteLine($"[Detect] After NMS: {indices.Length} detections");

                // Process filtered detections
                foreach (int idx in indices)
                {
                    var detection = rawDetections[idx];
                    
                    var rect = new Rect(
                        (int)detection.Box.X,
                        (int)detection.Box.Y,
                        (int)detection.Box.Width,
                        (int)detection.Box.Height
                    );
                    
                    // Ensure box is within frame bounds
                    rect.X = Math.Max(0, Math.Min(rect.X, origWidth - 1));
                    rect.Y = Math.Max(0, Math.Min(rect.Y, origHeight - 1));
                    rect.Width = Math.Min(rect.Width, origWidth - rect.X);
                    rect.Height = Math.Min(rect.Height, origHeight - rect.Y);
                    
                    var center = new Point(
                        rect.X + rect.Width / 2,
                        rect.Y + rect.Height / 2
                    );
                    
                    // Check if detection is inside zone (if zone is set)
                    if (detectionZone != null && detectionZone.Count >= 3)
                    {
                        if (!IsPointInPolygon(center, detectionZone))
                        {
                            Console.WriteLine($"[Detect][DNN] Detection at ({center.X},{center.Y}) outside zone, skipping");
                            continue;
                        }
                    }
                    
                    // Get vehicle type first
                    string vehicleType = GetVehicleTypeFromClassId(detection.ClassId);
                    
                    if (vehicleType == "unknown")
                    {
                        Console.WriteLine($"[Detect] ⚠ Unknown class ID: {detection.ClassId}");
                        Console.WriteLine($"[Detect] Update GetVehicleTypeFromClassId() to map this class!");
                        continue;
                    }
                    
                    // Refine vehicle type using size/aspect ratio
                    vehicleType = RefineVehicleType(vehicleType, rect, detection.Confidence);
                    
                    // Get tracker ID with improved matching
                    int trackerId = GetOrCreateTrackerId(center, rect, vehicleType, detection.Confidence);

                    var result = new DetectionResult
                    {
                        TrackerId = trackerId,
                        VehicleType = vehicleType,
                        Confidence = detection.Confidence,
                        BoundingBox = rect,
                        Center = center,
                        DetectedTime = DateTime.Now
                    };

                    results.Add(result);

                    Console.WriteLine($"[Detect] ✓ ID={trackerId}, Type={vehicleType}, " +
                                    $"Conf={detection.Confidence:F3} ({detection.Confidence * 100:F1}%), " +
                                    $"Pos=({center.X},{center.Y}), " +
                                    $"Box=[{rect.X},{rect.Y},{rect.Width}x{rect.Height}]");
                }

                CleanupOldTrackers();

                // Cleanup
                blob.Dispose();
                foreach (var mat in outs)
                    mat.Dispose();

                Console.WriteLine($"[Detect] ═══ Returning {results.Count} final detections ═══");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Detect] ✗ ERROR: {ex.Message}");
                Console.WriteLine($"[Detect] Stack: {ex.StackTrace}");
            }

            return results;
        }

        private class RawDetection
        {
            public Rect2d Box { get; set; }
            public float Confidence { get; set; }
            public int ClassId { get; set; }
        }

        private List<RawDetection> ParseYoloOutput(Mat output, int frameWidth, int frameHeight)
        {
            var detections = new List<RawDetection>();

            try
            {
                Console.WriteLine($"[ParseYolo] Input dimensions: {output.Dims}");
                
                if (output.Dims == 3)
                {
                    int batch = output.Size(0);
                    int dim1 = output.Size(1);
                    int dim2 = output.Size(2);
                    
                    Console.WriteLine($"[ParseYolo] Shape: [{batch}, {dim1}, {dim2}]");

                    // YOLOv8: [1, 84, 8400] - classes first
                    // YOLOv5: [1, 25200, 85] - detections first
                    bool isYoloV8Format = dim1 < 100;

                    Mat processedOutput = output;
                    
                    if (isYoloV8Format)
                    {
                        Console.WriteLine("[ParseYolo] Detected YOLOv8 format, transposing...");
                        // Reshape to 2D then transpose
                        processedOutput = output.Reshape(1, new int[] { dim1, dim2 });
                        processedOutput = processedOutput.T();
                        Console.WriteLine($"[ParseYolo] After transpose: {processedOutput.Rows}x{processedOutput.Cols}");
                    }
                    else
                    {
                        Console.WriteLine("[ParseYolo] Detected YOLOv5 format");
                        processedOutput = output.Reshape(1, new int[] { dim1, dim2 });
                    }

                    int numDetections = processedOutput.Rows;
                    int numData = processedOutput.Cols;
                    int numClasses = numData - 4; // x,y,w,h + classes

                    Console.WriteLine($"[ParseYolo] Processing {numDetections} detections with {numClasses} classes");

                    for (int i = 0; i < numDetections; i++)
                    {
                        // Extract data
                        float cx = processedOutput.At<float>(i, 0);
                        float cy = processedOutput.At<float>(i, 1);
                        float w = processedOutput.At<float>(i, 2);
                        float h = processedOutput.At<float>(i, 3);

                        // Find max class score
                        float maxScore = 0f;
                        int maxClassId = -1;

                        for (int c = 4; c < numData; c++)
                        {
                            float score = processedOutput.At<float>(i, c);
                            if (score > maxScore)
                            {
                                maxScore = score;
                                maxClassId = c - 4;
                            }
                        }

                        // Check confidence threshold
                        if (maxScore < _confThreshold)
                            continue;

                        // Convert coordinates
                        // YOLOv8 uses normalized coords, YOLOv5 might use absolute
                        float x, y, width, height;
                        
                        if (cx <= 1.0f && cy <= 1.0f) // Normalized [0,1]
                        {
                            x = (cx - w / 2f) * frameWidth;
                            y = (cy - h / 2f) * frameHeight;
                            width = w * frameWidth;
                            height = h * frameHeight;
                        }
                        else // Absolute coordinates
                        {
                            x = cx - w / 2f;
                            y = cy - h / 2f;
                            width = w;
                            height = h;
                        }

                        // Clamp to valid range
                        x = Math.Max(0, Math.Min(x, frameWidth));
                        y = Math.Max(0, Math.Min(y, frameHeight));
                        width = Math.Max(1, Math.Min(width, frameWidth - x));
                        height = Math.Max(1, Math.Min(height, frameHeight - y));

                        detections.Add(new RawDetection
                        {
                            Box = new Rect2d(x, y, width, height),
                            Confidence = maxScore,
                            ClassId = maxClassId
                        });
                    }

                    if (isYoloV8Format && processedOutput != output)
                    {
                        processedOutput.Dispose();
                    }
                }
                else if (output.Dims == 2)
                {
                    Console.WriteLine("[ParseYolo] 2D format (legacy)");
                    
                    int numDetections = output.Rows;
                    int numData = output.Cols;

                    for (int i = 0; i < numDetections; i++)
                    {
                        float cx = output.At<float>(i, 0);
                        float cy = output.At<float>(i, 1);
                        float w = output.At<float>(i, 2);
                        float h = output.At<float>(i, 3);
                        float objectness = output.At<float>(i, 4);

                        if (objectness < _confThreshold)
                            continue;

                        float maxScore = 0f;
                        int maxClassId = -1;
                        
                        for (int c = 5; c < numData; c++)
                        {
                            float score = output.At<float>(i, c) * objectness;
                            if (score > maxScore)
                            {
                                maxScore = score;
                                maxClassId = c - 5;
                            }
                        }

                        if (maxScore < _confThreshold)
                            continue;

                        float x = (cx - w / 2f) * frameWidth;
                        float y = (cy - h / 2f) * frameHeight;
                        float width = w * frameWidth;
                        float height = h * frameHeight;

                        detections.Add(new RawDetection
                        {
                            Box = new Rect2d(x, y, width, height),
                            Confidence = maxScore,
                            ClassId = maxClassId
                        });
                    }
                }

                Console.WriteLine($"[ParseYolo] Found {detections.Count} detections above threshold");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ParseYolo] ✗ ERROR: {ex.Message}");
            }

            return detections;
        }

        private string GetVehicleTypeFromClassId(int classId)
        {
            // Based on data.yaml: names: ['bicycle', 'bus', 'car', 'license-plate', 'motorcycle']
            // Class mapping for your custom trained model:
            // 0: bicycle
            // 1: bus
            // 2: car
            // 3: license-plate (not a vehicle, skip)
            // 4: motorcycle
            
            var mapped = classId switch
            {
                0 => VehicleType.Bicycle,     // bicycle
                1 => VehicleType.Bus,          // bus
                2 => VehicleType.Car,          // car
                3 => "unknown",                 // license-plate (skip)
                4 => VehicleType.Motorcycle,   // motorcycle
                _ => "unknown"
            };

            if (mapped != "unknown")
            {
                Console.WriteLine($"[ClassMapping] ✓ Class {classId} -> {mapped}");
            }
            else
            {
                Console.WriteLine($"[ClassMapping] ⚠ Class {classId} not mapped (license-plate or invalid)");
            }

            return mapped;
        }

        /// <summary>
        /// Refine vehicle type using size, aspect ratio, and confidence
        /// Helps distinguish motorcycles from bicycles
        /// </summary>
        private string RefineVehicleType(string detectedType, Rect boundingBox, float confidence)
        {
            int width = boundingBox.Width;
            int height = boundingBox.Height;
            int area = width * height;
            double aspectRatio = (double)width / height;

            // Size thresholds for camera at 3-5m distance (urban road)
            const int BICYCLE_MAX_AREA = 2000;      // Xe đạp: < 2000px
            const int MOTORCYCLE_MIN_AREA = 1200;   // Xe máy: giảm từ 1900 để bắt dễ hơn
            const int MOTORCYCLE_MAX_AREA = 10000;  // Tăng lên để không bỏ sót
            const int CAR_MIN_AREA = 9000;          // Giảm từ 13000 để bắt xa hơn
            const int CAR_MAX_AREA = 25000;         // Tăng lên cho linh hoạt
            const int BUS_MIN_AREA = 18000;         // Giảm từ 20000 để bắt bus nhỏ hơn
            
            const double BICYCLE_MIN_ASPECT = 0.1;    // Xe đạp: hẹp
            const double BICYCLE_MAX_ASPECT = 0.5;    // Tăng lên
            const double MOTORCYCLE_MIN_ASPECT = 0.2; // Giảm xuống
            const double MOTORCYCLE_MAX_ASPECT = 1.2; // Tăng lên để linh hoạt
            const double CAR_MIN_ASPECT = 0.4;        // Giảm xuống
            const double CAR_MAX_ASPECT = 2.5;        // Tăng lên
            const double BUS_MIN_ASPECT = 1.5;        // Giảm từ 2.0

            string refinedType = detectedType;
            string reason = "original";

            if ((detectedType == VehicleType.Bicycle || detectedType == VehicleType.Motorcycle))
            {
                if (area > BUS_MIN_AREA && aspectRatio > BUS_MIN_ASPECT)
                {
                    refinedType = VehicleType.Bus;
                    reason = "large_size_wide_aspect";
                }
                else if (area > CAR_MIN_AREA && aspectRatio > CAR_MIN_ASPECT)
                {
                    refinedType = VehicleType.Car;
                    reason = "car_size_range";
                }
            }
            
            // ===== PRIORITY 2: Fix Car misclassified as bicycle/motorcycle =====
            
            if (detectedType == VehicleType.Car)
            {
                // Car too small -> likely motorcycle or bicycle
                if (area < MOTORCYCLE_MAX_AREA && aspectRatio < CAR_MAX_ASPECT)
                {
                    if (area < BICYCLE_MAX_AREA && aspectRatio < BICYCLE_MAX_ASPECT)
                    {
                        refinedType = VehicleType.Bicycle;
                        reason = "too_small_for_car_bicycle";
                    }
                    else
                    {
                        refinedType = VehicleType.Motorcycle;
                        reason = "too_small_for_car_motorcycle";
                    }
                }
                // Car too large -> likely bus
                else if (area > BUS_MIN_AREA && aspectRatio > BUS_MIN_ASPECT)
                {
                    refinedType = VehicleType.Bus;
                    reason = "too_large_for_car";
                }
            }
            
            // ===== PRIORITY 3: Fix Bus misclassified as car =====
            
            if (detectedType == VehicleType.Bus)
            {
                // Bus too small -> likely car
                if (area < CAR_MAX_AREA)
                {
                    if (area > CAR_MIN_AREA && aspectRatio > CAR_MIN_ASPECT)
                    {
                        refinedType = VehicleType.Car;
                        reason = "too_small_for_bus";
                    }
                    else if (area < MOTORCYCLE_MAX_AREA)
                    {
                        refinedType = VehicleType.Motorcycle;
                        reason = "bus_actually_motorcycle";
                    }
                }
            }
            
            // ===== PRIORITY 4: Refine bicycle vs motorcycle =====
            
            if (detectedType == VehicleType.Bicycle || detectedType == VehicleType.Motorcycle)
            {
                // Very small + narrow -> bicycle
                if (area < BICYCLE_MAX_AREA && aspectRatio >= BICYCLE_MIN_ASPECT && aspectRatio < BICYCLE_MAX_ASPECT && confidence < 0.7)
                {
                    refinedType = VehicleType.Bicycle;
                    reason = "small_narrow_bicycle";
                }
                // Medium size -> motorcycle
                else if (area >= MOTORCYCLE_MIN_AREA && area <= MOTORCYCLE_MAX_AREA && 
                         aspectRatio >= MOTORCYCLE_MIN_ASPECT && aspectRatio <= MOTORCYCLE_MAX_ASPECT)
                {
                    refinedType = VehicleType.Motorcycle;
                    reason = "medium_size_motorcycle";
                }
            }
            
            // ===== PRIORITY 5: High confidence preservation =====
            
            // If high confidence, only override with strong size/aspect mismatch
            if (confidence > 0.75)
            {
                // Keep original unless size is drastically wrong
                if (detectedType == VehicleType.Car && area < MOTORCYCLE_MIN_AREA)
                {
                    refinedType = VehicleType.Bicycle;
                    reason = "high_conf_but_tiny";
                }
                else if (detectedType == VehicleType.Bicycle && area > CAR_MIN_AREA)
                {
                    refinedType = VehicleType.Car;
                    reason = "high_conf_but_huge";
                }
                else if (confidence > 0.85)
                {
                    refinedType = detectedType; // Trust very high confidence
                    reason = "very_high_confidence";
                }
            }

            if (refinedType != detectedType)
            {
                Console.WriteLine($"[Refinement] 🔄 Changed {detectedType} -> {refinedType} (reason: {reason}, area={area}, ratio={aspectRatio:F2}, conf={confidence:F2})");
            }
            else
            {
                Console.WriteLine($"[Refinement] ✓ Kept {detectedType} (area={area}, ratio={aspectRatio:F2}, conf={confidence:F2})");
            }

            return refinedType;
        }

        private int GetOrCreateTrackerId(Point center, Rect boundingBox, string vehicleType, float confidence)
        {
            const int maxDistance = 200;  // Increased from 150 for fast-moving vehicles
            const double minIoU = 0.25;   // Decreased from 0.3 for better matching
            const double highIoU = 0.65;  // Slightly lower for flexible matching
            
            int closestId = -1;
            double minDistance = double.MaxValue;
            double bestIoU = 0;

            // First pass: Look for exact matches (same vehicle, possibly type changed)
            foreach (var tracker in _trackers.Values.ToList())
            {
                double iou = CalculateIoU(boundingBox, tracker.LastBoundingBox);
                double dist = Math.Sqrt(
                    Math.Pow(center.X - tracker.LastCenter.X, 2) + 
                    Math.Pow(center.Y - tracker.LastCenter.Y, 2)
                );

                // High IoU means this is likely the SAME vehicle
                if (iou > highIoU && dist < maxDistance)
                {
                    // Check if vehicle type changed (bus/car confusion)
                    if (tracker.VehicleType != vehicleType)
                    {
                        Console.WriteLine($"[Tracker] 🔄 Type change detected ID={tracker.Id}: {tracker.VehicleType} -> {vehicleType} (IoU={iou:F2}, Frames={tracker.FramesSeen})");
                        
                        // Update to more reliable type based on:
                        // 1. Car -> Bus upgrade (vehicle entering zone, now see full body)
                        // 2. Higher confidence
                        // 3. Initial frames (still refining)
                        bool shouldUpdate = false;
                        
                        // PRIORITY: Allow Car->Bus upgrade in early frames (common issue)
                        if (tracker.VehicleType == VehicleType.Car && vehicleType == VehicleType.Bus && tracker.FramesSeen <= 5)
                        {
                            shouldUpdate = true;
                            Console.WriteLine($"[Tracker]   Reason: Car->Bus upgrade (saw full vehicle body, frames={tracker.FramesSeen})");
                        }
                        else if (confidence > tracker.LastConfidence + 0.15f)
                        {
                            shouldUpdate = true;
                            Console.WriteLine($"[Tracker]   Reason: Higher confidence ({confidence:F2} > {tracker.LastConfidence:F2})");
                        }
                        else if (tracker.FramesSeen < 3)
                        {
                            shouldUpdate = true;
                            Console.WriteLine($"[Tracker]   Reason: Initial frames, refining type");
                        }
                        
                        if (shouldUpdate)
                        {
                            tracker.VehicleType = vehicleType;
                            tracker.TypeChanges++;
                            
                            // CRITICAL: Reset counted flag when upgrading Car->Bus
                            // This ensures the vehicle is counted with correct type
                            if (tracker.IsCounted && vehicleType == VehicleType.Bus)
                            {
                                tracker.IsCounted = false;
                                Console.WriteLine($"[Tracker]   ❗ Reset counted flag for Car->Bus upgrade (will be counted as Bus)");
                            }
                        }
                    }
                    
                    // Update tracker
                    tracker.LastCenter = center;
                    tracker.LastBoundingBox = boundingBox;
                    tracker.FramesLost = 0;
                    tracker.FramesSeen++;
                    tracker.LastSeen = DateTime.Now;
                    tracker.LastConfidence = confidence;
                    
                    Console.WriteLine($"[Tracker] Updated ID={tracker.Id}, Type={tracker.VehicleType}, Frames={tracker.FramesSeen}, IoU={iou:F2}, Dist={dist:F0}px");
                    return tracker.Id;
                }
            }

            // Second pass: Standard matching with same type OR car->bus/truck upgrade
            foreach (var tracker in _trackers.Values.ToList())
            {
                double dist = Math.Sqrt(
                    Math.Pow(center.X - tracker.LastCenter.X, 2) + 
                    Math.Pow(center.Y - tracker.LastCenter.Y, 2)
                );
                double iou = CalculateIoU(boundingBox, tracker.LastBoundingBox);

                bool distanceMatch = dist < maxDistance;
                bool iouMatch = iou > minIoU;
                bool typeMatch = tracker.VehicleType == vehicleType;
                
                // SPECIAL CASE: Allow Car -> Bus/Truck upgrade (common when vehicle enters zone)
                // This prevents creating new ID when we see the full body of a large vehicle
                bool isUpgradeCase = (tracker.VehicleType == VehicleType.Car) && 
                                     (vehicleType == VehicleType.Bus) && 
                                     tracker.FramesSeen <= 5 && // Only in first few frames
                                     distanceMatch && 
                                     iou > 0.2; // Lower IoU threshold for upgrade

                if (((distanceMatch || iouMatch) && typeMatch) || isUpgradeCase)
                {
                    double score = iou * 2.0 + (1.0 - dist / maxDistance);
                    double currentBest = bestIoU * 2.0 + (1.0 - minDistance / maxDistance);
                    
                    if (score > currentBest)
                    {
                        minDistance = dist;
                        bestIoU = iou;
                        closestId = tracker.Id;
                    }
                }
            }

            if (closestId != -1)
            {
                var tracker = _trackers[closestId];
                
                // Check if this is a Car->Bus upgrade case
                if (tracker.VehicleType != vehicleType)
                {
                    Console.WriteLine($"[Tracker] 🔄 Upgrading ID={closestId}: {tracker.VehicleType} -> {vehicleType} (IoU={bestIoU:F2}, Frames={tracker.FramesSeen})");
                    Console.WriteLine($"[Tracker]   Reason: Detected larger vehicle body (initial detection was partial)");
                    
                    // CRITICAL: Reset counted flag when upgrading Car->Bus
                    // This ensures the vehicle is counted with correct type
                    if (tracker.IsCounted && vehicleType == VehicleType.Bus)
                    {
                        tracker.IsCounted = false;
                        Console.WriteLine($"[Tracker]   ❗ Reset counted flag for Car->Bus upgrade (will be counted as Bus)");
                    }
                    
                    tracker.VehicleType = vehicleType;
                    tracker.TypeChanges++;
                }
                
                tracker.LastCenter = center;
                tracker.LastBoundingBox = boundingBox;
                tracker.FramesLost = 0;
                tracker.FramesSeen++;
                tracker.LastSeen = DateTime.Now;
                tracker.LastConfidence = confidence;
                
                Console.WriteLine($"[Tracker] Updated ID={closestId}, Type={tracker.VehicleType}, Frames={tracker.FramesSeen}, IoU={bestIoU:F2}, Dist={minDistance:F0}px");
                return closestId;
            }

            // Create new tracker
            int newId = _nextTrackerId++;
            _trackers[newId] = new TrackerInfo
            {
                Id = newId,
                LastCenter = center,
                LastBoundingBox = boundingBox,
                VehicleType = vehicleType,
                InitialVehicleType = vehicleType,
                FramesLost = 0,
                FramesSeen = 1,
                LastSeen = DateTime.Now,
                LastConfidence = confidence,
                TypeChanges = 0,
                IsCounted = false
            };

            string emoji = vehicleType switch
            {
                VehicleType.Motorcycle => "🏍️",
                VehicleType.Bus => "🚌",
                VehicleType.Car => "🚗",
                VehicleType.Bicycle => "🚲",
                _ => "🚙"
            };
            
            Console.WriteLine($"[Tracker] {emoji} NEW ID={newId}, Type={vehicleType}, Conf={confidence:F2}");
            return newId;
        }

        private double CalculateIoU(Rect box1, Rect box2)
        {
            // Calculate intersection
            int x1 = Math.Max(box1.X, box2.X);
            int y1 = Math.Max(box1.Y, box2.Y);
            int x2 = Math.Min(box1.X + box1.Width, box2.X + box2.Width);
            int y2 = Math.Min(box1.Y + box1.Height, box2.Y + box2.Height);

            int intersectionWidth = Math.Max(0, x2 - x1);
            int intersectionHeight = Math.Max(0, y2 - y1);
            double intersectionArea = intersectionWidth * intersectionHeight;

            // Calculate union
            double box1Area = box1.Width * box1.Height;
            double box2Area = box2.Width * box2.Height;
            double unionArea = box1Area + box2Area - intersectionArea;

            // Return IoU
            return unionArea > 0 ? intersectionArea / unionArea : 0;
        }

        private bool IsPointInPolygon(Point point, List<Point> polygon)
        {
            bool inside = false;
            int j = polygon.Count - 1;

            for (int i = 0; i < polygon.Count; i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / 
                    (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }

        private void CleanupOldTrackers()
        {
            var toRemove = new List<int>();
            var now = DateTime.Now;
            
            foreach (var kvp in _trackers)
            {
                double secondsLost = (now - kvp.Value.LastSeen).TotalSeconds;
                
                // Remove trackers not seen for 2 seconds OR with very few frames seen (likely false positive)
                bool tooOld = secondsLost > 2.0;
                bool likelyFalsePositive = kvp.Value.FramesSeen < 3 && secondsLost > 0.5;
                
                if (tooOld || likelyFalsePositive)
                {
                    toRemove.Add(kvp.Key);
                    Console.WriteLine($"[Tracker] Removing ID={kvp.Key} (Seen={kvp.Value.FramesSeen}, Lost={secondsLost:F1}s)");
                }
            }

            foreach (var id in toRemove)
            {
                _trackers.Remove(id);
            }
        }

        public bool IsVehicleCounted(int trackerId)
        {
            if (_trackers.ContainsKey(trackerId))
            {
                return _trackers[trackerId].IsCounted;
            }
            return false;
        }

        public void MarkVehicleAsCounted(int trackerId)
        {
            if (_trackers.ContainsKey(trackerId))
            {
                _trackers[trackerId].IsCounted = true;
                Console.WriteLine($"[Tracker] ✓ Marked ID={trackerId} as counted (Type={_trackers[trackerId].VehicleType})");
            }
        }

        public string GetVehicleFinalType(int trackerId)
        {
            if (_trackers.ContainsKey(trackerId))
            {
                return _trackers[trackerId].VehicleType;
            }
            return "unknown";
        }

        /// <summary>
        /// Check if vehicle has crossed the counting line
        /// Detects crossing from BOTH directions (top to bottom OR bottom to top)
        /// Returns true when vehicle crosses from one side to the other
        /// </summary>
        public bool CheckLineCrossing(int trackerId, Point lineStart, Point lineEnd, int threshold = 50)
        {
            if (!_trackers.ContainsKey(trackerId))
                return false;

            var tracker = _trackers[trackerId];
            Point center = tracker.LastCenter;

            // Calculate distance from point to line
            double distance = DistanceFromPointToLine(center, lineStart, lineEnd);

            // Determine which side of the line the vehicle is on
            int currentPosition = GetSideOfLine(center, lineStart, lineEnd);

            // First time seeing this vehicle - initialize position
            if (tracker.LinePosition == 0 && currentPosition != 0)
            {
                tracker.LinePosition = currentPosition;
                Console.WriteLine($"[LineCrossing] ID={trackerId} Type={tracker.VehicleType} initialized at side={currentPosition}, dist={distance:F1}px");
                return false;
            }

            // Check if crossed the line (changed side)
            // Important: Accept ANY direction change (-1 to +1 OR +1 to -1)
            bool changedSide = (tracker.LinePosition != 0) && (currentPosition != 0) && (tracker.LinePosition != currentPosition);
            bool withinThreshold = Math.Abs(distance) <= threshold;
            
            // Also check if vehicle is VERY close to line (within 10px) even if not changed side yet
            // This helps catch fast-moving vehicles that might skip frames
            bool veryCloseToLine = Math.Abs(distance) <= 10;
            
            // Debug log for all vehicles to diagnose counting issues
            if (tracker.VehicleType == VehicleType.Motorcycle || tracker.VehicleType == VehicleType.Car || tracker.VehicleType == VehicleType.Bus)
            {
                string emoji = tracker.VehicleType switch {
                    VehicleType.Motorcycle => "🏍️",
                    VehicleType.Bus => "🚌",
                    VehicleType.Car => "🚗",
                    _ => "🚙"
                };
                Console.WriteLine($"[LineCrossing] {emoji} ID={trackerId}: side={tracker.LinePosition}->{currentPosition}, dist={distance:F1}px, changed={changedSide}, thresh={withinThreshold}, close={veryCloseToLine}, hasCrossed={tracker.HasCrossedLine}");
            }
            
            // Count when crossing detected OR very close to line
            if (!tracker.HasCrossedLine && ((changedSide && withinThreshold) || (veryCloseToLine && tracker.FramesSeen >= 3)))
            {
                tracker.HasCrossedLine = true;
                string direction = tracker.LinePosition == -1 ? "Top->Bottom" : "Bottom->Top";
                int prevSide = tracker.LinePosition;
                tracker.LinePosition = currentPosition;
                
                Console.WriteLine($"[LineCrossing] ✓✓✓ Vehicle ID={trackerId} CROSSED! Type={tracker.VehicleType}, Direction={direction} ({prevSide}->{currentPosition}), Dist={distance:F1}px");
                return true;
            }

            // Update position smoothly
            if (currentPosition != 0 && Math.Abs(distance) > 5) // Update only if not too close to line
            {
                tracker.LinePosition = currentPosition;
            }
            
            return false;
        }

        /// <summary>
        /// Calculate perpendicular distance from point to line
        /// </summary>
        private double DistanceFromPointToLine(Point point, Point lineStart, Point lineEnd)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;
            double lineLengthSquared = dx * dx + dy * dy;

            if (lineLengthSquared == 0)
                return Math.Sqrt(Math.Pow(point.X - lineStart.X, 2) + Math.Pow(point.Y - lineStart.Y, 2));

            double t = ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / lineLengthSquared;
            t = Math.Max(0, Math.Min(1, t));

            double nearestX = lineStart.X + t * dx;
            double nearestY = lineStart.Y + t * dy;

            return Math.Sqrt(Math.Pow(point.X - nearestX, 2) + Math.Pow(point.Y - nearestY, 2));
        }

        /// <summary>
        /// Determine which side of the line a point is on
        /// Returns: -1 for one side, +1 for the other side, 0 for on the line
        /// </summary>
        private int GetSideOfLine(Point point, Point lineStart, Point lineEnd)
        {
            double cross = (lineEnd.X - lineStart.X) * (point.Y - lineStart.Y) - 
                          (lineEnd.Y - lineStart.Y) * (point.X - lineStart.X);

            if (Math.Abs(cross) < 1e-6)
                return 0;

            return cross > 0 ? 1 : -1;
        }

        public void Dispose()
        {
            _net?.Dispose();
            _onnxSession?.Dispose();
            Console.WriteLine("[VehicleDetector] Disposed");
        }
    }
}