using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using Spectre.Console;
using static CoffeeShopStockpileManagement.SalesReport;

namespace CoffeeShopStockpileManagement
{
    public class SellProduct
    {
        private Dictionary<string, (string batchNumber, string category, string location, int quantity, string weight, string weightUnit, string packaging, DateTime? expirationDate, decimal price)> inventory;
        private InventoryManager inventoryManager;

        public SellProduct(InventoryManager inventoryManager)
        {
            this.inventoryManager = inventoryManager;
            this.inventory = inventoryManager.inventory;
            SalesReport salesReport = new SalesReport();
        }

        public void StartSelling()
        {
            bool continueSelling = true;
            List<(string itemName, int quantitySold, decimal totalPrice, string batchNumber)> cart = new();
            decimal grandTotal = 0;
            decimal receivedCash = 0;
            decimal changeDue = 0;

            SalesReport salesReport = new SalesReport();

            while (continueSelling)
            {
                Console.Clear();

                var table = new Table();
                table.AddColumn("[cyan]Item[/]");
                table.AddColumn("[cyan]Batch Number[/]");
                table.AddColumn("[cyan]Category[/]");
                table.AddColumn("[cyan]Location[/]");
                table.AddColumn("[cyan]Quantity[/]");
                table.AddColumn("[cyan]Price[/]");
                table.AddColumn("[cyan]Status[/]");
                table.AddColumn("[cyan]Expiration[/]");

                foreach (var item in inventory)
                {
                    var details = item.Value;
                    string status = details.quantity > 0 ? "[green]Available[/]" : "[red]Sold Out[/]";
                    table.AddRow(
                        item.Key,
                        details.batchNumber,
                        details.category,
                        details.location,
                        details.quantity.ToString(),
                        $"PHP{details.price:F2}",
                        status,
                        details.expirationDate?.ToString("yyyy-MM-dd") ?? "N/A"
                    );
                }

                AnsiConsole.Write(table);

                var availableItems = inventory.Where(i => i.Value.quantity > 0).Select(i => i.Key).ToList();
                if (availableItems.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No items available for sale![/]");
                    break;
                }

                DisplayCart(cart, grandTotal, receivedCash, changeDue);

                availableItems.Add("cancel");
                string selectedItem = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an [cyan]item[/] to sell (or [bold red]cancel[/] to exit):")
                        .AddChoices(availableItems));

                if (selectedItem.ToLower() == "cancel") break;

                var itemDetails = inventory[selectedItem];
                int quantityToSell = 0;

                while (true)
                {
                    string quantityInput = AnsiConsole.Ask<string>($"Enter the [cyan]quantity[/] to sell (Available: [green]{itemDetails.quantity}[/])(or type [bold red]cancel[/] to exit):");

                    if (quantityInput.ToLower() == "cancel")
                    {
                        AnsiConsole.MarkupLine("[yellow]Returning to product selection...[/]");
                        break;
                    }

                    if (int.TryParse(quantityInput, out quantityToSell) && quantityToSell > 0 && quantityToSell <= itemDetails.quantity)
                    {
                        break;
                    }

                    AnsiConsole.MarkupLine("[red]Invalid quantity. Please enter a valid number or type 'cancel' to go back.[/]");
                }

                if (quantityToSell == 0) continue;

                decimal totalPrice = quantityToSell * itemDetails.price;
                var existingItem = cart.FirstOrDefault(x => x.itemName == selectedItem);
                grandTotal += totalPrice;

                if (existingItem.itemName != null)
                {
                    cart.Remove(existingItem);
                    cart.Add((selectedItem, existingItem.quantitySold + quantityToSell, existingItem.totalPrice + totalPrice, itemDetails.batchNumber));
                }
                else
                {
                    cart.Add((selectedItem, quantityToSell, totalPrice, itemDetails.batchNumber));
                }

                string addMore = AnsiConsole.Ask<string>("Do you want to sell another item? (y/n):").ToLower();
                if (addMore != "y")
                {
                    DisplayCart(cart, grandTotal, receivedCash, changeDue);

                    while (true)
                    {
                        string receivedCashInput = AnsiConsole.Ask<string>("Enter [cyan]received cash[/] or type [bold red]cancel[/] to return to product selection:");

                        if (receivedCashInput.ToLower() == "cancel")
                        {
                            AnsiConsole.MarkupLine("[yellow]Returning to product selection...[/]");
                            break;
                        }

                        if (decimal.TryParse(receivedCashInput, out receivedCash))
                        {
                            if (receivedCash >= grandTotal)
                            {
                                changeDue = receivedCash - grandTotal;
                                DisplayCart(cart, grandTotal, receivedCash, changeDue);
                                continueSelling = false;

                                salesReport.SaveSalesData(cart.Select(item => new SaleRecord
                                {
                                    ItemName = item.itemName,
                                    Quantity = item.quantitySold,
                                    Total = item.totalPrice,
                                    BatchNumber = item.batchNumber,
                                    DateOfSale = DateTime.Now
                                }).ToList());

                                foreach (var item in cart)
                                {
                                    var inventoryItemDetails = inventory[item.itemName];
                                    inventory[item.itemName] = (
                                        inventoryItemDetails.batchNumber,
                                        inventoryItemDetails.category,
                                        inventoryItemDetails.location,
                                        inventoryItemDetails.quantity - item.quantitySold,
                                        inventoryItemDetails.weight,
                                        inventoryItemDetails.weightUnit,
                                        inventoryItemDetails.packaging,
                                        inventoryItemDetails.expirationDate,
                                        inventoryItemDetails.price
                                    );
                                }
                                break;
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("[red]Insufficient funds. Please enter an amount greater than or equal to the total.[/]");
                            }
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]Invalid cash input. Please enter a valid number.[/]");
                        }
                    }
                }
            }

