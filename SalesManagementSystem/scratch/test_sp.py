import base64
import pyodbc
import json

# Read system.dat
with open(r'c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat', 'rb') as f:
    raw_b64 = f.read().decode('utf-8')

raw = base64.b64decode(raw_b64).decode('utf-8')
parts = raw.split('@@')
conn_str = parts[1]
print("Conn string:", conn_str)

# Convert OLE DB / SqlClient conn str to pyodbc connection string
# Driver={SQL Server};...
driver_conn = f"Driver={{SQL Server}};{conn_str}"

try:
    conn = pyodbc.connect(driver_conn)
    cursor = conn.cursor()
    
    # 1. Get sp_CongNo_PhaseTra_NCC_GetList text
    cursor.execute("EXEC sp_helptext 'sp_CongNo_PhaseTra_NCC_GetList'")
    rows = cursor.fetchall()
    sp_ncc_text = "".join([r[0] for r in rows])
    with open("scratch/sp_CongNo_PhaseTra_NCC_GetList_DB.sql", "w", encoding="utf-8") as f:
        f.write(sp_ncc_text)
    print("Saved sp_CongNo_PhaseTra_NCC_GetList_DB.sql")
    
    # 2. Get sp_Dashboard_GetData text
    cursor.execute("EXEC sp_helptext 'sp_Dashboard_GetData'")
    rows2 = cursor.fetchall()
    sp_dash_text = "".join([r[0] for r in rows2])
    with open("scratch/sp_Dashboard_GetData_DB.sql", "w", encoding="utf-8") as f:
        f.write(sp_dash_text)
    print("Saved sp_Dashboard_GetData_DB.sql")
    
    # 3. Test sp_Dashboard_GetData for 2026-08-01 to 2026-08-31
    cursor.execute("EXEC sp_Dashboard_GetData '2026-08-01', '2026-08-31', '2026-07-01', '2026-07-31'")
    dash_row = cursor.fetchone()
    columns = [column[0] for column in cursor.description]
    dash_dict = dict(zip(columns, [float(v) if isinstance(v, (int, float)) else str(v) for v in dash_row]))
    print("DASHBOARD RESULT FOR CÔNG NỢ NCC:")
    print("TongTienHangNCC:", dash_dict.get("TongTienHangNCC"))
    print("DaThanhToanNCC:", dash_dict.get("DaThanhToanNCC"))
    print("CongNoNhaCungCap:", dash_dict.get("CongNoNhaCungCap"))

    # 4. Test sp_CongNo_PhaseTra_NCC_GetList for no date filter vs with date filter
    print("\nMAN HINH CONG NO NCC (ALL TIME):")
    cursor.execute("EXEC sp_CongNo_PhaseTra_NCC_GetList NULL, NULL, NULL, NULL")
    list_all = cursor.fetchall()
    cols = [column[0] for column in cursor.description]
    total_hang = sum(r[cols.index('TongTienHang')] for r in list_all)
    total_da_tra = sum(r[cols.index('DaThanhToan')] for r in list_all)
    total_con_lai = sum(r[cols.index('ConLai')] for r in list_all)
    print("Total TongTienHang:", total_hang)
    print("Total DaThanhToan:", total_da_tra)
    print("Total ConLai:", total_con_lai)

    print("\nMAN HINH CONG NO NCC (01/08/2026 -> 31/08/2026):")
    cursor.execute("EXEC sp_CongNo_PhaseTra_NCC_GetList '2026-08-01', '2026-08-31', NULL, NULL")
    list_aug = cursor.fetchall()
    total_hang_aug = sum(r[cols.index('TongTienHang')] for r in list_aug)
    total_da_tra_aug = sum(r[cols.index('DaThanhToan')] for r in list_aug)
    total_con_lai_aug = sum(r[cols.index('ConLai')] for r in list_aug)
    print("Total TongTienHang:", total_hang_aug)
    print("Total DaThanhToan:", total_da_tra_aug)
    print("Total ConLai:", total_con_lai_aug)

    conn.Close()
except Exception as e:
    print("Python SQL error:", e)
