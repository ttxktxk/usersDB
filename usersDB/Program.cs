using System;
using Npgsql;

class Program
{
    static void Main()
    {
        // ข้อมูลการเชื่อมต่อ
        string connString = "Host=localhost;Username=postgres;Password=123456789;Database=postgres";

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            // ตั้งค่า search_path ให้ชี้ไปที่ schema ของคุณ
            using (var setSearchPathCmd = new NpgsqlCommand("SET search_path TO schema;", conn))
            {
                setSearchPathCmd.ExecuteNonQuery();
            }

            // เริ่มต้น Transaction
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // กำหนดชื่อที่ต้องการค้นหา
                    string userName = "Michael Brown";

                    // ค้นหาชื่อในตาราง users
                    string selectUserQuery = "SELECT user_id FROM users WHERE name = @name";
                    using (var cmd = new NpgsqlCommand(selectUserQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("name", userName);
                        var userId = cmd.ExecuteScalar();

                        // ถ้าไม่พบชื่อใน users ให้แจ้งว่าไม่พบและ Rollback
                        if (userId == null)
                        {
                            Console.WriteLine("User not found.");
                            transaction.Rollback();
                            return;
                        }

                        // กำหนดน้ำหนักและความสูงใหม่
                        float newWeight = 55;  
                        float newHeight = 150; 

                        // ตรวจสอบน้ำหนักก่อน
                        if (newWeight < 50)
                        {
                            Console.WriteLine("Error: Weight is less than 50.");
                            transaction.Rollback();
                            return;
                        }

                        // อัปเดตข้อมูลใน user_details
                        string updateQuery = "UPDATE user_details SET weight = @weight, height = @height WHERE user_id = @user_id";
                        using (var updateCmd = new NpgsqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("weight", newWeight);
                            updateCmd.Parameters.AddWithValue("height", newHeight);
                            updateCmd.Parameters.AddWithValue("user_id", userId);
                            updateCmd.ExecuteNonQuery();
                        }

                        // ทำการ Commit หากไม่มีข้อผิดพลาด
                        transaction.Commit();
                        Console.WriteLine("User details updated successfully.");
                    }
                }
                catch (Npgsql.PostgresException ex)
                {
                    Console.WriteLine($"Postgres Error: {ex.Message}");
                    Console.WriteLine($"Error Code: {ex.SqlState}");
                    transaction.Rollback();
                }

            }
        }
    }
}
