from ultralytics import YOLO
import torch
import argparse

def train_yolo(data_yaml=None, platform="win"):

    # Nếu không truyền data_yaml từ command line, dùng đường dẫn mặc định
    if data_yaml is None:
        if platform == "mac":
            data_yaml = "/path/to/Intersection Traffic.v10i.yolov8/data.yaml"
        elif platform == "win":
            data_yaml = r"D:\C#\project\Do_An_Cuoi_Ki\Backup_App\TrafficMonitorApp\Intersection Traffic.v10i.yolov8\data.yaml"

    # Chọn device và cấu hình theo platform
    if platform == "mac":
        device = "mps" if torch.backends.mps.is_available() else "cpu"
        model_file = "yolov8n.pt"
        batch_size = 32
        imgsz = 640
        epochs = 50
        project_name = "vehicle_mac"
    elif platform == "win":
        device = 0
        model_file = "yolov8s.pt"
        batch_size = 32
        imgsz = 640
        epochs = 60
        project_name = "vehicle_win"
    else:
        raise ValueError("Platform phải là 'mac' hoặc 'win'")

    print(f"Training on {platform} with device: {device}")
    print(f"Using data YAML: {data_yaml}")

    # Load model pretrained
    model = YOLO(model_file)

    # Train
    model.train(
        data=data_yaml,
        epochs=epochs,
        imgsz=imgsz,
        batch=batch_size,
        project=project_name,
        name="yolo_vehicle",
        pretrained=True,
        device=device
    )

    # Export ONNX
    model.export(format="onnx")
    print(f"Model đã export sang ONNX tại project {project_name}")

def main():
    parser = argparse.ArgumentParser(description="YOLO Vehicle Training")
    parser.add_argument("--data_yaml", type=str, default=None,
                        help="Đường dẫn tới file YAML dataset")
    parser.add_argument("--platform", type=str, default="mac",
                        help="Hệ điều hành: mac hoặc win")
    args = parser.parse_args()

    train_yolo(args.data_yaml, platform=args.platform)

if __name__ == "__main__":
    main()
