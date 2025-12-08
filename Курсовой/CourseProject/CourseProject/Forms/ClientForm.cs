using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;
using static CourseProject.AuthorizationForm;

namespace CourseProject
{
    public partial class ClientForm : Form
    {
        private User _currentUser;
        private string connectionString = @"Data Source=localhost;Initial Catalog=CourseProject;User ID=EgorP;Password=ISPP;Encrypt=False";
        private DataTable _allRequestsDataTable;

        public ClientForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            LoadUserRequests();
            DisplayUserInfo();
            LoadSearchOptions();
        }

        private void DisplayUserInfo()
        {
            lblWelcome.Text = $"Добро пожаловать, {_currentUser.Surname} {_currentUser.Name}";
            if (!string.IsNullOrEmpty(_currentUser.Patronymic))
                lblWelcome.Text += $" {_currentUser.Patronymic}";
        }

        private void LoadUserRequests()
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
                            r.TelephoneNumber,
                            r.CountersNumber,
                            r.Comment,
                            ISNULL(r.Master, 'Не назначен') as Master,
                            r.Status
                        FROM Request r
                        INNER JOIN [User] u ON r.IdUser = u.IdUser
                        WHERE r.IdUser = @UserId
                        ORDER BY r.RequestDate DESC";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserId", _currentUser.Id);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    _allRequestsDataTable = new DataTable();
                    adapter.Fill(_allRequestsDataTable);

                    dataGridViewRequests.DataSource = _allRequestsDataTable;
                    ConfigureDataGridView();

                    this.Text = "Панель клиента";
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
                dataGridViewRequests.Columns["Status"].HeaderText = "Статус";
                dataGridViewRequests.Columns["Status"].Width = 120;
            }
        }



        private void btnCreateRequest_Click(object sender, EventArgs e)
        {
            CreateRequestForm createForm = new CreateRequestForm(_currentUser);
            createForm.FormClosed += (s, args) => LoadUserRequests();
            createForm.ShowDialog();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUserRequests();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridViewRequests_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewRequests.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString().ToLower();

                if (status.Contains("выполн") || status.Contains("заверш"))
                    e.CellStyle.BackColor = Color.LightGreen;
                else if (status.Contains("в работ") || status.Contains("в процессе"))
                    e.CellStyle.BackColor = Color.LightYellow;
                else if (status.Contains("отклон") || status.Contains("отмен"))
                    e.CellStyle.BackColor = Color.Red;
                else if (status.Contains("рассмотр") || status.Contains("На рассмотрении"))
                    e.CellStyle.BackColor = Color.LightBlue;
            }
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dataGridViewRequests.DataSource = _allRequestsDataTable;
                this.Text = "Панель клиента";
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
                return;
            }

            string searchBy = cmbSearchBy.SelectedItem?.ToString() ?? "Все поля";
            string filterExpression = "";

            // Экранируем специальные символы для DataView RowFilter
            string escapedSearchText = EscapeRowFilterValue(searchText);

            switch (searchBy)
            {
                case "Все поля":
                    // Для клиента убираем поле ClientName из поиска (оно всегда одинаковое)
                    string textSearchFilter = string.Format(
                        "(Address LIKE '*{0}*') OR " +
                        "(Master LIKE '*{0}*') OR " +
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

                case "Мастер":
                    filterExpression = string.Format("Master LIKE '*{0}*'", escapedSearchText);
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
                    this.Text = $"Панель клиента - Найдено: {foundCount} заявок";
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

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        // Экспорт данных

    }
}