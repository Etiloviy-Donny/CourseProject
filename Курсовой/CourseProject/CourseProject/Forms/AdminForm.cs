using CourseProject;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using static CourseProject.AuthorizationForm;

namespace CourseProject
{
    public partial class AdminForm : Form
    {
        private User _currentUser;
        private string connectionString = @"Data Source=localhost;Initial Catalog=CourseProject;User ID=EgorP;Password=ISPP;Encrypt=False";
        private DataTable _allRequestsDataTable;

        public AdminForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            LoadAllRequests();
            LoadStatuses();
            LoadMastersFromDB();
            LoadSearchOptions();
        }

        private void LoadAllRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                SELECT 
                    r.IdRequest,
                    r.RequestDate,
                    r.Address,
                    u.UserSurname + ' ' + u.UserName as ClientName,
                    u.TelephoneNumber,
                    r.CountersNumber,
                    r.Comment,
                    ISNULL(r.Master, 'Не назначен') as Master,
                    r.Status
                FROM Request r
                INNER JOIN [User] u ON r.IdUser = u.IdUser
                ORDER BY r.RequestDate DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    _allRequestsDataTable = new DataTable();
                    adapter.Fill(_allRequestsDataTable);

                    dataGridViewRequests.DataSource = _allRequestsDataTable;
                    ConfigureDataGridView();

                    // Сбрасываем заголовок
                    this.Text = "Панель администратора";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSearchOptions()
        {
            cmbSearchBy.SelectedIndex = 0; // Выбираем "Все поля" по умолчанию
        }



        private void LoadMastersFromDB()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            IdUser,
                            UserSurname + ' ' + UserName + 
                            ISNULL(' ' + UserPatronymic, '') as Master
                        FROM [User] 
                        WHERE IdRole = 3
                        ORDER BY UserSurname, UserName";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    cmbMaster.Items.Clear();
                    cmbMaster.Items.Add(""); // Пустое значение для сброса назначения

                    while (reader.Read())
                    {
                        cmbMaster.Items.Add(reader["Master"].ToString());
                    }
                    reader.Close();

