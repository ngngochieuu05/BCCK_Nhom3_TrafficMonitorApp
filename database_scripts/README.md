# 🗄️ Database Scripts - Traffic Monitor

## 📁 File Structure

```
database_scripts/
├── 01_create_database.sql      - Tạo database và tables
├── 02_sample_queries.sql       - Các câu query mẫu
├── 03_insert_sample_data.sql   - Dữ liệu test
└── README.md                   - File này
```

---

## 🚀 Hướng Dẫn Setup Database

### **Cách 1: Sử dụng SQL Server Management Studio (SSMS)**

#### Bước 1: Mở SSMS
1. Tải và cài đặt SSMS: https://aka.ms/ssmsfullsetup
2. Mở **SQL Server Management Studio**

#### Bước 2: Connect to LocalDB
1. Click **Connect > Database Engine**
2. **Server name:** `(localdb)\MSSQLLocalDB`
3. **Authentication:** Windows Authentication
4. Click **Connect**

#### Bước 3: Chạy Script Tạo Database
1. Click **File > Open > File...**
2. Chọn file `01_create_database.sql`
3. Click **Execute** (hoặc nhấn `F5`)
4. Kiểm tra Messages: "Database TrafficMonitorDb created successfully!"

#### Bước 4: Insert Sample Data (Tùy chọn)
1. Mở file `03_insert_sample_data.sql`
2. Click **Execute**
3. Verify: Sẽ thấy "Inserted sample vehicle detections"

#### Bước 5: Test Queries
1. Mở file `02_sample_queries.sql`
2. Chọn một query (highlight)
3. Click **Execute** hoặc `F5`

---

### **Cách 2: Sử dụng Azure Data Studio**

#### Bước 1: Cài Đặt
1. Tải Azure Data Studio: https://aka.ms/azuredatastudio
2. Cài đặt và mở

#### Bước 2: Connect
1. Click **New Connection**
2. **Server:** `(localdb)\MSSQLLocalDB`
3. **Authentication:** Windows Authentication
4. Click **Connect**

#### Bước 3: Chạy Scripts
1. Click **File > Open File**
2. Chọn `01_create_database.sql`
3. Click **Run** (hoặc `F5`)
4. Làm tương tự với các file khác

---

### **Cách 3: Sử dụng VS Code + SQL Extension**

#### Bước 1: Cài Extension
1. Mở VS Code
2. Install extension: **SQL Server (mssql)**
3. Reload VS Code

#### Bước 2: Connect
1. Nhấn `Ctrl+Shift+P`
2. Gõ: `MS SQL: Connect`
3. Server: `(localdb)\MSSQLLocalDB`
4. Windows Authentication

#### Bước 3: Chạy Scripts
1. Mở file `.sql`
2. Nhấn `Ctrl+Shift+E` để execute

---

### **Cách 4: Command Line (sqlcmd)**

```powershell
# 1. Create database
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "01_create_database.sql"

# 2. Insert sample data (optional)
sqlcmd -S "(localdb)\MSSQLLocalDB" -d TrafficMonitorDb -i "03_insert_sample_data.sql"

# 3. Query data
sqlcmd -S "(localdb)\MSSQLLocalDB" -d TrafficMonitorDb -Q "SELECT * FROM TrafficSessions"
```

---

## 📊 Sample Queries Explained

### Query 1: View All Sessions
```sql
SELECT * FROM TrafficSessions ORDER BY StartTime DESC;
```
Xem tất cả phiên giám sát, mới nhất trước.

### Query 3: Vehicle Count by Type
```sql
SELECT VehicleType, COUNT(*) AS Total FROM VehicleDetections GROUP BY VehicleType;
```
Đếm số lượng mỗi loại xe.

### Query 6: Hourly Statistics
```sql
SELECT * FROM HourlyStatistics ORDER BY HourTimestamp DESC;
```
Thống kê theo giờ với mức tắc nghẽn.

