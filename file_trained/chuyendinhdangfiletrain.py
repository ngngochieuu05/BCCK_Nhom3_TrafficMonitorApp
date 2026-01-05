"""
Chương trình chuyển đổi YOLOv8 .pt sang .onnx
Tương thích với TrafficMonitorApp C# Application
"""

import os
import sys
from pathlib import Path

def check_ultralytics():
    """Kiểm tra và cài đặt ultralytics nếu chưa có"""
    try:
        import ultralytics
        print(f"✅ Ultralytics đã cài đặt - Phiên bản: {ultralytics.__version__}")
        return True
    except ImportError:
        print("❌ Chưa cài đặt ultralytics")
        print("📦 Đang cài đặt ultralytics...")
        os.system("pip install ultralytics")
        try:
            import ultralytics
            print(f"✅ Cài đặt thành công - Phiên bản: {ultralytics.__version__}")
            return True
        except:
            print("❌ Không thể cài đặt ultralytics. Vui lòng chạy: pip install ultralytics")
            return False

def convert_pt_to_onnx(pt_file_path, output_dir=None, imgsz=640, simplify=True, dynamic=False):
    """
    Chuyển đổi YOLOv8 .pt sang .onnx
    
    Args:
        pt_file_path (str): Đường dẫn file .pt
        output_dir (str): Thư mục lưu file .onnx (mặc định: cùng thư mục với .pt)
        imgsz (int): Kích thước ảnh đầu vào (640, 320, 1280...)
        simplify (bool): Đơn giản hóa model ONNX (khuyến nghị: True)
        dynamic (bool): Dynamic batch size (False = batch size cố định = 1)
    
    Returns:
        str: Đường dẫn file .onnx đã tạo
    """
    from ultralytics import YOLO
    
    # Kiểm tra file .pt tồn tại
    pt_path = Path(pt_file_path)
    if not pt_path.exists():
        raise FileNotFoundError(f"❌ Không tìm thấy file: {pt_file_path}")
    
    print(f"\n{'='*60}")
    print(f"🔄 CHUYỂN ĐỔI YOLOV8 .PT SANG .ONNX")
    print(f"{'='*60}")
    print(f"📂 File đầu vào: {pt_path.name}")
    print(f"📏 Kích thước ảnh: {imgsz}x{imgsz}")
    print(f"🔧 Simplify: {simplify}")
    print(f"🔧 Dynamic batch: {dynamic}")
    print(f"{'='*60}\n")
    
    # Tải model
    print("📦 Đang tải model YOLOv8...")
    model = YOLO(str(pt_path))
    print("✅ Tải model thành công!")
    
    # Xuất sang ONNX
    print(f"\n🚀 Đang chuyển đổi sang ONNX...")
    print("⏳ Vui lòng đợi...")
    
    export_path = model.export(
        format='onnx',
        imgsz=imgsz,
        simplify=simplify,
        dynamic=dynamic,
        opset=12  # ONNX opset version (12 tương thích tốt với ONNX Runtime)
    )
    
    # Di chuyển file nếu cần
    if output_dir:
        output_path = Path(output_dir)
        output_path.mkdir(parents=True, exist_ok=True)
        
        final_path = output_path / Path(export_path).name
        if Path(export_path) != final_path:
            import shutil
            shutil.move(export_path, final_path)
            export_path = str(final_path)
    
    print(f"\n{'='*60}")
    print(f"✅ CHUYỂN ĐỔI THÀNH CÔNG!")
    print(f"{'='*60}")
    print(f"📁 File ONNX: {export_path}")
    print(f"📊 Kích thước: {Path(export_path).stat().st_size / (1024*1024):.2f} MB")
    print(f"{'='*60}\n")
    
    return export_path

