using CourseProject;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using static CourseProject.AuthorizationForm;

namespace CourseProject
{
    public partial class MasterForm : Form
    {
        private User _currentUser;
        private string connectionString = @"Data Source=localhost;Initial Catalog=CourseProject;User ID=EgorP;Password=ISPP;Encrypt=False";
        private DataTable _allRequestsDataTable;

        public MasterForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void MasterForm_Load(object sender, EventArgs e)
        {
            LoadMasterRequests();
            LoadStatuses();
            LoadSearchOptions();
        }

        private void LoadMasterRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Формируем полное имя мастера как в базе данных
                    string masterFullName = $"{_currentUser.Surname} {_currentUser.Name} {_currentUser.Patronymic}".Trim();

                    // Используем запрос для получения только заявок назначенных на этого мастера
                    string query = @"
                SELECT 
                    r.IdRequest,
                    r.RequestDate,
                    r.Address,
                    u.UserSurname + ' ' + u.UserName as ClientName,
                    r.TelephoneNumber,
                    r.CountersNumber,
                    r.Comment,
                    ISNULL(r.Master, 'Не назначен') as Master,
                    r.Status
                FROM Request r
                INNER JOIN [User] u ON r.IdUser = u.IdUser
                WHERE r.Master = @MasterName
                ORDER BY r.RequestDate DESC";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@MasterName", masterFullName);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    _allRequestsDataTable = new DataTable();
                    adapter.Fill(_allRequestsDataTable);

                    dataGridViewRequests.DataSource = _allRequestsDataTable;
                    ConfigureDataGridView();

                    // Показываем количество заявок
                    int requestCount = _allRequestsDataTable.Rows.Count;
                    this.Text = $"Панель мастера - Заявок: {requestCount}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSearchOptions()
        {
            cmbSearchBy.SelectedIndex = 0;
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
                dataGridViewRequests.Columns["Master"].Visible = false; // Скрываем колонку Мастер
                dataGridViewRequests.Columns["Status"].HeaderText = "Статус";
                dataGridViewRequests.Columns["Status"].Width = 120;
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

                string status = row.Cells["Status"].Value?.ToString() ?? "";
                cmbStatus.Text = !string.IsNullOrEmpty(status) ? status : "";

                btnUpdate.Enabled = true;
            }
            else
            {
                btnUpdate.Enabled = false;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewRequests.SelectedRows.Count == 0)
                    throw new InvalidOperationException("Не выбрана заявка для обновления");

                UpdateRequest();
                MessageBox.Show("Заявка успешно обновлена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMasterRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateRequest()
        {
            int requestId = (int)dataGridViewRequests.SelectedRows[0].Cells["IdRequest"].Value;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(@"
                UPDATE Request 
                SET Status = @Status,
                    Address = @Address,
                    CountersNumber = @CountersNumber,
                    Comment = @Comment
                WHERE IdRequest = @RequestId", connection))
            {
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadMasterRequests();
            txtSearch.Text = "";
            int requestCount = _allRequestsDataTable?.Rows.Count ?? 0;
            this.Text = $"Панель мастера - Заявок: {requestCount}";
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

        private void txtCounterNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // ===== ПОИСК ЗАЯВОК =====
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dataGridViewRequests.DataSource = _allRequestsDataTable;
                this.Text = "Панель мастера";
            }
            else
            {
                ApplySearchFilter();
            }
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
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
                int requestCount = _allRequestsDataTable.Rows.Count;
                this.Text = $"Панель мастера - Заявок: {requestCount}";
                return;
            }

            string searchBy = cmbSearchBy.SelectedItem?.ToString() ?? "Все поля";
            string filterExpression = "";

            // Экранируем специальные символы для DataView RowFilter
            string escapedSearchText = EscapeRowFilterValue(searchText);

            switch (searchBy)
            {
                case "Все поля":
                    // Для мастера убираем поле Master из поиска (все заявки назначены на него)
                    string textSearchFilter = string.Format(
                        "(Address LIKE '*{0}*') OR " +
                        "(ClientName LIKE '*{0}*') OR " +
                        "(Status LIKE '*{0}*') OR " +
                        "(Comment LIKE '*{0}*') OR " +
                        "(TelephoneNumber LIKE '*{0}*')",
                        escapedSearchText);

                    // Проверяем, можно ли искать как номер заявки
                    if (int.TryParse(searchText, out int requestId))
                    {
                        // Если ввели число, ищем и по текстовым полям, и по номеру заявки
                        filterExpression = string.Format("({0}) OR (IdRequest = {1})", textSearchFilter, requestId);
                    }
                    else
                    {
                        // Если не число, ищем только по текстовым полям
                        filterExpression = textSearchFilter;
                    }
                    break;

                case "Адрес":
                    filterExpression = string.Format("Address LIKE '*{0}*'", escapedSearchText);
                    break;

                case "Клиент":
                    filterExpression = string.Format("ClientName LIKE '*{0}*'", escapedSearchText);
                    break;

                case "Статус":
                    filterExpression = string.Format("Status LIKE '*{0}*'", escapedSearchText);
                    break;

                case "Номер заявки":
                    if (int.TryParse(searchText, out int searchRequestId))
                    {
                        filterExpression = string.Format("IdRequest = {0}", searchRequestId);
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
                    this.Text = $"Панель мастера - Найдено: {foundCount} заявок";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string EscapeRowFilterValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            // Экранируем специальные символы для DataView RowFilter
            value = value.Replace("[", "[[]");
            value = value.Replace("]", "[]]");
            value = value.Replace("*", "[*]");
            value = value.Replace("?", "[?]");
            value = value.Replace("#", "[#]");
            value = value.Replace("'", "''");
            value = value.Replace("\"", "\"\"");

            return value;
        }

        private void MasterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
        // Экспорт данных
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_allRequestsDataTable == null || _allRequestsDataTable.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ExcelHelper.ExportToExcel(_allRequestsDataTable, $"Заявки_мастер_{DateTime.Now:yyyyMMdd}");
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}