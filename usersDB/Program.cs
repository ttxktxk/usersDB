using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = "Host=localhost;Username=postgres;Password=123456789;Database=postgres";

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            using (var setSearchPathCmd = new NpgsqlCommand("SET search_path TO schema;", conn))
            {
                setSearchPathCmd.ExecuteNonQuery();
            }

            string userName = "Michael Brown";

            // ดึง user_id จากฐานข้อมูล
            string selectUserQuery = "SELECT user_id FROM users WHERE name = @name";
            object userId = null;

            using (var cmd = new NpgsqlCommand(selectUserQuery, conn))
            {
                cmd.Parameters.AddWithValue("name", userName);
                userId = cmd.ExecuteScalar();
            }

            if (userId == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            // เริ่มต้น
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    float newWeight = 40; 
                    float newHeight = 160;

                    // อัปเดตข้อมูลใน user_details
                    string updateQuery = "UPDATE user_details SET weight = @weight, height = @height WHERE user_id = @user_id";
                    using (var updateCmd = new NpgsqlCommand(updateQuery, conn, transaction))
                    {
                        updateCmd.Parameters.AddWithValue("weight", newWeight);
                        updateCmd.Parameters.AddWithValue("height", newHeight);
                        updateCmd.Parameters.AddWithValue("user_id", userId);
                        updateCmd.ExecuteNonQuery();
                    }

                    // ตรวจสอบเงื่อนไขหลังจากการอัปเดต
                    if (newWeight < 50)
                    {
                        Console.WriteLine("Warning: Weight is less than 50.");
                        throw new InvalidOperationException("Invalid weight. Rolling back transaction.");
                    }

                    // หากไม่มีข้อผิดพลาด ให้ Commit
                    transaction.Commit();
                    Console.WriteLine("User details updated successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    transaction.Rollback();
                    Console.WriteLine("Transaction rolled back.");
                }
            }
        }
    }
}
