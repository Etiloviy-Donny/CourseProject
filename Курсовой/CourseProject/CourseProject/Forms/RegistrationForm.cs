using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CourseProject
{
    public partial class RegistrationForm : Form
    {
        private string connectionString = @"Data Source=localhost;Initial Catalog=CourseProject;User ID=EgorP;Password=ISPP;Encrypt=False";

        public RegistrationForm()
        {
            InitializeComponent();
        }

        private void RegistrationForm_Load(object sender, EventArgs e)
        {
            // Скрываем выбор роли и устанавливаем роль "Зарегистрированный пользователь"
            LoadRole();
        }

        private void LoadRole()
        {
            // Скрываем элементы связанные с выбором роли

            // Автоматически устанавливаем роль "Зарегистрированный пользователь"
            // Обычно это роль с Id = 4 (но нужно проверить в вашей базе данных)
            SetDefaultRole();
        }

        private void SetDefaultRole()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Ищем роль "Зарегистрированный пользователь" или аналогичную
                    string query = "SELECT IdRole FROM Role WHERE RoleName LIKE '%пользователь%' OR RoleName = 'client' OR RoleName = 'user'";
                    SqlCommand command = new SqlCommand(query, connection);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        // Роль найдена, можно использовать её ID
                        // В реальном коде мы будем использовать это значение при регистрации
                    }
                    else
                    {
                        // Если роль не найдена, используем значение по умолчанию (обычно 4 для клиента)
                        // В реальном коде можно создать роль или использовать известный ID
                    }
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки используем значение по умолчанию
                MessageBox.Show($"Ошибка при определении роли по умолчанию: {ex.Message}", "Предупреждение",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(textBoxLogin.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLogin.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                return false;
            }

            if (textBoxPassword.Text != textBoxConfirmPassword.Text)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxConfirmPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxSurname.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxSurname.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxPhone.Text))
            {
                MessageBox.Show("Введите номер телефона", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPhone.Focus();
                return false;
            }

            return true; // Убрали проверку роли
        }

        private bool IsLoginExists(string login)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM [User] WHERE UserLogin = @Login";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Login", login);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки логина: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
        }



        private int GetDefaultRoleId()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Пробуем найти роль "Зарегистрированный пользователь" или аналогичную
                    string query = @"
                        SELECT IdRole FROM Role 
                        WHERE RoleName LIKE '%пользователь%' 
                           OR RoleName = 'client' 
                           OR RoleName = 'user'
                           OR RoleName = 'Зарегистрированный пользователь'";

                    SqlCommand command = new SqlCommand(query, connection);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return (int)result;
                    }

                    // Если не нашли, пробуем найти любую роль с наименьшим ID (обычно это клиент)
                    query = "SELECT TOP 1 IdRole FROM Role ORDER BY IdRole";
                    command = new SqlCommand(query, connection);
                    result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return (int)result;
                    }

                    // Если вообще нет ролей (маловероятно)
                    return 4; // Значение по умолчанию
                }
            }
            catch (Exception)
            {
                // В случае ошибки возвращаем значение по умолчанию
                return 4; // Обычно 4 - это ID для роли клиента
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            string login = textBoxLogin.Text.Trim();

            if (IsLoginExists(login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLogin.Focus();
                return;
            }

            try
            {
                int defaultRoleId = GetDefaultRoleId();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO [User] 
                                    (UserLogin, UserPassword, UserSurname, UserName, UserPatronymic, TelephoneNumber, IdRole) 
                                    VALUES 
                                    (@Login, @Password, @Surname, @Name, @Patronymic, @Phone, @RoleId)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", (textBoxPassword.Text));
                    command.Parameters.AddWithValue("@Surname", textBoxSurname.Text.Trim());
                    command.Parameters.AddWithValue("@Name", textBoxName.Text.Trim());
                    command.Parameters.AddWithValue("@Patronymic", string.IsNullOrWhiteSpace(textBoxPatronymic.Text) ? (object)DBNull.Value : textBoxPatronymic.Text.Trim());
                    command.Parameters.AddWithValue("@Phone", textBoxPhone.Text.Trim());
                    command.Parameters.AddWithValue("@RoleId", defaultRoleId);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Пользователь успешно зарегистрирован! Вы получили роль 'Зарегистрированный пользователь'.",
                                      "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при регистрации пользователя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBoxPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '+')
            {
                e.Handled = true;
            }
        }
    }
}