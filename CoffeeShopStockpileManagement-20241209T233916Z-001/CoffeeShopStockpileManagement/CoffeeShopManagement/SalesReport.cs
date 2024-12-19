using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;

namespace CoffeeShopStockpileManagement
{
    public class SalesReport
    {
        public List<SaleRecord> salesRecords = new List<SaleRecord>();

        public SalesReport()
        {
            salesRecords = new List<SaleRecord>();
            LoadSalesData();
        }

        public void AddSale(string itemName, int quantitySold, decimal totalPrice, string batchNumber)
        {
            if (quantitySold <= 0 || totalPrice <= 0)
            {
                AnsiConsole.MarkupLine("[red]Invalid sale. Quantity sold or total price cannot be zero or less.[/]");
                return;
            }

            var sale = new SaleRecord
            {
                ItemName = itemName,
                Quantity = quantitySold,
                Total = totalPrice,
                BatchNumber = batchNumber,
                DateOfSale = DateTime.Now
            };

            salesRecords.Add(sale);

            AnsiConsole.MarkupLine($"[yellow]Sales records count: {salesRecords.Count}[/]");

            SaveSalesData(salesRecords);
        }

        public void ViewTotalSales()
        {
            decimal totalSales = 0;
            foreach (var record in salesRecords)
            {
                totalSales += record.Total;
            }

            AnsiConsole.MarkupLine("[bold green]Total Sales: [/]" + totalSales.ToString("₱0.00"));
        }

        public void SaveSalesData(List<SaleRecord> sales)
        {
            string reportFileName = "SalesReport.txt";

            try
            {
                using (StreamWriter writer = new StreamWriter(reportFileName, append: true))
                {
                    foreach (var sale in sales)
                    {
                        try
                        {
                            writer.WriteLine($"Sale Date: {sale.DateOfSale:yyyy-MM-dd HH:mm:ss}");
                            writer.WriteLine($"Item: {sale.ItemName}, Quantity Sold: {sale.Quantity}, Total Price: PHP {sale.Total:F2}, Batch Number: {sale.BatchNumber}");
                            writer.WriteLine("----------------------------");

                            AnsiConsole.MarkupLine($"[green]Sale recorded: {sale.ItemName}, {sale.Quantity} x PHP {sale.Total:F2}[/]");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Failed to write sale record for {sale.ItemName}: {ex.Message}[/]");
                        }
                    }
                }

                AnsiConsole.MarkupLine($"[green]Sales data saved to: {reportFileName}[/]");
            }
            catch (UnauthorizedAccessException)
            {
                AnsiConsole.MarkupLine("[red]Error: Access to the file is denied. Check file permissions.[/]");
            }
            catch (IOException ex)
            {
                AnsiConsole.MarkupLine($"[red]I/O Error while saving the file: {ex.Message}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An unexpected error occurred while saving sales data: {ex.Message}[/]");
            }
        }

        public void LoadSalesData()
        {
            string filePath = "SalesReport.txt";

            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            try
                            {
                                AnsiConsole.MarkupLine($"[yellow]Reading line: {line}[/]");

                                if (line.StartsWith("Sale Date:"))
                                {
                                    var sale = new SaleRecord();

                                    sale.DateOfSale = DateTime.TryParse(line.Substring(11), out DateTime dateOfSale) ? dateOfSale : DateTime.MinValue;
                                    AnsiConsole.MarkupLine($"[yellow]Parsed DateOfSale: {sale.DateOfSale}[/]");

                                    sale.ItemName = reader.ReadLine()?.Substring(6) ?? string.Empty;
                                    sale.Quantity = int.TryParse(reader.ReadLine()?.Substring(17), out int quantity) ? quantity : 0;
                                    sale.Total = decimal.TryParse(reader.ReadLine()?.Substring(15), out decimal total) ? total : 0.0m;
                                    sale.BatchNumber = reader.ReadLine()?.Substring(14) ?? string.Empty;

                                    if (sale.Quantity > 0 && sale.Total > 0)
                                    {
                                        salesRecords.Add(sale);
                                        AnsiConsole.MarkupLine($"[yellow]Parsed Sale - Item: {sale.ItemName}, Quantity: {sale.Quantity}, Total: {sale.Total}[/]");
                                    }
                                    else
                                    {
                                        AnsiConsole.MarkupLine("[red]Invalid sale data skipped.[/]");
                                    }
                                }
                            }
                            catch (FormatException ex)
                            {
                                AnsiConsole.MarkupLine($"[red]Invalid data format encountered: {ex.Message}[/]");
                            }
                            catch (Exception ex)
                            {
                                AnsiConsole.MarkupLine($"[red]Error processing sales data: {ex.Message}[/]");
                            }
                        }
                    }

                    AnsiConsole.MarkupLine("Sales data loaded successfully.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]No sales data file found.[/]");
                }
            }
            catch (UnauthorizedAccessException)
            {
                AnsiConsole.MarkupLine("[red]Error: Access to the file is denied. Check file permissions.[/]");
            }
            catch (IOException ex)
            {
                AnsiConsole.MarkupLine($"[red]I/O Error while reading the file: {ex.Message}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An unexpected error occurred while loading sales data: {ex.Message}[/]");
            }
        }

        public class SaleRecord
        {
            public string? ItemName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
            public DateTime DateOfSale { get; set; }
            public string? BatchNumber { get; set; }
        }
    }
}
