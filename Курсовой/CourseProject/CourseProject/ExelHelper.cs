using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Collections.Generic;

namespace CourseProject
{
    public static class ExcelHelper
    {
        // Экспорт DataTable в Excel
        public static bool ExportToExcel(DataTable dataTable, string fileName)
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Files|*.xlsx|Excel 97-2003|*.xls";
                    saveFileDialog.FileName = fileName;
                    saveFileDialog.Title = "Сохранить файл Excel";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Создаем Excel приложение
                        Microsoft.Office.Interop.Excel.Application excelApp = null;
                        Microsoft.Office.Interop.Excel.Workbook workbook = null;
                        Microsoft.Office.Interop.Excel.Worksheet worksheet = null;

                        try
                        {
                            excelApp = new Microsoft.Office.Interop.Excel.Application();
                            workbook = excelApp.Workbooks.Add();
                            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.ActiveSheet;

                            // Добавляем заголовки
                            for (int i = 0; i < dataTable.Columns.Count; i++)
                            {
                                worksheet.Cells[1, i + 1] = dataTable.Columns[i].ColumnName;
                            }

                            // Добавляем данные
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                for (int j = 0; j < dataTable.Columns.Count; j++)
                                {
                                    worksheet.Cells[i + 2, j + 1] = dataTable.Rows[i][j];
                                }
                            }

                            // Авто-размер колонок
                            worksheet.Columns.AutoFit();

                            // Сохраняем файл
                            workbook.SaveAs(saveFileDialog.FileName);
                            workbook.Close();
                            excelApp.Quit();

