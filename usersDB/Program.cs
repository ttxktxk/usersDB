using System;
using Npgsql;

class Program
{
    static void Main()
    {
        // �����š����������
        string connString = "Host=localhost;Username=postgres;Password=123456789;Database=postgres";

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            // ��駤�� search_path �����价�� schema �ͧ�س
            using (var setSearchPathCmd = new NpgsqlCommand("SET search_path TO schema;", conn))
            {
                setSearchPathCmd.ExecuteNonQuery();
            }

            // ������� Transaction
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // ��˹����ͷ���ͧ��ä���
                    string userName = "Michael Brown";

                    // ���Ҫ���㹵��ҧ users
                    string selectUserQuery = "SELECT user_id FROM users WHERE name = @name";
                    using (var cmd = new NpgsqlCommand(selectUserQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("name", userName);
                        var userId = cmd.ExecuteScalar();

                        // �����辺����� users ����������辺��� Rollback
                        if (userId == null)
                        {
                            Console.WriteLine("User not found.");
                            transaction.Rollback();
                            return;
                        }

                        // ��˹����˹ѡ��Ф����٧����
                        float newWeight = 55;  
                        float newHeight = 150; 

                        // ��Ǩ�ͺ���˹ѡ��͹
                        if (newWeight < 50)
                        {
                            Console.WriteLine("Error: Weight is less than 50.");
                            transaction.Rollback();
                            return;
                        }

                        // �ѻവ������� user_details
                        string updateQuery = "UPDATE user_details SET weight = @weight, height = @height WHERE user_id = @user_id";
                        using (var updateCmd = new NpgsqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("weight", newWeight);
                            updateCmd.Parameters.AddWithValue("height", newHeight);
                            updateCmd.Parameters.AddWithValue("user_id", userId);
                            updateCmd.ExecuteNonQuery();
                        }

                        // �ӡ�� Commit �ҡ����բ�ͼԴ��Ҵ
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
