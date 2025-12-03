using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CourseProject
{
    public partial class AuthorizationForm : Form
    {
        private const string ConnectionString = "Data Source=localhost;Initial Catalog=CourseProject;User ID=EgorP;Password=ISPP;Encrypt=False";

        private TextBox txtLogin;
        private TextBox txtPassword;
        private Label lblStatus;

        public AuthorizationForm()
        {
            CreateLoginForm();
            ConfigureForm();
        }

        private void ConfigureForm()
        {
            this.Text = "Авторизация";
            this.Size = new Size(350, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CreateLoginForm()
        {
            var lblLogin = new Label
            {
                Text = "Логин:",
                Location = new Point(30, 30),
                Size = new Size(80, 20),
                Font = new Font("Microsoft Sans Serif", 10F)
            };

            txtLogin = new TextBox
            {
                Location = new Point(120, 30),
                Size = new Size(180, 25),
                Font = new Font("Microsoft Sans Serif", 10F),
                Name = "txtLogin"
            };

            var lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(30, 70),
                Size = new Size(80, 20),
                Font = new Font("Microsoft Sans Serif", 10F)
            };

            txtPassword = new TextBox
            {
                Location = new Point(120, 70),
                Size = new Size(180, 25),
                Font = new Font("Microsoft Sans Serif", 10F),
                PasswordChar = '*',
                Name = "txtPassword"
            };

            var btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(120, 120),
                Size = new Size(80, 30),
                Font = new Font("Microsoft Sans Serif", 10F),
                BackColor = Color.LightBlue
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(220, 120),
                Size = new Size(80, 30),
                Font = new Font("Microsoft Sans Serif", 10F),
                BackColor = Color.LightCoral
            };

            var btnRegister = new Button
            {
                Text = "Зарегистрироваться",
                Location = new Point(120, 160),
                Size = new Size(180, 30),
                Font = new Font("Microsoft Sans Serif", 10F),
                BackColor = Color.LightGreen
            };

            lblStatus = new Label
            {
                Text = "",
                Location = new Point(120, 100),
                Size = new Size(280, 20),
                Font = new Font("Microsoft Sans Serif", 9F),
                ForeColor = Color.Red,
                Name = "lblStatus"
            };

            btnLogin.Click += BtnLogin_Click;
            btnCancel.Click += BtnCancel_Click;
            btnRegister.Click += BtnRegister_Click;
            txtPassword.KeyPress += TxtPassword_KeyPress;

            this.Controls.AddRange(new Control[]
            {
                lblLogin, txtLogin,
                lblPassword, txtPassword,
                btnLogin, btnCancel,
                btnRegister,
                lblStatus
            });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            AuthenticateUser();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                AuthenticateUser();
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            OpenRegistrationForm();
        }

        private void OpenRegistrationForm()
        {
            try
            {
                RegistrationForm registrationForm = new RegistrationForm();

                // Открываем форму регистрации как диалоговое окно
                DialogResult result = registrationForm.ShowDialog();

                // Если регистрация прошла успешно, можно очистить поля или показать сообщение
                if (result == DialogResult.OK)
                {
                    lblStatus.Text = "Регистрация завершена! Теперь вы можете войти.";
                    lblStatus.ForeColor = Color.Green;

                    // Очищаем поля для удобства
                    txtLogin.Text = "";
                    txtPassword.Text = "";
                    txtLogin.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы регистрации: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AuthenticateUser()
        {
            string login = txtLogin?.Text?.Trim() ?? "";
            string password = txtPassword?.Text ?? "";

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "Введите логин и пароль";
                return;
            }

            try
            {
                lblStatus.Text = "Проверка данных...";
                lblStatus.ForeColor = Color.Blue;

                // Используем реальную проверку с БД
                var user = AuthenticateInDatabase(login, password);

                if (user != null)
                {
                    lblStatus.Text = "Успешная авторизация!";
                    lblStatus.ForeColor = Color.Green;

                    System.Threading.Thread.Sleep(500);
                    OpenRoleBasedForm(user);
                }
                else
                {
                    lblStatus.Text = "Неверный логин или пароль";
                    lblStatus.ForeColor = Color.Red;
                    txtPassword.Text = "";
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Ошибка: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private User AuthenticateInDatabase(string login, string password)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string query = @"
                SELECT 
                    u.IdUser, 
                    u.UserLogin, 
                    u.UserSurname, 
                    u.UserName, 
                    u.UserPatronymic,
                    u.TelephoneNumber,
                    u.IdRole,
                    r.RoleName
                FROM [User] u
                INNER JOIN [Role] r ON u.IdRole = r.IdRole
                WHERE u.UserLogin = @Login AND u.UserPassword = @Password";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", login);
                        command.Parameters.AddWithValue("@Password", password);



                        using (var reader = command.ExecuteReader())
                        {
                            // ОТЛАДКА: проверяем есть ли строки
                            if (reader.HasRows)
                            {
                                reader.Read();
                                var user = new User
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("IdUser")),
                                    Login = reader.GetString(reader.GetOrdinal("UserLogin")),
                                    Surname = reader.GetString(reader.GetOrdinal("UserSurname")),
                                    Name = reader.GetString(reader.GetOrdinal("UserName")),
                                    Patronymic = reader.IsDBNull(reader.GetOrdinal("UserPatronymic")) ?
                                                string.Empty : reader.GetString(reader.GetOrdinal("UserPatronymic")),
                                    Telephone = reader.IsDBNull(reader.GetOrdinal("TelephoneNumber")) ?
                                                    string.Empty : reader.GetString(reader.GetOrdinal("TelephoneNumber")),
                                    RoleId = reader.GetInt32(reader.GetOrdinal("IdRole")),
                                    RoleName = reader.GetString(reader.GetOrdinal("RoleName"))
                                };

                                
                                return user;
                            }
                            else
                            {
                                
                                return null;
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка базы данных");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                return null;
            }
        }

        private void OpenRoleBasedForm(User user)
        {
            if (user == null)
            {
                MessageBox.Show("Ошибка авторизации", "Ошибка");
                return;
            }

            Form roleForm = null;
            string roleName = user.RoleName.ToLower();

            if (roleName == "администратор" || roleName == "administrator" || roleName == "admin")
            {
                roleForm = new AdminForm(user);
            }
            else if (roleName == "менеджер" || roleName == "manager")
            {
                roleForm = new ManagerForm(user);
            }
            else if (roleName == "мастер-поверитель" || roleName == "master" || roleName == "техник")
            {
                roleForm = new MasterForm(user);
            }
            else if (roleName == "зарегистрированный пользователь" || roleName == "client")
            {
                roleForm = new ClientForm(user);
            }

            if (roleForm != null)
            {
                roleForm.Show();
                this.Hide();
                roleForm.FormClosed += (s, args) => this.Close();
            }
            else
            {
                MessageBox.Show($"Неизвестная роль: {user.RoleName}", "Ошибка");
            }
        }
    }
}




