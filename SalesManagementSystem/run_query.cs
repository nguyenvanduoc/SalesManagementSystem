using System;
using System.Data.SqlClient;
using SalesManagementSystem.Helpers.Security;

class Program {
    static void Main(string[] args) {
        try {
            string connStr = ConfigManager.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connStr)) {
                conn.Open();
                string scriptName = args.Length > 0 ? args[0] : "App_Data/add_sotienkhac.sql";
                string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptName);
                if (System.IO.File.Exists(sqlPath)) {
                    string sql = System.IO.File.ReadAllText(sqlPath);
                    var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts) {
                        if (!string.IsNullOrWhiteSpace(part)) {
                            using (var cmd = conn.CreateCommand()) {
                                cmd.CommandText = part;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    Console.WriteLine("SQL script " + scriptName + " executed successfully!");
                } else {
                    Console.WriteLine("File " + scriptName + " not found!");
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
