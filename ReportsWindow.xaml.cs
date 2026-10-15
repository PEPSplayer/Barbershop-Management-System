using ClosedXML.Excel;
using HairSalonIS.Data;
using HairSalonIS.Services;
using Microsoft.Win32;
using Npgsql;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace HairSalonIS.Views
{
    public partial class ReportsWindow : Window
    {
        private DataTable? currentReportTable;

        public ReportsWindow()
        {
            InitializeComponent();

            QuestPDF.Settings.License = LicenseType.Community;

            DateTime today = DateTime.Today;

            StartDatePicker.SelectedDate = new DateTime(today.Year, today.Month, 1);
            EndDatePicker.SelectedDate = today;

            GenerateReport();
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void GenerateReport()
        {
            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Выберите дату начала периода.",
                    "Формирование отчёта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Выберите дату окончания периода.",
                    "Формирование отчёта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value.Date;
            DateTime endDate = EndDatePicker.SelectedDate.Value.Date;

            if (endDate < startDate)
            {
                MessageBox.Show(
                    "Дата окончания не может быть раньше даты начала.",
                    "Формирование отчёта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            currentReportTable = Database.ExecuteQuery(
                @"select
                    order_id as ""Номер заказа"",
                    order_date as ""Дата заказа"",
                    time_from as ""Время начала"",
                    time_before as ""Время окончания"",
                    client_name as ""Клиент"",
                    master_name as ""Мастер"",
                    status_text as ""Статус"",
                    services as ""Услуги"",
                    services_sum as ""Сумма услуг"",
                    cheque_sum as ""Сумма чека""
                from get_orders_report(@startDate::date, @endDate::date)",
            new NpgsqlParameter("@startDate", startDate),
            new NpgsqlParameter("@endDate", endDate)
            );          

            ReportDataGrid.ItemsSource = currentReportTable.DefaultView;

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            if (currentReportTable == null)
            {
                SummaryTextBlock.Text = "Отчёт не сформирован.";
                return;
            }

            int ordersCount = currentReportTable.Rows.Count;
            decimal servicesSum = 0;
            decimal chequeSum = 0;

            foreach (DataRow row in currentReportTable.Rows)
            {
                if (row["Сумма услуг"] != DBNull.Value)
                {
                    servicesSum += Convert.ToDecimal(row["Сумма услуг"]);
                }

                if (row["Сумма чека"] != DBNull.Value)
                {
                    chequeSum += Convert.ToDecimal(row["Сумма чека"]);
                }
            }

            SummaryTextBlock.Text =
                $"Количество заказов: {ordersCount} | Сумма услуг: {servicesSum} руб. | Сумма чеков: {chequeSum} руб.";
        }

        private void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentReportTable == null || currentReportTable.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Нет данных для экспорта. Сначала сформируйте отчёт.",
                    "Экспорт в Excel",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Сохранить отчёт в Excel",
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Отчёт_по_заказам.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        IXLWorksheet worksheet = workbook.Worksheets.Add("Отчёт по заказам");

                        worksheet.Cell(1, 1).Value = "Отчёт по заказам парикмахерской";
                        worksheet.Range(1, 1, 1, currentReportTable.Columns.Count).Merge();
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        string periodText =
                            $"Период: {StartDatePicker.SelectedDate:dd.MM.yyyy} - {EndDatePicker.SelectedDate:dd.MM.yyyy}";

                        worksheet.Cell(2, 1).Value = periodText;
                        worksheet.Range(2, 1, 2, currentReportTable.Columns.Count).Merge();
                        worksheet.Cell(2, 1).Style.Font.Italic = true;
                        worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell(4, 1).InsertTable(currentReportTable, "OrdersReport", true);

                        int lastRow = 4 + currentReportTable.Rows.Count + 2;

                        worksheet.Cell(lastRow, 1).Value = SummaryTextBlock.Text;
                        worksheet.Range(lastRow, 1, lastRow, currentReportTable.Columns.Count).Merge();
                        worksheet.Cell(lastRow, 1).Style.Font.Bold = true;

                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show(
                        "Отчёт успешно экспортирован в Excel.",
                        "Экспорт в Excel",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    LogService.AddLog(
                        "Экспорт",
                        $"Пользователь {AuthService.CurrentAdminName} экспортировал отчёт по заказам в Excel."
                    );

                    OpenFile(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ошибка при экспорте отчёта:\n" + ex.Message,
                        "Экспорт в Excel",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentReportTable == null || currentReportTable.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Нет данных для экспорта. Сначала сформируйте отчёт.",
                    "Экспорт в PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Сохранить отчёт в PDF",
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "Отчёт_по_заказам.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string startDate = StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? "";
                    string endDate = EndDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? "";
                    string summary = SummaryTextBlock.Text;

                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4.Landscape());
                            page.Margin(20);
                            page.DefaultTextStyle(x => x.FontSize(9));

                            page.Header().Column(column =>
                            {
                                column.Item().Text("Отчёт по заказам парикмахерской")
                                    .FontSize(18)
                                    .Bold()
                                    .AlignCenter();

                                column.Item().Text($"Период: {startDate} - {endDate}")
                                    .FontSize(11)
                                    .Italic()
                                    .AlignCenter();

                                column.Item().PaddingTop(10);
                            });

                            page.Content().Column(column =>
                            {
                                column.Item().Table(table =>
                                {
                                    int columnsCount = currentReportTable.Columns.Count;

                                    table.ColumnsDefinition(columns =>
                                    {
                                        for (int i = 0; i < columnsCount; i++)
                                        {
                                            columns.RelativeColumn();
                                        }
                                    });

                                    table.Header(header =>
                                    {
                                        foreach (DataColumn dataColumn in currentReportTable.Columns)
                                        {
                                            header.Cell()
                                                .Background(Colors.Grey.Lighten2)
                                                .Border(1)
                                                .Padding(4)
                                                .Text(dataColumn.ColumnName)
                                                .Bold();
                                        }
                                    });

                                    foreach (DataRow row in currentReportTable.Rows)
                                    {
                                        foreach (DataColumn dataColumn in currentReportTable.Columns)
                                        {
                                            string value = FormatPdfCellValue(row[dataColumn]);

                                            table.Cell()
                                                .Border(1)
                                                .Padding(4)
                                                .Text(value);
                                        }
                                    }
                                });

                                column.Item()
                                    .PaddingTop(15)
                                    .Text(summary)
                                    .FontSize(11)
                                    .Bold();
                            });

                            page.Footer()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("Страница ");
                                    text.CurrentPageNumber();
                                    text.Span(" из ");
                                    text.TotalPages();
                                });
                        });
                    }).GeneratePdf(saveFileDialog.FileName);

                    MessageBox.Show(
                        "Отчёт успешно экспортирован в PDF.",
                        "Экспорт в PDF",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    LogService.AddLog(
                        "Экспорт",
                        $"Пользователь {AuthService.CurrentAdminName} экспортировал отчёт по заказам в PDF."
                    );

                    OpenFile(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ошибка при экспорте отчёта в PDF:\n" + ex.Message,
                        "Экспорт в PDF",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private string FormatPdfCellValue(object value)
        {
            if (value == DBNull.Value || value == null)
            {
                return "";
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd.MM.yyyy");
            }

            if (value is DateOnly dateOnly)
            {
                return dateOnly.ToString("dd.MM.yyyy");
            }

            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("0.##");
            }

            return value.ToString() ?? "";
        }

        private void OpenFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = fileName,
                        UseShellExecute = true
                    }
                );
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}