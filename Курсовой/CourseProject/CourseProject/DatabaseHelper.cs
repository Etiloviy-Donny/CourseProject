using CourseProject.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

public static class DatabaseHelper
{
    private const string ConnectionString = "Server=localhost;Database=YourDatabase;Integrated Security=true;";

    // Получить все заявки с телефонами пользователей вместо IdUser
    public static List<RequestWithPhone> GetRequestsWithPhones()
    {
        var requests = new List<RequestWithPhone>();

        try
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT 
                        r.IdRequest,
                        r.RequestDate,
                        r.Address,
                        u.TelephoneNumber AS UserPhone,
                        r.CountersNumber,
                        r.Comment,
                        r.Master,
                        r.Status
                    FROM Request r
                    INNER JOIN [User] u ON r.IdUser = u.IdUser
                    ORDER BY r.RequestDate DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new RequestWithPhone
                            {
                                IdRequest = reader.GetInt32(reader.GetOrdinal("IdRequest")),
                                RequestDate = reader.GetDateTime(reader.GetOrdinal("RequestDate")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                UserPhone = reader.IsDBNull(reader.GetOrdinal("UserPhone")) ?
                                           "Не указан" : reader.GetString(reader.GetOrdinal("UserPhone")),
                                CountersNumber = reader.GetString(reader.GetOrdinal("CountersNumber")),
                                Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ?
                                         string.Empty : reader.GetString(reader.GetOrdinal("Comment")),
                                Master = reader.IsDBNull(reader.GetOrdinal("Master")) ?
                                        "Не назначен" : reader.GetString(reader.GetOrdinal("Master")),
                                Status = reader.GetString(reader.GetOrdinal("Status"))
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка");
        }

        return requests;
    }

    // Получить заявки конкретного пользователя по его телефону
    public static List<RequestWithPhone> GetRequestsByUserPhone(string phoneNumber)
    {
        var requests = new List<RequestWithPhone>();

        try
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT 
                        r.IdRequest,
                        r.RequestDate,
                        r.Address,
                        u.TelephoneNumber AS UserPhone,
                        r.CountersNumber,
                        r.Comment,
                        r.Master,
                        r.Status
                    FROM Request r
                    INNER JOIN [User] u ON r.IdUser = u.IdUser
                    WHERE u.TelephoneNumber = @Phone
                    ORDER BY r.RequestDate DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Phone", phoneNumber);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new RequestWithPhone
                            {
                                IdRequest = reader.GetInt32(reader.GetOrdinal("IdRequest")),
                                RequestDate = reader.GetDateTime(reader.GetOrdinal("RequestDate")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                UserPhone = reader.GetString(reader.GetOrdinal("UserPhone")),
                                CountersNumber = reader.GetString(reader.GetOrdinal("CountersNumber")),
                                Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ?
                                         string.Empty : reader.GetString(reader.GetOrdinal("Comment")),
                                Master = reader.IsDBNull(reader.GetOrdinal("Master")) ?
                                        "Не назначен" : reader.GetString(reader.GetOrdinal("Master")),
                                Status = reader.GetString(reader.GetOrdinal("Status"))
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка");
        }

        return requests;
    }
}