using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;

namespace CoffeeShopStockpileManagement
{
    class SupplierManager
    {
        private const string SuppliersFile = "suppliers.txt";
        private Dictionary<string, SupplierDetails> suppliers = new Dictionary<string, SupplierDetails>();

        public void ManageSuppliers()
        {
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Supplier Management[/]")
                        .AddChoices(new[] {
                        "1. Add Supplier",
                        "2. Remove Supplier",
                        "3. View Suppliers",
                        "4. Back to Main Menu"
                        }));

                switch (choice)
                {
                    case "1. Add Supplier":
                        AddSupplier();
                        break;
                    case "2. Remove Supplier":
                        RemoveSupplier();
                        break;
                    case "3. View Suppliers":
                        ViewSuppliers();
                        break;
                    case "4. Back to Main Menu":
                        return;
                }

                Console.Clear();
            }
        }

        public void AddSupplier()
        {
            bool continueAdding = true;

            while (continueAdding)
            {
                Console.Clear();

                var table = new Table();
                table.AddColumn("[cyan]Supplier Name[/]");
                table.AddColumn("[cyan]Contact Information[/]");
                table.AddColumn("[cyan]Products Supplied[/]");

                foreach (var supplier in suppliers)
                {
                    table.AddRow(
                        supplier.Key,
                        supplier.Value.ContactInfo,
                        string.Join(", ", supplier.Value.Products)
                    );
                }

                AnsiConsole.Write(table);

                string supplierName = AnsiConsole.Ask<string>("Enter the [cyan]supplier name[/] or type [yellow]cancel[/] to abort:");
                if (supplierName.ToLower() == "cancel") return;

                string contactInfo = AnsiConsole.Ask<string>("Enter the [cyan]contact information[/] or type [yellow]cancel[/] to abort:");
                if (contactInfo.ToLower() == "cancel") return;

                var products = new List<string>();
                bool addProducts = true;
                while (addProducts)
                {
                    string product = AnsiConsole.Ask<string>("Enter a [cyan]product supplied[/] or type [yellow]done[/] to finish:");
                    if (product.ToLower() == "done") break;
                    products.Add(product);
                }

                suppliers[supplierName] = new SupplierDetails
                {
                    ContactInfo = contactInfo,
                    Products = products
                };

                AnsiConsole.MarkupLine($"[green]Added supplier: {supplierName} with contact {contactInfo} and products: {string.Join(", ", products)}.[/]");

                string addAnother = AnsiConsole.Ask<string>("Would you like to add another supplier? (y/n):").ToLower();
                if (addAnother != "y")
                {
                    continueAdding = false;
                }
            }
        }

        public void ViewSuppliers()
        {
            Console.Clear();

            if (suppliers.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No suppliers found![/]");
            }
            else
            {
                var table = new Table();
                table.AddColumn("[cyan]Supplier Name[/]");
                table.AddColumn("[cyan]Contact Information[/]");
                table.AddColumn("[cyan]Products Supplied[/]");

                foreach (var supplier in suppliers)
                {
                    table.AddRow(
                        supplier.Key,
                        supplier.Value.ContactInfo,
                        string.Join(", ", supplier.Value.Products)
                    );
                }

                AnsiConsole.Write(table);
            }

            AnsiConsole.MarkupLine("\n[green]Press any key to return to the Supplier Management Menu...[/]");
            Console.ReadKey(true);
        }

        public void RemoveSupplier()
        {
            bool continueRemoving = true;

            while (continueRemoving)
            {
                Console.Clear();

                if (suppliers.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No suppliers available to remove![/]");
                    break;
                }

                var table = new Table();
                table.AddColumn("[cyan]Supplier Name[/]");
                table.AddColumn("[cyan]Contact Information[/]");
                table.AddColumn("[cyan]Products Supplied[/]");

                foreach (var supplier in suppliers)
                {
                    table.AddRow(
                        supplier.Key,
                        supplier.Value.ContactInfo,
                        string.Join(", ", supplier.Value.Products)
                    );
                }

                AnsiConsole.Write(table);

                string supplierName = AnsiConsole.Ask<string>("Enter the [cyan]supplier name to remove[/] or type [yellow]cancel[/]:");
                if (supplierName.ToLower() == "cancel")
                {
                    AnsiConsole.MarkupLine("[yellow]Supplier removal canceled.[/]");
                    break;
                }

                if (suppliers.Remove(supplierName))
                {
                    AnsiConsole.MarkupLine($"[green]{supplierName} removed from suppliers.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Supplier not found.[/]");
                }

                string removeAnother = AnsiConsole.Ask<string>("Would you like to remove another supplier? (y/n):").ToLower();
                if (removeAnother != "y")
                {
                    continueRemoving = false;
                }
            }
        }

        public void SaveSupplierData()
        {
            using (var writer = new StreamWriter(SuppliersFile))
            {
                foreach (var supplier in suppliers)
                {
                    writer.WriteLine($"{supplier.Key}|{supplier.Value.ContactInfo}|{string.Join(",", supplier.Value.Products)}");
                }
            }
        }

        public void LoadSupplierData()
        {
            if (File.Exists(SuppliersFile))
            {
                foreach (var line in File.ReadAllLines(SuppliersFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 2)
                    {
                        suppliers[parts[0]] = new SupplierDetails
                        {
                            ContactInfo = parts[1],
                            Products = parts.Length > 2 ? new List<string>(parts[2].Split(',')) : new List<string>()
                        };
                    }
                }
            }
        }

        public class SupplierDetails
        {
            public string ContactInfo { get; set; } = string.Empty;
            public List<string> Products { get; set; } = new List<string>();
        }
    }
}