### Query 8: Peak Traffic Hours
```sql
SELECT TOP 10 * FROM HourlyStatistics ORDER BY TotalVehicles DESC;
```
10 giờ cao điểm nhất.

---

## 🔍 Useful Management Queries

### Check Database Size
```sql
SELECT 
    name AS DatabaseName,
    (size * 8.0 / 1024) AS SizeMB
FROM sys.master_files
WHERE name = 'TrafficMonitorDb';
```

### Check Table Sizes
```sql
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCount,
    (SUM(a.total_pages) * 8 / 1024.0) AS TotalSpaceMB
FROM sys.tables t
JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
JOIN sys.partitions p ON i.object_id = p.OBJECT_ID
JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.name IN ('TrafficSessions', 'VehicleDetections', 'HourlyStatistics')
GROUP BY t.Name, p.Rows;
```

### Backup Database
```sql
BACKUP DATABASE TrafficMonitorDb 
TO DISK = 'D:\Backups\TrafficMonitorDb.bak'
WITH FORMAT;
```

### Delete Old Data (older than 30 days)
```sql
DELETE FROM TrafficSessions WHERE StartTime < DATEADD(DAY, -30, GETDATE());
```

---

## 🛠️ Troubleshooting

### Error: "Cannot connect to (localdb)\MSSQLLocalDB"
**Solution:**
```powershell
# Start LocalDB
sqllocaldb start MSSQLLocalDB

# Check instances
sqllocaldb info

# If not found, create it
sqllocaldb create MSSQLLocalDB
```

### Error: "Database already exists"
**Solution:** 
- Uncomment phần DROP DATABASE trong `01_create_database.sql`
- Hoặc chạy: `DROP DATABASE TrafficMonitorDb;`

### Error: "Login failed"
**Solution:**
- Đảm bảo dùng **Windows Authentication**
- Không dùng SQL Server Authentication cho LocalDB

---

## 📈 Database Schema Diagram

```
TrafficSessions (Parent)
├── SessionId (PK)
├── StartTime
├── EndTime
├── SourceType
├── TotalVehicles
└── ... (metadata)

VehicleDetections (Child)
├── DetectionId (PK)
├── SessionId (FK) → TrafficSessions
├── DetectedTime
├── VehicleType
├── Confidence
└── ... (position, size)

HourlyStatistics (Independent)
├── StatId (PK)
├── HourTimestamp (UNIQUE)
├── TotalVehicles
├── CarCount, MotorcycleCount, ...
└── CongestionLevel
```

**Relationships:**
- `TrafficSessions` 1-to-Many `VehicleDetections` (CASCADE DELETE)
- `HourlyStatistics` is independent (aggregated data)

---

## 🎯 Quick Reference

| Task | Command |
|------|---------|
| Connect | `sqlcmd -S "(localdb)\MSSQLLocalDB"` |
| Use DB | `USE TrafficMonitorDb;` |
| List tables | `SELECT * FROM INFORMATION_SCHEMA.TABLES;` |
| Count rows | `SELECT COUNT(*) FROM TrafficSessions;` |
| Recent data | `SELECT TOP 10 * FROM VehicleDetections ORDER BY DetectedTime DESC;` |

---

## 📝 Notes

1. **LocalDB vs SQL Server:**
   - LocalDB: Development only, single user
   - SQL Server: Production, multi-user

2. **Connection String in App:**
   ```csharp
   Server=(localdb)\mssqllocaldb;Database=TrafficMonitorDb;Trusted_Connection=True;
   ```

3. **Performance:**
   - Indexes already created on common query columns
   - Consider archiving old data if > 1M rows

4. **Security:**
   - LocalDB uses Windows Authentication only
   - No password needed
   - Only accessible by current Windows user

---

**🎉 Database setup complete! Bây giờ bạn có thể:**
- ✅ Mở database trong SSMS/Azure Data Studio
- ✅ Chạy queries để xem dữ liệu
- ✅ Phân tích traffic patterns
- ✅ Export reports

**Need help?** Check queries in `02_sample_queries.sql`!