def batch_convert(pt_folder, output_folder=None, imgsz=640):
    """
    Chuyển đổi tất cả file .pt trong thư mục
    
    Args:
        pt_folder (str): Thư mục chứa các file .pt
        output_folder (str): Thư mục lưu file .onnx
        imgsz (int): Kích thước ảnh
    """
    pt_files = list(Path(pt_folder).glob("*.pt"))
    
    if not pt_files:
        print(f"❌ Không tìm thấy file .pt nào trong: {pt_folder}")
        return
    
    print(f"\n📦 Tìm thấy {len(pt_files)} file .pt")
    print(f"{'='*60}\n")
    
    success_count = 0
    for i, pt_file in enumerate(pt_files, 1):
        print(f"\n[{i}/{len(pt_files)}] Đang xử lý: {pt_file.name}")
        try:
            convert_pt_to_onnx(str(pt_file), output_folder, imgsz)
            success_count += 1
        except Exception as e:
            print(f"❌ Lỗi: {e}\n")
    
    print(f"\n{'='*60}")
    print(f"🎉 HOÀN THÀNH: {success_count}/{len(pt_files)} file")
    print(f"{'='*60}\n")

def main():
    """Main function - Interactive mode"""
    print("\n" + "="*60)
    print("🚗 YOLOV8 .PT TO .ONNX CONVERTER")
    print("   Dành cho TrafficMonitorApp")
    print("="*60 + "\n")
    
    # Kiểm tra ultralytics
    if not check_ultralytics():
        return
    
    print("\n" + "="*60)
    print("CHỌN CHẾ ĐỘ:")
    print("="*60)
    print("1. Chuyển đổi 1 file .pt")
    print("2. Chuyển đổi tất cả file .pt trong thư mục")
    print("0. Thoát")
    print("="*60)
    
    choice = input("\n👉 Chọn (1/2/0): ").strip()
    
    if choice == "0":
        print("👋 Tạm biệt!")
        return
    
    elif choice == "1":
        # Chuyển đổi 1 file
        pt_file = input("\n📂 Nhập đường dẫn file .pt: ").strip().strip('"')
        
        if not pt_file:
            print("❌ Đường dẫn trống!")
            return
        
        # Tùy chọn nâng cao
        print("\n" + "="*60)
        print("TÙY CHỌN NÂNG CAO (Enter để dùng mặc định)")
        print("="*60)
        
        output_dir = input("📁 Thư mục lưu .onnx (mặc định: cùng folder): ").strip().strip('"')
        if not output_dir:
            output_dir = None
        
        imgsz_input = input("📏 Kích thước ảnh (mặc định: 640): ").strip()
        imgsz = int(imgsz_input) if imgsz_input else 640
        
        try:
            onnx_path = convert_pt_to_onnx(pt_file, output_dir, imgsz)
            print(f"✅ Sử dụng file này trong TrafficMonitorApp:")
            print(f"   {onnx_path}")
        except Exception as e:
            print(f"\n❌ LỖI: {e}")
    
    elif choice == "2":
        # Chuyển đổi nhiều file
        pt_folder = input("\n📂 Nhập thư mục chứa file .pt: ").strip().strip('"')
        
        if not pt_folder:
            print("❌ Đường dẫn trống!")
            return
        
        output_folder = input("📁 Thư mục lưu .onnx (Enter = cùng folder): ").strip().strip('"')
        if not output_folder:
            output_folder = None
        
        imgsz_input = input("📏 Kích thước ảnh (mặc định: 640): ").strip()
        imgsz = int(imgsz_input) if imgsz_input else 640
        
        try:
            batch_convert(pt_folder, output_folder, imgsz)
        except Exception as e:
            print(f"\n❌ LỖI: {e}")
    
    else:
        print("❌ Lựa chọn không hợp lệ!")

if __name__ == "__main__":
    # Chế độ command line arguments
    if len(sys.argv) > 1:
        pt_file = sys.argv[1]
        output_dir = sys.argv[2] if len(sys.argv) > 2 else None
        imgsz = int(sys.argv[3]) if len(sys.argv) > 3 else 640
        
        try:
            if check_ultralytics():
                convert_pt_to_onnx(pt_file, output_dir, imgsz)
        except Exception as e:
            print(f"\n❌ LỖI: {e}")
            sys.exit(1)
    else:
        # Chế độ interactive
        main()
