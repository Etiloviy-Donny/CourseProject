using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace CourseProject
{
    public partial class ImportPreviewForm : Form
    {
        private DataTable _importedData;
        private string _connectionString;
        private int _currentUserId; // Добавляем поле для ID пользователя

        // Конструктор с 2 параметрами (для совместимости)
        public ImportPreviewForm(DataTable importedData, string connectionString)
        {
            InitializeComponent();
            _importedData = importedData;
            _connectionString = connectionString;
            _currentUserId = 0; // По умолчанию 0 (гость)

            TestDatabaseConnection();
        }

        // Конструктор с 3 параметрами (если нужно передать ID пользователя)
        public ImportPreviewForm(DataTable importedData, string connectionString, int userId)
        {
            InitializeComponent();
            _importedData = importedData;
            _connectionString = connectionString;
            _currentUserId = userId;

            TestDatabaseConnection();
        }

        private void ImportPreviewForm_Load(object sender, EventArgs e)
        {
            dataGridViewPreview.DataSource = _importedData;
            lblRecordCount.Text = $"Записей: {_importedData.Rows.Count}";
            lblColumnsCount.Text = $"Столбцов: {_importedData.Columns.Count}";
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateDataStructure(_importedData))
                {
                    return;
                }

                int savedCount = SaveRequestsToDatabase(_importedData);

                if (savedCount > 0)
                {
                    MessageBox.Show($"Успешно сохранено {savedCount} заявок в базу данных",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить заявки в базу данных",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в БД: {ex.Message}\n\n{ex.StackTrace}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateDataStructure(DataTable dataTable)
        {
            // Проверяем только обязательные поля: адрес и количество счетчиков
            // Клиент не требуется
            bool hasAddress = false;
            bool hasCounters = false;

            foreach (DataColumn column in dataTable.Columns)
            {
                string columnName = column.ColumnName;

                if (columnName.IndexOf("адрес", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("address", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    hasAddress = true;
                }

                if (columnName.IndexOf("счетчик", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("количество", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("counters", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("number", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    hasCounters = true;
                }
            }

            if (!hasAddress)
            {
                MessageBox.Show($"В файле отсутствует колонка с адресом.\n" +
                               $"Ожидаемые названия: 'Адрес' или 'Address'",
                    "Ошибка структуры", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!hasCounters)
            {
                MessageBox.Show($"В файле отсутствует колонка с количеством счетчиков.\n" +
                               $"Ожидаемые названия: 'Количество счетчиков', 'CountersNumber' или 'Counters'",
                    "Ошибка структуры", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private int SaveRequestsToDatabase(DataTable dataTable)
        {
            int savedCount = 0;
            int skippedCount = 0;
            int errorCount = 0;
            List<string> errorMessages = new List<string>();
            List<string> skipMessages = new List<string>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения к БД: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        DataRow row = dataTable.Rows[i];

                        try
                        {
                            if (ShouldSkipRow(row))
                            {
                                skippedCount++;
                                skipMessages.Add($"Строка {i + 1}: Пропущена - отсутствует адрес");
                                continue;
                            }

                            string clientName = GetClientName(row);
                            string phone = GetPhone(row);
                            string address = GetAddress(row);
                            int counters = GetCountersNumber(row);
                            string comment = GetComment(row);
                            string master = GetMaster(row);
                            string status = GetStatus(row);
                            DateTime requestDate = GetRequestDate(row);

                            if (string.IsNullOrWhiteSpace(address))
                            {
                                throw new Exception("Адрес не указан");
                            }

                            if (counters <= 0)
                            {
                                throw new Exception("Некорректное количество счетчиков");
                            }

                            int userId = DetermineUserId(connection, transaction, clientName, phone);

                            // ИСПРАВЛЕННЫЙ ЗАПРОС: Добавляем TelephoneNumber
                            string insertQuery = @"
                        INSERT INTO Request 
                        (IdUser, RequestDate, Address, CountersNumber, Comment, Master, Status, TelephoneNumber) 
                        VALUES 
                        (@IdUser, @RequestDate, @Address, @CountersNumber, @Comment, @Master, @Status, @TelephoneNumber)";

                            using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@IdUser", userId);
                                command.Parameters.AddWithValue("@RequestDate", requestDate);
                                command.Parameters.AddWithValue("@Address", address);
                                command.Parameters.AddWithValue("@CountersNumber", counters);
                                command.Parameters.AddWithValue("@Comment",
                                    string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment);
                                command.Parameters.AddWithValue("@Master",
                                    string.IsNullOrEmpty(master) ? (object)DBNull.Value : master);
                                command.Parameters.AddWithValue("@Status",
                                    string.IsNullOrEmpty(status) ? "На рассмотрении" : status);

                                // ВАЖНО: Добавляем телефон, используем DBNull.Value если телефон пустой
                                object phoneValue = string.IsNullOrWhiteSpace(phone) ?
                                    (object)DBNull.Value : phone;
                                command.Parameters.AddWithValue("@TelephoneNumber", phoneValue);

                                command.ExecuteNonQuery();
                                savedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorMessages.Add($"Строка {i + 1}: {ex.Message}");
                        }
                    }

                    if (savedCount > 0)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Ошибка транзакции: {ex.Message}", ex);
                }
            }

            ShowDetailedReport(savedCount, skippedCount, errorCount, skipMessages, errorMessages);

            return savedCount;
        }

        private int DetermineUserId(SqlConnection connection, SqlTransaction transaction, string clientName, string phone)
        {
            // 1. Если есть информация о клиенте, пытаемся найти/создать пользователя
            if (!string.IsNullOrWhiteSpace(clientName) || !string.IsNullOrWhiteSpace(phone))
            {
                int userId = GetOrCreateUser(connection, transaction, clientName, phone);
                if (userId > 0)
                {
                    return userId;
                }
            }

            // 2. Если нет информации о клиенте, используем текущего пользователя (если передан)
            if (_currentUserId > 0)
            {
                return _currentUserId;
            }

            // 3. Иначе используем гостя
            return GetGuestUserId(connection, transaction);
        }

        private bool ShouldSkipRow(DataRow row)
        {
            // Требуем только адрес
            bool hasAddress = !string.IsNullOrWhiteSpace(GetAddress(row));

            return !hasAddress;
        }

        private void ShowDetailedReport(int saved, int skipped, int errors, List<string> skips, List<string> errorList)
        {
            string report = $"Отчет об импорте:\n\n" +
                           $"✅ Успешно сохранено: {saved}\n" +
                           $"⏭️ Пропущено (отсутствует адрес): {skipped}\n" +
                           $"❌ С ошибками: {errors}\n" +
                           $"Всего строк в файле: {saved + skipped + errors}\n";

            if (skipped > 0 && skips.Count > 0)
            {
                report += $"\nПропущенные строки:\n";
                foreach (var msg in skips.Take(3))
                    report += $"  {msg}\n";
                if (skips.Count > 3)
                    report += $"  ... и еще {skips.Count - 3} строк\n";
            }

            if (errors > 0 && errorList.Count > 0)
            {
                report += $"\nОшибки:\n";
                foreach (var msg in errorList.Take(3))
                    report += $"  {msg}\n";
                if (errorList.Count > 3)
                    report += $"  ... и еще {errorList.Count - 3} ошибок\n";
            }

            MessageBox.Show(report, "Подробный отчет", MessageBoxButtons.OK,
                errors == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        // Методы получения данных из строки
        private string GetClientName(DataRow row)
        {
            // Опциональный поиск клиента
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;
                if (columnName.IndexOf("клиент", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("client", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("фио", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    object value = row[columnName];
                    if (value != null && value != DBNull.Value)
                        return value.ToString().Trim();
                }
            }
            return "";
        }

        private string GetPhone(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;
                if (columnName.IndexOf("телефон", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("phone", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("номер", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    object value = row[columnName];
                    if (value != null && value != DBNull.Value)
                        return value.ToString().Trim();
                }
            }
            return "";
        }

        private string GetAddress(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;
                if (columnName.IndexOf("адрес", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("address", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    object value = row[columnName];
                    if (value != null && value != DBNull.Value)
                        return value.ToString().Trim();
                }
            }
            return "";
        }

        private int GetCountersNumber(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;
                if (columnName.IndexOf("счетчик", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("количество", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("counters", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("number", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    object value = row[columnName];
                    if (value != null && value != DBNull.Value)
                    {
                        if (int.TryParse(value.ToString(), out int result) && result > 0)
                        {
                            return result;
                        }
                    }
                }
            }
            return 1;
        }

        private string GetComment(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;
                if (columnName.IndexOf("комментарий", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("comment", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("примечание", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    object value = row[columnName];
                    if (value != null && value != DBNull.Value)
                        return value.ToString().Trim();
                }
            }
            return "";
        }

        private string GetMaster(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;
                if (columnName.IndexOf("мастер", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("master", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("исполнитель", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    object value = row[columnName];
                    if (value != null && value != DBNull.Value)
                        return value.ToString().Trim();
                }
            }
            return "";
        }

        private string GetStatus(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;
                if (columnName.IndexOf("статус", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("status", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("состояние", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    object value = row[columnName];
                    if (value != null && value != DBNull.Value)
                        return value.ToString().Trim();
                }
            }
            return "На рассмотрении";
        }

        private DateTime GetRequestDate(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;
                if (columnName.IndexOf("дата", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    columnName.IndexOf("date", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    object value = row[columnName];
                    if (value != null && value != DBNull.Value)
                    {
                        if (DateTime.TryParse(value.ToString(), out DateTime result))
                        {
                            return result;
                        }
                    }
                }
            }
            return DateTime.Now;
        }

        private int GetOrCreateUser(SqlConnection connection, SqlTransaction transaction, string clientName, string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientName))
                {
                    clientName = "--";
                }

                string normalizedPhone = NormalizePhone(phone);

                // Если телефон не указан, используем NULL или пустую строку
                object phoneValue = string.IsNullOrEmpty(normalizedPhone) ?
                    (object)DBNull.Value : normalizedPhone;

                if (!string.IsNullOrEmpty(normalizedPhone))
                {
                    string findUserByPhoneQuery = @"
                SELECT IdUser FROM [User] 
                WHERE (TelephoneNumber = @Phone OR 
                       TelephoneNumber = @PhoneWithoutPlus OR
                       REPLACE(REPLACE(REPLACE(TelephoneNumber, ' ', ''), '-', ''), '(', '') = @CleanedPhone)";

                    using (SqlCommand command = new SqlCommand(findUserByPhoneQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Phone", phoneValue);
                        command.Parameters.AddWithValue("@PhoneWithoutPlus", normalizedPhone.TrimStart('+'));
                        command.Parameters.AddWithValue("@CleanedPhone", CleanPhoneNumber(normalizedPhone));

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }

                string[] nameParts = clientName.Split(new[] { ' ', '\t', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 2)
                {
                    string surname = nameParts[0];
                    string name = nameParts[1];
                    string patronymic = nameParts.Length > 2 ? nameParts[2] : "";

                    string findUserByNameQuery = @"
                SELECT IdUser FROM [User] 
                WHERE UserSurname LIKE @Surname + '%' 
                AND UserName LIKE @Name + '%' 
                AND (@Patronymic IS NULL OR UserPatronymic LIKE @Patronymic + '%' OR @Patronymic = '')";

                    using (SqlCommand command = new SqlCommand(findUserByNameQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Surname", surname);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Patronymic", string.IsNullOrEmpty(patronymic) ? (object)DBNull.Value : patronymic);

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }

                // Создаем нового пользователя
                string surnameToCreate = nameParts.Length > 0 ? nameParts[0] : "";
                string nameToCreate = nameParts.Length > 1 ? nameParts[1] : "";
                string patronymicToCreate = nameParts.Length > 2 ? nameParts[2] : "";

                string login = GenerateUniqueLogin(connection, transaction, surnameToCreate, nameToCreate);

                // ИСПРАВЛЕННЫЙ ЗАПРОС: Указываем NULL для TelephoneNumber если телефон не указан
                string insertUserQuery = @"
            INSERT INTO [User] 
            (UserSurname, UserName, UserPatronymic, TelephoneNumber, IdRole, UserLogin, UserPassword) 
            VALUES 
            (@Surname, @Name, @Patronymic, @Phone, 2, @Login, @Password);
            SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new SqlCommand(insertUserQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@Surname", surnameToCreate);
                    command.Parameters.AddWithValue("@Name", nameToCreate);
                    command.Parameters.AddWithValue("@Patronymic",
                        string.IsNullOrEmpty(patronymicToCreate) ? (object)DBNull.Value : patronymicToCreate);
                    command.Parameters.AddWithValue("@Phone", phoneValue); // Используем DBNull.Value если телефон пустой
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", HashPassword("password123"));

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания пользователя: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }


        private string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "";

            phone = phone.Trim();
            if (phone.StartsWith("+"))
            {
                string digits = new string(phone.Substring(1).Where(char.IsDigit).ToArray());
                return "+" + digits;
            }
            else
            {
                return new string(phone.Where(char.IsDigit).ToArray());
            }
        }

        private string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "";

            return new string(phone.Where(char.IsDigit).ToArray());
        }

        private string GenerateUniqueLogin(SqlConnection connection, SqlTransaction transaction, string surname, string name)
        {
            string baseLogin = $"{surname.ToLower()}.{name.ToLower()}";
            string login = baseLogin;
            int counter = 1;

            while (IsLoginExists(connection, transaction, login))
            {
                login = $"{baseLogin}{counter}";
                counter++;
            }

            return login;
        }

        private bool IsLoginExists(SqlConnection connection, SqlTransaction transaction, string login)
        {
            string query = "SELECT COUNT(1) FROM [User] WHERE UserLogin = @Login";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@Login", login);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        private int GetGuestUserId(SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                string findGuestQuery = "SELECT IdUser FROM [User] WHERE UserLogin = 'guest.user'";
                using (SqlCommand command = new SqlCommand(findGuestQuery, connection, transaction))
                {
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }

                // ИСПРАВЛЕННЫЙ ЗАПРОС: Указываем NULL для TelephoneNumber
                string createGuestQuery = @"
            INSERT INTO [User] 
            (UserSurname, UserName, UserPatronymic, TelephoneNumber, IdRole, UserLogin, UserPassword) 
            VALUES 
            ('Гость', 'Пользователь', '', NULL, 2, 'guest.user', @Password);
            SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new SqlCommand(createGuestQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@Password", HashPassword("guest123"));
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания гостевого пользователя: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void TestDatabaseConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string checkTablesQuery = @"
                SELECT 
                    CASE WHEN EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Request') THEN 1 ELSE 0 END as HasRequestTable,
                    CASE WHEN EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'User') THEN 1 ELSE 0 END as HasUserTable";

                    using (SqlCommand command = new SqlCommand(checkTablesQuery, connection));

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка тестирования БД: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}