using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using static CourseProject.AuthorizationForm;

namespace CourseProject
{
    public partial class ClientForm : Form
    {
        private User _currentUser;
        private string connectionString = @"Data Source=localhost;Initial Catalog=CourseProject;User ID=EgorP;Password=ISPP;Encrypt=False";

        public ClientForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            LoadUserRequests();
            DisplayUserInfo();
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
                            IdRequest,
                            RequestDate,
                            Address,
                            CountersNumber,
                            Comment,
                            Master,
                            Status
                        FROM Request 
                        WHERE IdUser = @UserId
                        ORDER BY RequestDate DESC";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserId", _currentUser.Id);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGridViewRequests.DataSource = dataTable;
                    dataGridViewRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Настройка столбцов
                    if (dataGridViewRequests.Columns.Count > 0)
                    {
                        dataGridViewRequests.Columns["IdRequest"].HeaderText = "№";
                        dataGridViewRequests.Columns["RequestDate"].HeaderText = "Дата создания";
                        dataGridViewRequests.Columns["Address"].HeaderText = "Адрес";
                        dataGridViewRequests.Columns["CountersNumber"].HeaderText = "Кол-во счётчиков";
                        dataGridViewRequests.Columns["Comment"].HeaderText = "Комментарий";
                        dataGridViewRequests.Columns["Master"].HeaderText = "Мастер";
                        dataGridViewRequests.Columns["Status"].HeaderText = "Статус";
                    }

                    lblRequestCount.Text = $"Всего заявок: {dataTable.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}