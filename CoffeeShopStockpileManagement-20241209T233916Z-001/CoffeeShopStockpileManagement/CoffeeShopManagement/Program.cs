using System;
using Spectre.Console;

namespace CoffeeShopStockpileManagement
{
    class CoffeeShopManagementSystem
    {
        private static InventoryManager inventoryManager = new InventoryManager();
        private static SupplierManager supplierManager = new SupplierManager();
        private static SellProduct sellProduct = new SellProduct(inventoryManager);
        private static SalesReport salesReport = new SalesReport();

        static void Main(string[] args)
        {
            inventoryManager.LoadInventoryData();
            supplierManager.LoadSupplierData();
            salesReport.LoadSalesData();

            bool isRunning = true;

            while (isRunning)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold cyan]Coffee Stock Management System[/]")
                        .AddChoices(new[] {
                            "1. Manage Inventory",
                            "2. Supplier Management",
                            "3. Sell Product",
                            "4. Exit",
                        })
                );

                switch (choice)
                {
                    case "1. Manage Inventory":
                        inventoryManager.ManageInventory();
                        break;

                    case "2. Supplier Management":
                        supplierManager.ManageSuppliers();
                        break;

                    case "3. Sell Product":
                        sellProduct.StartSelling();
                        break;

                    case "4. Exit":
                        inventoryManager.SaveInventoryData();
                        supplierManager.SaveSupplierData();
                        salesReport.SaveSalesData(salesReport.salesRecords);
                        isRunning = false; // End the program
                        AnsiConsole.MarkupLine("[bold green]Thank you for using the system![/]");
                        break;

                    default:
                        AnsiConsole.MarkupLine("[red]Invalid choice. Please try again.[/]");
                        break;
                }

                if (isRunning)
                {
                    AnsiConsole.MarkupLine("\nPress any key to return to the main menu...");
                    Console.ReadKey();
                }
            }
        }
    }
}