            Console.Clear();
            GenerateReceipt(cart, grandTotal, receivedCash, changeDue);
            DisplayCart(cart, grandTotal, receivedCash, changeDue);
        }

        private void DisplayCart(List<(string itemName, int quantitySold, decimal totalPrice, string batchNumber)> cart, decimal grandTotal, decimal receivedCash, decimal changeDue)
        {
            var table = new Table();
            table.AddColumn("[cyan]Item[/]");
            table.AddColumn("[cyan]Batch Number[/]");
            table.AddColumn("[cyan]Quantity[/]");
            table.AddColumn("[cyan]Total Price[/]");

            foreach (var item in cart)
            {
                table.AddRow(item.itemName, item.batchNumber, item.quantitySold.ToString(), $"PHP {item.totalPrice:F2}");
            }

            table.AddRow("", "", "[bold]Grand Total:[/]", $"[bold green]PHP {grandTotal:F2}[/]");

            if (receivedCash > 0)
            {
                table.AddRow("", "", "[bold]Received Cash:[/]", $"[bold cyan]PHP {receivedCash:F2}[/]");
                table.AddRow("", "", "[bold]Change Due:[/]", $"[bold yellow]PHP {changeDue:F2}[/]");
            }

            AnsiConsole.Write(table);
        }

        private void GenerateReceipt(
            List<(string itemName, int quantitySold, decimal totalPrice, string batchNumber)> cart,
            decimal grandTotal,
            decimal receivedCash,
            decimal changeDue)
        {
            if (cart.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No items were sold.[/]");
                return;
            }

            string folderPath = "Receipts";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string receiptFileName = Path.Combine(folderPath, $"Receipt_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            using (var writer = new StreamWriter(receiptFileName))
            {
                writer.WriteLine("Coffee Shop Receipt");
                writer.WriteLine("===================");
                writer.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();

                foreach (var item in cart)
                {
                    string itemName = item.itemName;
                    int quantity = item.quantitySold;
                    decimal itemTotal = item.totalPrice;
                    string batchNumber = item.batchNumber;

                    writer.WriteLine($"{itemName,-40}{quantity}x");
                    writer.WriteLine($"{batchNumber,-40}{itemTotal:F2}");
                    writer.WriteLine();
                }

                writer.WriteLine("-----------------------");
                writer.WriteLine($"Total: PHP {grandTotal:F2}");
                writer.WriteLine("-----------------------");
                writer.WriteLine($"Received Cash            PHP       {receivedCash:F2}");
                writer.WriteLine($"Change Due              PHP       {changeDue:F2}");
            }

            AnsiConsole.MarkupLine($"[green]Receipt generated: {receiptFileName}[/]");
        }
    }
}