                            MessageBox.Show($"Данные успешно экспортированы в файл:\n{saveFileDialog.FileName}",
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        finally
                        {
                            // Освобождаем ресурсы
                            if (workbook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                            if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                            if (excelApp != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return false;
        }

        // Импорт из Excel в DataTable
        public static DataTable ImportFromExcel()
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
                    openFileDialog.Title = "Выберите файл Excel для импорта";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        string connectionString = "";

                        if (filePath.EndsWith(".xlsx"))
                        {
                            connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties='Excel 12.0 Xml;HDR=YES;'";
                        }
                        else if (filePath.EndsWith(".xls"))
                        {
                            connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={filePath};Extended Properties='Excel 8.0;HDR=YES;'";
                        }
                        else
                        {
                            MessageBox.Show("Неподдерживаемый формат файла", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }

                        using (OleDbConnection connection = new OleDbConnection(connectionString))
                        {
                            connection.Open();

                            DataTable sheets = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            if (sheets == null || sheets.Rows.Count == 0)
                            {
                                MessageBox.Show("В файле Excel нет листов", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return null;
                            }

                            string firstSheet = sheets.Rows[0]["TABLE_NAME"].ToString();
                            string query = $"SELECT * FROM [{firstSheet}]";

                            using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);

                                // Удаляем полностью пустые строки
                                DataTable cleanedTable = RemoveEmptyRows(dataTable);

                                MessageBox.Show($"Успешно импортировано {cleanedTable.Rows.Count} записей",
                                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                return cleanedTable;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте из Excel: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return null;
        }

        // Метод для удаления пустых строк
        private static DataTable RemoveEmptyRows(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return dataTable;

            DataTable result = dataTable.Clone();

            foreach (DataRow row in dataTable.Rows)
            {
                bool isEmptyRow = true;

                // Проверяем все ячейки в строке
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (row[column] != DBNull.Value &&
                        !string.IsNullOrWhiteSpace(row[column].ToString()))
                    {
                        isEmptyRow = false;
                        break;
                    }
                }

                // Если строка не пустая, добавляем ее в результат
                if (!isEmptyRow)
                {
                    result.ImportRow(row);
                }
            }

            return result;
        }

        // Экспорт в CSV (альтернатива без Excel)
        public static bool ExportToCSV(DataTable dataTable, string fileName)
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV Files|*.csv";
                    saveFileDialog.FileName = fileName;
                    saveFileDialog.Title = "Сохранить файл CSV";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                        {
                            // Заголовки
                            for (int i = 0; i < dataTable.Columns.Count; i++)
                            {
                                sw.Write(dataTable.Columns[i].ColumnName);
                                if (i < dataTable.Columns.Count - 1) sw.Write(";");
                            }
                            sw.WriteLine();

                            // Данные
                            foreach (DataRow row in dataTable.Rows)
                            {
                                for (int i = 0; i < dataTable.Columns.Count; i++)
                                {
                                    string value = row[i].ToString();
                                    // Экранируем кавычки и точки с запятой
                                    if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
                                    {
                                        value = "\"" + value.Replace("\"", "\"\"") + "\"";
                                    }
                                    sw.Write(value);
                                    if (i < dataTable.Columns.Count - 1) sw.Write(";");
                                }
                                sw.WriteLine();
                            }
                        }

                        MessageBox.Show($"Данные успешно экспортированы в файл:\n{saveFileDialog.FileName}",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в CSV: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return false;
        }
        public static List<ImportedRequest> ImportRequestsFromExcel()
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
                    openFileDialog.Title = "Выберите файл Excel с заявками";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        string connectionString = "";

                        if (filePath.EndsWith(".xlsx"))
                        {
                            connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties='Excel 12.0 Xml;HDR=YES;'";
                        }
                        else if (filePath.EndsWith(".xls"))
                        {
                            connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={filePath};Extended Properties='Excel 8.0;HDR=YES;'";
                        }
                        else
                        {
                            MessageBox.Show("Неподдерживаемый формат файла", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }

                        using (OleDbConnection connection = new OleDbConnection(connectionString))
                        {
                            connection.Open();

                            DataTable sheets = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            if (sheets == null || sheets.Rows.Count == 0)
                            {
                                MessageBox.Show("В файле Excel нет листов", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return null;
                            }

                            string firstSheet = sheets.Rows[0]["TABLE_NAME"].ToString();
                            string query = $"SELECT * FROM [{firstSheet}]";

                            using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);

                                // Преобразуем DataTable в список заявок
                                List<ImportedRequest> requests = ParseDataTableToRequests(dataTable);

                                if (requests.Count > 0)
                                {
                                    MessageBox.Show($"Успешно импортировано {requests.Count} заявок",
                                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return requests;
                                }
                                else
                                {
                                    MessageBox.Show("Не удалось распознать данные заявок", "Ошибка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте из Excel: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return null;
        }

        // Парсинг DataTable в список заявок
        private static List<ImportedRequest> ParseDataTableToRequests(DataTable dataTable)
        {
            List<ImportedRequest> requests = new List<ImportedRequest>();

            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    ImportedRequest request = new ImportedRequest();

                    // Маппинг колонок Excel на поля заявки
                    // Адаптируйте под вашу структуру Excel файла

                    // ID клиента (по ФИО или телефону)
                    if (dataTable.Columns.Contains("ClientName"))
                        request.ClientName = row["ClientName"]?.ToString();

                    if (dataTable.Columns.Contains("Телефон") || dataTable.Columns.Contains("Phone") || dataTable.Columns.Contains("Telephone"))
                        request.Phone = row[dataTable.Columns.Contains("Телефон") ? "Телефон" :
                                          dataTable.Columns.Contains("Phone") ? "Phone" : "Telephone"]?.ToString();

                    if (dataTable.Columns.Contains("Адрес") || dataTable.Columns.Contains("Address"))
                        request.Address = row[dataTable.Columns.Contains("Адрес") ? "Адрес" : "Address"]?.ToString();

                    if (dataTable.Columns.Contains("КоличествоСчетчиков") || dataTable.Columns.Contains("Counters") || dataTable.Columns.Contains("CountersNumber"))
                        request.CountersNumber = Convert.ToInt32(row[dataTable.Columns.Contains("КоличествоСчетчиков") ? "КоличествоСчетчиков" :
                                                        dataTable.Columns.Contains("Counters") ? "Counters" : "CountersNumber"]);

                    if (dataTable.Columns.Contains("Комментарий") || dataTable.Columns.Contains("Comment"))
                        request.Comment = row[dataTable.Columns.Contains("Комментарий") ? "Комментарий" : "Comment"]?.ToString();

                    if (dataTable.Columns.Contains("Мастер") || dataTable.Columns.Contains("Master"))
                        request.Master = row[dataTable.Columns.Contains("Мастер") ? "Мастер" : "Master"]?.ToString();

                    if (dataTable.Columns.Contains("Статус") || dataTable.Columns.Contains("Status"))
                        request.Status = row[dataTable.Columns.Contains("Статус") ? "Статус" : "Status"]?.ToString();
                    else
                        request.Status = "На рассмотрении"; // Значение по умолчанию

                    if (dataTable.Columns.Contains("Дата") || dataTable.Columns.Contains("Date") || dataTable.Columns.Contains("RequestDate"))
                    {
                        string dateStr = row[dataTable.Columns.Contains("Дата") ? "Дата" :
                                           dataTable.Columns.Contains("Date") ? "Date" : "RequestDate"]?.ToString();
                        if (DateTime.TryParse(dateStr, out DateTime date))
                            request.RequestDate = date;
                        else
                            request.RequestDate = DateTime.Now;
                    }
                    else
                    {
                        request.RequestDate = DateTime.Now;
                    }

                    requests.Add(request);
                }
                catch (Exception ex)
                {
                    // Пропускаем строки с ошибками, но логируем
                    Console.WriteLine($"Ошибка парсинга строки: {ex.Message}");
                }
            }

            return requests;
        }
    }
}
