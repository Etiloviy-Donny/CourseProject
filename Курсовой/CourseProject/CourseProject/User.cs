using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class User
{
    public int Id { get; set; }
    public string Telephone { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string Surname { get; set; }
    public string Name { get; set; }
    public string Patronymic { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }

    public string FullName => $"{Surname} {Name} {Patronymic}".Trim();

    public static string GetPhoneNumberById(int userId, string connectionString)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT TelephoneNumber FROM [User] WHERE IdUser = IdUser";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    var result = command.ExecuteScalar();
                    return result?.ToString() ?? "Не указан";
                }
            }
        }
        catch (Exception)
        {
            return "Ошибка получения";
        }
    }

}