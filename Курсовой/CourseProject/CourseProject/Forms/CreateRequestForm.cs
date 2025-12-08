using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
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
            txtAddress.Text = "Введите адрес";
            txtAddress.ForeColor = Color.Gray;

            // Показываем информацию о том, кто создает заявку
            lblUserInfo.Text = $"Создаёт: {_currentUser.Surname} {_currentUser.Name} {_currentUser.Patronymic} ({GetRoleName()})";

            // Настраиваем поле телефона в зависимости от роли
            if (IsAdminOrManager())
            {
                // Для админа/менеджера показываем пустое поле с подсказкой
                txtPhoneNumber.Text = "Введите телефон клиента";
                txtPhoneNumber.ForeColor = Color.Gray;
                txtPhoneNumber.Tag = "placeholder";
                this.Text = "Создание заявки (от администратора)";
            }
            else
            {
                // Для клиента показываем подсказку с его телефоном
                txtPhoneNumber.Text = "Введите телефон для заявки";
                txtPhoneNumber.ForeColor = Color.Gray;
                txtPhoneNumber.Tag = "placeholder";
                this.Text = "Создание заявки";
            }
        }

        private string GetRoleName()
        {
            switch (_currentUser.RoleId)
            {
                case 1: return "Администратор";
                case 2: return "Менеджер";
                case 3: return "Клиент";
                case 4: return "Мастер";
                default: return "Пользователь";
            }
        }

        private bool IsAdminOrManager()
        {
            return _currentUser.RoleId == 1 || _currentUser.RoleId == 2;
        }

        private void txtPhoneNumber_Enter(object sender, EventArgs e)
        {
            // Очищаем поле если это подсказка
            if (txtPhoneNumber.ForeColor == Color.Gray)
            {
                txtPhoneNumber.Text = "";
                txtPhoneNumber.ForeColor = Color.Black;
                txtPhoneNumber.Tag = null;
            }
        }

        private void txtPhoneNumber_Leave(object sender, EventArgs e)
        {
            // Если поле пустое, возвращаем подсказку
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
            {
                if (IsAdminOrManager())
                {
                    txtPhoneNumber.Text = "Введите телефон клиента";
                }
                else
                {
                    txtPhoneNumber.Text = "Введите телефон для заявки";
                }
                txtPhoneNumber.ForeColor = Color.Gray;
                txtPhoneNumber.Tag = "placeholder";
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
            // Проверка адреса
            if (string.IsNullOrWhiteSpace(txtAddress.Text) || txtAddress.ForeColor == Color.Gray)
            {
                MessageBox.Show("Введите адрес", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAddress.Focus();
                return false;
            }

            // Проверка количества счетчиков
            if (string.IsNullOrWhiteSpace(txtCounterNumber.Text))
            {
                MessageBox.Show("Введите количество счетчиков", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCounterNumber.Focus();
                return false;
            }

            if (!int.TryParse(txtCounterNumber.Text, out int counterNumber) || counterNumber <= 0 || counterNumber>100)
            {
                MessageBox.Show("Введите корректное количество счетчиков", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCounterNumber.Focus();
                return false;
            }

            // Проверка телефона
            string phoneNumber = GetPhoneNumberFromField();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                MessageBox.Show("Введите номер телефона для заявки", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhoneNumber.Focus();
                return false;
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                MessageBox.Show("Введите корректный номер телефона (10-15 цифр)", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhoneNumber.Focus();
                return false;
            }

            return true;
        }

        private string GetPhoneNumberFromField()
        {
            // Если это placeholder (серая подсказка), возвращаем пустую строку
            if (txtPhoneNumber.ForeColor == Color.Gray)
            {
                return "";
            }

            return txtPhoneNumber.Text.Trim();
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

                    // ВАЖНО: Используем ID текущего пользователя (админа/менеджера/клиента)
                    // Кто создает заявку - тот и будет в поле IdUser
                    string query = @"
                        INSERT INTO Request 
                        (RequestDate, Address, IdUser, CountersNumber, Comment, Master, Status, TelephoneNumber) 
                        VALUES 
                        (@RequestDate, @Address, @UserId, @CounterNumber, @Comment, @Master, @Status, @TelephoneNumber)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@RequestDate", DateTime.Now);

                    // Получаем и очищаем адрес
                    string address = txtAddress.ForeColor == Color.Gray ? "" : txtAddress.Text.Trim();
                    command.Parameters.AddWithValue("@Address", address);

                    // IdUser - тот, кто создает заявку
                    command.Parameters.AddWithValue("@UserId", _currentUser.Id);

                    command.Parameters.AddWithValue("@CounterNumber", int.Parse(txtCounterNumber.Text.Trim()));

                    string comment = string.IsNullOrWhiteSpace(txtComment.Text) ? "" : txtComment.Text.Trim();
                    command.Parameters.AddWithValue("@Comment", comment);

                    command.Parameters.AddWithValue("@Master", (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", "Новая");

                    // ВАЖНО: Получаем телефон ИЗ ПОЛЯ ВВОДА, а не из профиля пользователя
                    string phoneNumber = GetPhoneNumberFromField();
                    phoneNumber = CleanPhoneNumber(phoneNumber);

                    command.Parameters.AddWithValue("@TelephoneNumber", phoneNumber);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        string message = IsAdminOrManager()
                            ? $"Заявка успешно создана!"
                            : "Заявка успешно создана!";

                        MessageBox.Show(message, "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при создании заявки", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заявки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return "";

            // Оставляем только цифры и знак + в начале
            if (phoneNumber.StartsWith("+"))
            {
                string digits = new string(phoneNumber.Substring(1).Where(char.IsDigit).ToArray());
                return "+" + digits;
            }
            else
            {
                return new string(phoneNumber.Where(char.IsDigit).ToArray());
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
                e.KeyChar != '(' &&
                e.KeyChar != ')' &&
                e.KeyChar != ' ' &&
                e.KeyChar != '-' &&
                e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }

            // Проверяем, что + только в начале
            if (e.KeyChar == '+')
            {
                TextBox textBox = sender as TextBox;
                if (textBox.SelectionStart > 0)
                {
                    e.Handled = true;
                }
            }
        }

        private void txtCounterNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры и Backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void txtAddress_Enter(object sender, EventArgs e)
        {
            // Очищаем placeholder при фокусе
            if (txtAddress.ForeColor == Color.Gray)
            {
                txtAddress.Text = "";
                txtAddress.ForeColor = Color.Black;
            }
        }

        private void txtAddress_Leave(object sender, EventArgs e)
        {
            // Возвращаем placeholder если поле пустое
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                txtAddress.Text = "Введите адрес";
                txtAddress.ForeColor = Color.Gray;
            }
        }
    }
}