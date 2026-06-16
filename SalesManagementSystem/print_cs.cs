using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string cs = SalesManagementSystem.Helpers.Security.ConfigManager.GetConnectionString("DefaultConnection");
        Console.WriteLine(cs);
    }
}
