using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CourseProject
{
    public partial class CreateRequestForm : Form
    {
        private User _currentUser;
        private string connectionString = @"Data Source=localhost;Initial Catalog=CourseProject;User ID=EgorP;Password=ISPP;Encrypt=False";

        public CreateRequestForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void CreateRequestForm_Load(object sender, EventArgs e)
        {
            // Автозаполнение информации о пользователе
            txtAddress.Text = "Введите ваш адрес"; // Можно сделать автозаполнение из профиля
            lblUserInfo.Text = $"{_currentUser.Surname} {_currentUser.Name} {_currentUser.Patronymic}";

            // Загружаем номер телефона пользователя из базы данных
            LoadUserPhoneNumber();
        }

        private void LoadUserPhoneNumber()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT TelephoneNumber FROM [User] WHERE IdUser = @UserId";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserId", _currentUser.Id);

                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        txtPhoneNumber.Text = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки номера телефона: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (ValidateRequest())
            {
                CreateNewRequest();
            }
        }

        private bool ValidateRequest()
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Введите адрес", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAddress.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCounterNumber.Text))
            {
                MessageBox.Show("Введите номер счетчика", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCounterNumber.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
            {
                MessageBox.Show("Введите номер телефона", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhoneNumber.Focus();
                return false;
            }

            // Валидация формата номера телефона (простая проверка)
            if (!IsValidPhoneNumber(txtPhoneNumber.Text))
            {
                MessageBox.Show("Введите корректный номер телефона", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhoneNumber.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Убираем все нецифровые символы для проверки
            string cleanNumber = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d]", "");

            // Проверяем, что остались только цифры и длина от 10 до 15 символов
            return cleanNumber.Length >= 10 && cleanNumber.Length <= 15;
        }

        private void CreateNewRequest()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Обновляем номер телефона пользователя
                    UpdateUserPhoneNumber();

                    string query = @"
                        INSERT INTO Request 
                        (RequestDate, Address, IdUser, CountersNumber, Comment, Master, Status) 
                        VALUES 
                        (@RequestDate, @Address, @UserId, @CounterNumber, @Comment, @Master, @Status)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@RequestDate", DateTime.Now);
                    command.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                    command.Parameters.AddWithValue("@UserId", _currentUser.Id);
                    command.Parameters.AddWithValue("@CounterNumber", txtCounterNumber.Text.Trim());
                    command.Parameters.AddWithValue("@Comment", string.IsNullOrWhiteSpace(txtComment.Text) ? (object)DBNull.Value : txtComment.Text.Trim());
                    command.Parameters.AddWithValue("@Master", (object)DBNull.Value); // Пока нет мастера
                    command.Parameters.AddWithValue("@Status", "Новая");

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Заявка успешно создана!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при создании заявки", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заявки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateUserPhoneNumber()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "UPDATE [User] SET TelephoneNumber = @PhoneNumber WHERE IdUser = @UserId";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@PhoneNumber", txtPhoneNumber.Text.Trim());
                    command.Parameters.AddWithValue("@UserId", _currentUser.Id);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления номера телефона: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtPhoneNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем: цифры, +, -, (, ), пробел и Backspace
            if (!char.IsDigit(e.KeyChar) &&
                e.KeyChar != '+' &&
                e.KeyChar != '-' &&
                e.KeyChar != '(' &&
                e.KeyChar != ')' &&
                e.KeyChar != ' ' &&
                e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }
    }
}