                    if (cmbMaster.Items.Count == 1)
                    {
                        MessageBox.Show("В системе не найдено пользователей с ролью мастера (IdRole = 3).",
                                      "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мастеров из базы данных: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureDataGridView()
        {
            dataGridViewRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewRequests.ReadOnly = true;
            dataGridViewRequests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRequests.AllowUserToAddRows = false;
            dataGridViewRequests.AllowUserToDeleteRows = false;

            if (dataGridViewRequests.Columns.Count > 0)
            {
                dataGridViewRequests.Columns["IdRequest"].HeaderText = "№";
                dataGridViewRequests.Columns["IdRequest"].Width = 50;
                dataGridViewRequests.Columns["RequestDate"].HeaderText = "Дата создания";
                dataGridViewRequests.Columns["RequestDate"].Width = 120;
                dataGridViewRequests.Columns["Address"].HeaderText = "Адрес";
                dataGridViewRequests.Columns["ClientName"].HeaderText = "Клиент";
                dataGridViewRequests.Columns["ClientName"].Width = 150;
                dataGridViewRequests.Columns["TelephoneNumber"].HeaderText = "Телефон";
                dataGridViewRequests.Columns["TelephoneNumber"].Width = 100;
                dataGridViewRequests.Columns["CountersNumber"].HeaderText = "Кол-во счетчиков";
                dataGridViewRequests.Columns["CountersNumber"].Width = 120;
                dataGridViewRequests.Columns["Comment"].HeaderText = "Комментарий";
                dataGridViewRequests.Columns["Master"].HeaderText = "Мастер";
                dataGridViewRequests.Columns["Master"].Width = 120;
                dataGridViewRequests.Columns["Status"].HeaderText = "Статус";
                dataGridViewRequests.Columns["Status"].Width = 120;
            }
        }

        private void btnCreateUser_Click(object sender, EventArgs e)
        {
            OpenAddUserForm();
        }

        private void OpenAddUserForm()
        {
            try
            {
                AddUserForm addUserForm = new AddUserForm();
                addUserForm.FormClosed += (s, args) =>
                {
                    if (addUserForm.DialogResult == DialogResult.OK)
                    {
                        LoadMastersFromDB();
                        MessageBox.Show("Список пользователей обновлен!", "Информация",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };
                addUserForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы создания пользователя: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatuses()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.AddRange(new string[] {
                "На рассмотрении",
                "В работе",
                "Выполнена",
                "Отклонена"
            });
        }

        private void dataGridViewRequests_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewRequests.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewRequests.SelectedRows[0];

                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";
                txtCounterNumber.Text = row.Cells["CountersNumber"].Value?.ToString() ?? "";
                txtComment.Text = row.Cells["Comment"].Value?.ToString() ?? "";

                string master = row.Cells["Master"].Value?.ToString() ?? "";
                string status = row.Cells["Status"].Value?.ToString() ?? "";

                if (!string.IsNullOrEmpty(master) && master != "Не назначен")
                {
                    cmbMaster.Text = master;
                }
                else
                {
                    cmbMaster.Text = "";
                }

                if (!string.IsNullOrEmpty(status))
                {
                    cmbStatus.Text = status;
                }
                else
                {
                    cmbStatus.Text = "";
                }

                btnUpdate.Enabled = true;
                btnDeleteRequest.Enabled = true;
            }
            else
            {
                btnUpdate.Enabled = false;
                btnDeleteRequest.Enabled = false;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateInput();

                if (dataGridViewRequests.SelectedRows.Count == 0)
                    throw new InvalidOperationException("Не выбрана заявка для обновления");

                UpdateRequest();
                MessageBox.Show("Заявка успешно обновлена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
                throw new ArgumentException("Заполните адрес");

            if (string.IsNullOrWhiteSpace(txtCounterNumber.Text))
                throw new ArgumentException("Заполните количество счетчиков");

            if (!int.TryParse(txtCounterNumber.Text, out int counterCount) || counterCount <= 0)
                throw new ArgumentException("Введите корректное количество счетчиков");


            if (string.IsNullOrWhiteSpace(cmbStatus.Text))
                throw new ArgumentException("Выберите статус заявки");
        }

        private void UpdateRequest()
        {
            int requestId = (int)dataGridViewRequests.SelectedRows[0].Cells["IdRequest"].Value;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(@"
                UPDATE Request 
                SET Master = @Master, 
                    Status = @Status,
                    Address = @Address,
                    CountersNumber = @CountersNumber,
                    Comment = @Comment
                WHERE IdRequest = @RequestId", connection))
            {
                command.Parameters.Add("@Master", SqlDbType.NVarChar, 100).Value =
                    GetMasterValue();
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = cmbStatus.Text.Trim();
                command.Parameters.Add("@Address", SqlDbType.NVarChar, 200).Value = txtAddress.Text.Trim();
                command.Parameters.Add("@CountersNumber", SqlDbType.Int).Value =
                    int.Parse(txtCounterNumber.Text);
                command.Parameters.Add("@Comment", SqlDbType.NVarChar, 500).Value =
                    string.IsNullOrWhiteSpace(txtComment.Text) ? (object)DBNull.Value : txtComment.Text.Trim();
                command.Parameters.Add("@RequestId", SqlDbType.Int).Value = requestId;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private object GetMasterValue()
        {
            return string.IsNullOrWhiteSpace(cmbMaster.Text) || cmbMaster.Text == "Не назначен"
                ? (object)DBNull.Value
                : cmbMaster.Text.Trim();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllRequests();
            txtSearch.Text = ""; // Сбрасываем поиск
            this.Text = "Панель администратора"; // Сбрасываем заголовок
        }

        private void dataGridViewRequests_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewRequests.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString().ToLower();

                if (status.Contains("выполнен") || status.Contains("готов"))
                    e.CellStyle.BackColor = Color.LightGreen;
                else if (status.Contains("в работ"))
                    e.CellStyle.BackColor = Color.LightYellow;
                else if (status.Contains("отклон"))
                    e.CellStyle.BackColor = Color.LightCoral;
                else if (status.Contains("рассмотрени"))
                    e.CellStyle.BackColor = Color.LightBlue;
                else
                    e.CellStyle.BackColor = Color.White;
            }
        }

        private void btnClearSelection_Click(object sender, EventArgs e)
        {
            dataGridViewRequests.ClearSelection();
            ClearForm();
        }

        private void ClearForm()
        {
            txtAddress.Text = "";
            txtCounterNumber.Text = "";
            txtComment.Text = "";
            cmbMaster.Text = "";
            cmbStatus.Text = "";
            btnUpdate.Enabled = false;
        }

        private void txtCounterNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void AdminForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
        private void btnAddRequest_Click(object sender, EventArgs e)
        {
            try
            {
                CreateRequestForm createRequestForm = new CreateRequestForm(_currentUser);
                createRequestForm.ShowDialog();
                // Обновляем список после закрытия формы создания
                LoadAllRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы создания заявки: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteRequest_Click(object sender, EventArgs e)
        {
            if (dataGridViewRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заявку для удаления", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную заявку?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int requestId = (int)dataGridViewRequests.SelectedRows[0].Cells["IdRequest"].Value;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    using (SqlCommand command = new SqlCommand("DELETE FROM Request WHERE IdRequest = @RequestId", connection))
                    {
                        command.Parameters.Add("@RequestId", SqlDbType.Int).Value = requestId;

                        connection.Open();
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Заявка успешно удалена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении заявки: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            ApplySearchFilter();
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Поиск при нажатии Enter
            if (e.KeyChar == (char)Keys.Enter)
            {
                ApplySearchFilter();
                e.Handled = true;
            }
        }

        private void ApplySearchFilter()
        {
            if (_allRequestsDataTable == null) return;

            string searchText = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                dataGridViewRequests.DataSource = _allRequestsDataTable;
                return;
            }

            string searchBy = cmbSearchBy.SelectedItem?.ToString() ?? "Все поля";
            string filterExpression = "";

            // Экранируем специальные символы для DataView RowFilter
            string escapedSearchText = EscapeRowFilterValue(searchText);

            switch (searchBy)
            {
                case "Все поля":
                    // Для числового поля IdRequest используем Convert, чтобы преобразовать в строку
                    // Для поиска по номеру заявки как тексту
                    filterExpression = string.Format(
                        "(ClientName LIKE '*{0}*') OR " +
                        "(Address LIKE '*{0}*') OR " +
                        "(Master LIKE '*{0}*') OR " +
                        "(Status LIKE '*{0}*') OR " +
                        "(Comment LIKE '*{0}*') OR " +
                        "(TelephoneNumber LIKE '*{0}*') OR " +
                        "(CONVERT(IdRequest, 'System.String') LIKE '*{0}*')",
                        escapedSearchText);
                    break;

                case "Клиент":
                    filterExpression = string.Format("ClientName LIKE '*{0}*'", escapedSearchText);
                    break;

                case "Адрес":
                    filterExpression = string.Format("Address LIKE '*{0}*'", escapedSearchText);
                    break;

                case "Мастер":
                    filterExpression = string.Format("Master LIKE '*{0}*'", escapedSearchText);
                    break;

                case "Статус":
                    filterExpression = string.Format("Status LIKE '*{0}*'", escapedSearchText);
                    break;

                case "Номер заявки":
                    // Для точного поиска по номеру заявки
                    if (int.TryParse(searchText, out int requestId))
                    {
                        // Можно искать как точное число
                        filterExpression = string.Format("IdRequest = {0}", requestId);
                        // ИЛИ как текст (если хотите частичное совпадение):
                        // filterExpression = string.Format("CONVERT(IdRequest, 'System.String') LIKE '*{0}*'", escapedSearchText);
                    }
                    else
                    {
                        MessageBox.Show("Введите корректный номер заявки (только цифры)", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    break;
            }

            try
            {
                DataView filteredView = new DataView(_allRequestsDataTable);
                filteredView.RowFilter = filterExpression;

                dataGridViewRequests.DataSource = filteredView;

                int foundCount = filteredView.Count;
                if (foundCount == 0)
                {
                    MessageBox.Show("Заявки не найдены", "Результат поиска",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    this.Text = $"Панель менеджера - Найдено: {foundCount} заявок";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}\nФильтр: {filterExpression}\n\nПопробуйте ввести поисковый запрос без специальных символов.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string EscapeRowFilterValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            // Экранируем специальные символы для DataView RowFilter
            // DataView использует * для любого количества символов
            // и ? для одного символа
            value = value.Replace("[", "[[]");
            value = value.Replace("]", "[]]");
            value = value.Replace("*", "[*]");
            value = value.Replace("?", "[?]");
            value = value.Replace("#", "[#]");
            value = value.Replace("'", "''");
            value = value.Replace("\"", "\"\"");

            return value;
        }
    
         private void btnExport_Click(object sender, EventArgs e)
         {
            if (_allRequestsDataTable == null || _allRequestsDataTable.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Предлагаем выбор формата
            using (var dialog = new Form())
            {
                dialog.Text = "Выберите формат экспорта";
                dialog.Size = new Size(300, 150);
                dialog.StartPosition = FormStartPosition.CenterParent;

                var btnExcel = new Button { Text = "Excel (.xlsx)", Location = new Point(50, 20), Size = new Size(200, 30) };
                var btnCSV = new Button { Text = "CSV (.csv)", Location = new Point(50, 60), Size = new Size(200, 30) };

                btnExcel.Click += (s, args) =>
                {
                    dialog.DialogResult = DialogResult.Yes;
                    ExcelHelper.ExportToExcel(_allRequestsDataTable, $"Заявки_админ_{DateTime.Now:yyyyMMdd}");
                    dialog.Close();
                };

                btnCSV.Click += (s, args) =>
                {
                    dialog.DialogResult = DialogResult.No;
                    ExcelHelper.ExportToCSV(_allRequestsDataTable, $"Заявки_админ_{DateTime.Now:yyyyMMdd}");
                    dialog.Close();
                };

                dialog.Controls.Add(btnExcel);
                dialog.Controls.Add(btnCSV);
                dialog.ShowDialog();
            }
        }

        // Импорт данных
        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                var importedData = ExcelHelper.ImportFromExcel();

                if (importedData != null && importedData.Rows.Count > 0)
                {
                    // Передаем ID текущего администратора
                    ImportPreviewForm previewForm = new ImportPreviewForm(importedData, connectionString);

                    if (previewForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadAllRequests();
                        MessageBox.Show("Импорт завершен успешно!",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
