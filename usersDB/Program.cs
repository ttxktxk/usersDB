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

            // �֧ user_id �ҡ�ҹ������
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

            // �������
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    float newWeight = 40; 
                    float newHeight = 160;

                    // �ѻവ������� user_details
                    string updateQuery = "UPDATE user_details SET weight = @weight, height = @height WHERE user_id = @user_id";
                    using (var updateCmd = new NpgsqlCommand(updateQuery, conn, transaction))
                    {
                        updateCmd.Parameters.AddWithValue("weight", newWeight);
                        updateCmd.Parameters.AddWithValue("height", newHeight);
                        updateCmd.Parameters.AddWithValue("user_id", userId);
                        updateCmd.ExecuteNonQuery();
                    }

                    // ��Ǩ�ͺ���͹���ѧ�ҡ����ѻവ
                    if (newWeight < 50)
                    {
                        Console.WriteLine("Warning: Weight is less than 50.");
                        throw new InvalidOperationException("Invalid weight. Rolling back transaction.");
                    }

                    // �ҡ����բ�ͼԴ��Ҵ ��� Commit
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
