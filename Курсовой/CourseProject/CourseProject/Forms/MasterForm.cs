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

        public MasterForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void MasterForm_Load(object sender, EventArgs e)
        {
            LoadMasterRequests();
            DisplayMasterInfo();
            LoadStatuses();
        }

        private void DisplayMasterInfo()
        {
            string masterName = $"{_currentUser.Surname} {_currentUser.Name}";
            if (!string.IsNullOrEmpty(_currentUser.Patronymic))
                masterName += $" {_currentUser.Patronymic}";

            lblMasterInfo.Text = $"Мастер: {masterName}";
        }

        private void LoadMasterRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Получаем полное имя мастера для поиска в заявках
                    string masterFullName = $"{_currentUser.Surname} {_currentUser.Name}";
                    if (!string.IsNullOrEmpty(_currentUser.Patronymic))
                        masterFullName += $" {_currentUser.Patronymic}";

                    string query = @"
                        SELECT 
                            r.IdRequest,
                            r.RequestDate,
                            r.Address,
                            u.UserSurname + ' ' + u.UserName as ClientName,
                            u.TelephoneNumber,
                            r.CountersNumber,
                            r.Comment,
                            r.Status
                        FROM Request r
                        INNER JOIN [User] u ON r.IdUser = u.IdUser
                        WHERE r.Master = @MasterName
                        ORDER BY 
                            CASE 
                                WHEN r.Status = 'В работе' THEN 1
                                WHEN r.Status = 'На рассмотрении' THEN 2
                                ELSE 3
                            END,
                            r.RequestDate DESC";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@MasterName", masterFullName);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGridViewRequests.DataSource = dataTable;
                    ConfigureDataGridView();

                    lblRequestCount.Text = $"Заявок: {dataTable.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                dataGridViewRequests.Columns["CountersNumber"].HeaderText = "Номер счетчика";
                dataGridViewRequests.Columns["CountersNumber"].Width = 120;
                dataGridViewRequests.Columns["Comment"].HeaderText = "Комментарий";
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

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (dataGridViewRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заявку для изменения статуса", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(cmbStatus.Text))
            {
                MessageBox.Show("Выберите новый статус", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int requestId = (int)dataGridViewRequests.SelectedRows[0].Cells["IdRequest"].Value;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        UPDATE Request 
                        SET Status = @Status
                        WHERE IdRequest = @RequestId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Status", cmbStatus.Text.Trim());
                    command.Parameters.AddWithValue("@RequestId", requestId);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Статус заявки успешно обновлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadMasterRequests();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadMasterRequests();
            ClearForm();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridViewRequests_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewRequests.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewRequests.SelectedRows[0];

                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";
                txtCounterNumber.Text = row.Cells["CountersNumber"].Value?.ToString() ?? "";
                txtComment.Text = row.Cells["Comment"].Value?.ToString() ?? "";
                txtClientPhone.Text = row.Cells["TelephoneNumber"].Value?.ToString() ?? "";

                string status = row.Cells["Status"].Value?.ToString() ?? "";

                if (!string.IsNullOrEmpty(status))
                {
                    cmbStatus.Text = status;
                }

                btnUpdateStatus.Enabled = true;
            }
            else
            {
                btnUpdateStatus.Enabled = false;
            }
        }

        private void ClearForm()
        {
            txtAddress.Text = "";
            txtCounterNumber.Text = "";
            txtComment.Text = "";
            txtClientPhone.Text = "";
            cmbStatus.Text = "";
            btnUpdateStatus.Enabled = false;
        }

        private void dataGridViewRequests_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewRequests.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString().ToLower();

                if (status.Contains("выполн"))
                    e.CellStyle.BackColor = Color.LightGreen;
                else if (status.Contains("в работ"))
                    e.CellStyle.BackColor = Color.LightYellow;
                else if (status.Contains("отклон"))
                    e.CellStyle.BackColor = Color.LightCoral;
                else if (status.Contains("рассмотр"))
                    e.CellStyle.BackColor = Color.LightBlue;
            }
        }
    }
}