using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;

namespace CoffeeShopStockpileManagement
{
    public class InventoryManager
    {
        public Dictionary<string, (
            string batchNumber, string category, 
            string location, int quantity, 
            string weight, string weightUnit, string packaging, 
            DateTime? expirationDate, decimal price)> inventory
        = new Dictionary<string, (string, string, string, int, string, string, string, DateTime?, decimal)>();
        
        private readonly List<string> categories = new List<string> { 
            "Roasted Beans", "Accessories", 
            "Disposable Products", "Coffee Equipment", 
            "Other", "cancel" };
        private readonly List<string> locations 
            = new List<string> { "Coffee Shop Storage", "On Display", "Other", "cancel" };
        private readonly List<string> weightUnit 
            = new List<string> { "g", "kg", "ml", "l", "cancel" };

        public Dictionary<string, (
            string batchNumber, 
            string category, 
            string location, 
            int quantity, 
            string weight, 
            string weightUnit, 
            string packaging, 
            DateTime? expirationDate, 
            decimal price)> 
            GetAvailableItems()
        {
            return inventory;
        }
        public InventoryManager()
        {
            inventory = new Dictionary<string, (
                string, 
                string, 
                string, 
                int, 
                string, 
                string, 
                string, 
                DateTime?, 
                decimal)>();

            LoadInventoryData();
        }
        public void ManageInventory()
        {
            while (true)
            {
                Console.Clear();
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Inventory Management[/]")
                        .AddChoices(new[] {
                    "1. Add Item",
                    "2. Update Item",
                    "3. Remove Item",
                    "4. View Inventory",
                    "5. Back to Main Menu"

                        }));

                switch (choice)
                {
                    case "1. Add Item":
                        AddInventory();
                        break;
                    case "2. Update Item":
                        UpdateInventory();
                        break;
                    case "3. Remove Item":
                        RemoveInventory();
                        break;
                    case "4. View Inventory":
                        ViewInventory(true);
                        break;
                    case "5. Back to Main Menu":
                        SaveInventoryData();
                        return;
                }
            }
        }
        public void AddInventory()
        {
            bool continueAdding = true;

            while (continueAdding)
            {
                Console.Clear();

                var table = new Table();
                table.AddColumn("[cyan]Item[/]");
                table.AddColumn("[cyan]Batch Number[/]");
                table.AddColumn("[cyan]Category[/]");
                table.AddColumn("[cyan]Location[/]");
                table.AddColumn("[cyan]Quantity[/]");
                table.AddColumn("[cyan]Weight[/]");
                table.AddColumn("[cyan]Weight Unit[/]");
                table.AddColumn("[cyan]Packaging[/]");
                table.AddColumn("[cyan]Expiration Date[/]");
                table.AddColumn("[cyan]Price[/]");
                table.AddColumn("[cyan]Status[/]");

                foreach (var item in inventory)
                {
                    var details = item.Value;
                    string expiration = details.expirationDate.HasValue 
                        ? details.expirationDate.Value.ToShortDateString() 
                        : "[yellow]No Expiration[/]";
                    string status = details.expirationDate.HasValue
                        ? (details.expirationDate.Value < DateTime.Now 
                        ? "[red]Expired[/]" 
                        : "[green]Valid[/]")
                        : "[yellow]N/A[/]";

                    table.AddRow(item.Key, details.batchNumber, details.category, 
                        details.location, details.quantity.ToString(), details.weight.ToString(), 
                        details.weightUnit, details.packaging, expiration, $"PHP{details.price:F2}", status);
                }
                AnsiConsole.Write(table);

                string itemName = "";
                bool itemNameValid = false;

                while (!itemNameValid)
                {
                    itemName = AnsiConsole.Ask<string>("Enter the [cyan]item name[/] (or type [bold red]cancel[/] to exit):");

                    if (itemName.ToLower() == "cancel") return;

                    if (inventory.ContainsKey(itemName))
                    {
                        AnsiConsole.MarkupLine("[red]Error: This item name already exists in the inventory. Please enter a different name.[/]");
                    }
                    else
                    {
                        itemNameValid = true;
                    }
                }

                string batchNumber = "";

                while (true)
                {
                    batchNumber = AnsiConsole.Ask<string>("Enter the [cyan]batch number[/] (or type [bold red]cancel[/] to exit):");
                    if (batchNumber.ToLower() == "cancel") return;

                    if (inventory.Values.Any(item => item.batchNumber == batchNumber))
                    {
                        AnsiConsole.MarkupLine("[red]Error: This batch number already exists in the inventory. Please enter a different batch number.[/]");
                    }
                    else
                    {
                        break;
                    }
                }

                string category = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select [cyan]category[/] (or select [bold red]cancel[/] to exit):")
                        .AddChoices(categories));
                if (category == "cancel") return;

                string location = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select [cyan]location[/] (or select [bold red]cancel[/] to exit):")
                        .AddChoices(locations));
                if (location == "cancel") return;

                int quantity = 0;
                bool validInput = false;

                while (!validInput)
                {
                    string input = AnsiConsole.Ask<string>("Enter the [cyan]quantity[/] (or type [bold red]cancel[/] to exit):");

                    if (input.ToLower() == "cancel")
                    {
                        return;
                    }

                    if (int.TryParse(input, out quantity))
                    {
                        if (quantity > 0)
                        {
                            validInput = true;
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]Quantity must be greater than 0.[/]");
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Invalid input. Please enter a valid integer.[/]");
                    }
                }

                string weight;

                while (true)
                {
                    weight = AnsiConsole.Ask<string>("Enter the [cyan]weight[/] (or type [bold red]cancel[/] to exit):");

                    if (weight.ToLower() == "cancel") return;

                    if (string.IsNullOrEmpty(weight) || !double.TryParse(weight, out double parsedWeight) || parsedWeight <= 0)
                    {
                        AnsiConsole.MarkupLine("[red]Invalid weight input. Please enter a valid number greater than zero for weight.[/]");
                    }
                    else
                    {
                        break;
                    }
                }

                string weightUnit = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select [cyan]weight unit (kg, g, lb, etc.)[/] (or select [bold red]cancel[/] to exit):")
                        .AddChoices(new[] { "g", "kg", "ml", "l", "cancel" }));
                if (weightUnit == "cancel") return;

                string packaging = AnsiConsole.Ask<string>("Enter the [cyan]packaging[/] (or type [bold red]cancel[/] to exit):");
                if (packaging.ToLower() == "cancel") return;

                string expirationInput = AnsiConsole.Ask<string>("Does this product have an expiration date? (y/n):");
                DateTime? expirationDate = null;

                if (expirationInput.ToLower() == "y")
                {
                    while (true)
                    {
                        string expirationDateInput = AnsiConsole.Ask<string>("Enter the [cyan]expiration date (yyyy-MM-dd)[/]:");

                        if (expirationDateInput.ToLower() == "cancel") return;

                        if (DateTime.TryParse(expirationDateInput, out DateTime parsedDate))
                        {
                            expirationDate = parsedDate;
                            break;
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]Invalid date format. Please enter a valid date in the format yyyy-MM-dd.[/]");
                        }
                    }
                }

                decimal price = 0;
                bool validPriceInput = false;

                while (!validPriceInput)
                {
                    string priceInput = AnsiConsole.Ask<string>("Enter the [cyan]price[/] (or type [bold red]cancel[/] to exit):");

                    if (priceInput.ToLower() == "cancel") return;

                    if (decimal.TryParse(priceInput, out price) && price > 0)
                    {
                        validPriceInput = true;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Invalid price input. Please enter a valid number greater than zero for price.[/]");
                    }
                }

                inventory.Add(itemName, (batchNumber, category, location, quantity, weight, weightUnit, packaging, expirationDate, price));
                AnsiConsole.MarkupLine($"[green]Added {itemName} with batch {batchNumber} in {category} category at {location}, quantity {quantity} and expiry date {expirationDate} to inventory.[/]");

                string addAnother = AnsiConsole.Ask<string>("Would you like to add another item? (y/n):").ToLower();

                Console.Clear();

                if (addAnother != "y")
                {
                    continueAdding = false;
                }
            }

            ViewInventory(true);

            SaveInventoryData();

            Console.Clear();
        }
        public void UpdateInventory()
        {
            bool continueUpdating = true;

            while (continueUpdating)
            {
                Console.Clear();

                var table = new Table();
                table.AddColumn("[cyan]Item[/]");
                table.AddColumn("[cyan]Batch Number[/]");
                table.AddColumn("[cyan]Category[/]");
                table.AddColumn("[cyan]Location[/]");
                table.AddColumn("[cyan]Quantity[/]");
                table.AddColumn("[cyan]Weight[/]");
                table.AddColumn("[cyan]Weight Unit[/]");
                table.AddColumn("[cyan]Packaging[/]");
                table.AddColumn("[cyan]Expiration Date[/]");
                table.AddColumn("[cyan]Price[/]");
                table.AddColumn("[cyan]Status[/]");

                foreach (var item in inventory)
                {
                    var details = item.Value;
                    string expiration = details.expirationDate.HasValue ? details.expirationDate.Value.ToShortDateString() : "[yellow]No Expiration[/]";
                    string status = details.expirationDate.HasValue
                        ? (details.expirationDate.Value < DateTime.Now ? "[red]Expired[/]" : "[green]Valid[/]")
                        : "[yellow]N/A[/]";
                    table.AddRow(item.Key, details.batchNumber, details.category, details.location, details.quantity.ToString(), details.weight, details.weightUnit, details.packaging, expiration, status);
                }
                AnsiConsole.Write(table);

                string itemName = AnsiConsole.Ask<string>("Enter the [cyan]item name[/] (or type [bold red]cancel[/] to exit):");
                if (itemName.ToLower() == "cancel") return;

                if (inventory.ContainsKey(itemName))
                {
                    var currentItem = inventory[itemName];

                    bool fieldUpdateDone = false;
                    while (!fieldUpdateDone)
                    {
                        string option = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("Which [cyan]field[/] would you like to update?")
                                .AddChoices("Category", "Location", "Quantity", "Weight", "Weight Unit", "Packaging", "Expiration Date", "Done"));

                        switch (option.ToLower())
                        {
                            case "category":
                                string newCategory = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title($"Current category: [green]{currentItem.category}[/]. Select [cyan]new category[/]):")
                                        .AddChoices(categories));
                                if (newCategory != "cancel")
                                {
                                    inventory[itemName] = (currentItem.batchNumber, newCategory, currentItem.location, currentItem.quantity, currentItem.weight, currentItem.weightUnit, currentItem.packaging, currentItem.expirationDate, currentItem.price);
                                    AnsiConsole.MarkupLine($"[green]Category updated to {newCategory}.[/]");
                                }
                                break;

                            case "location":
                                string newLocation = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title($"Current location: [green]{currentItem.location}[/]. Enter new location):")
                                        .AddChoices(locations));
                                if (newLocation != "cancel")
                                {
                                    inventory[itemName] = (currentItem.batchNumber, currentItem.category, newLocation, currentItem.quantity, currentItem.weight, currentItem.weightUnit, currentItem.packaging, currentItem.expirationDate, currentItem.price);
                                    AnsiConsole.MarkupLine($"[green]Location updated to {newLocation}.[/]");
                                }
                                break;

                            case "quantity":
                                int newQuantity;
                                while (true)
                                {
                                    string quantityInput = AnsiConsole.Ask<string>($"Current quantity: [green]{currentItem.quantity}[/]. Enter new quantity (or type [bold red]cancel[/] to cancel):");
                                    if (quantityInput.ToLower() == "cancel")
                                    {
                                        newQuantity = currentItem.quantity;
                                        break;
                                    }

                                    if (int.TryParse(quantityInput, out newQuantity) && newQuantity >= 0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        AnsiConsole.MarkupLine("[red]Invalid quantity. Quantity must be a non-negative integer.[/]");
                                    }
                                }
                                if (newQuantity != currentItem.quantity)
                                {
                                    inventory[itemName] = (currentItem.batchNumber, currentItem.category, currentItem.location, newQuantity, currentItem.weight, currentItem.weightUnit, currentItem.packaging, currentItem.expirationDate, currentItem.price);
                                    AnsiConsole.MarkupLine($"[green]Quantity updated to {newQuantity}.[/]");
                                }
                                break;

                            case "weight":
                                string newWeight;
                                while (true)
                                {
                                    newWeight = AnsiConsole.Ask<string>($"Current weight: [green]{currentItem.weight}[/]. Enter new weight (or type [bold red]cancel[/] to cancel):");
                                    if (newWeight.ToLower() == "cancel")
                                    {
                                        newWeight = currentItem.weight;
                                        break;
                                    }

                                    if (double.TryParse(newWeight, out double weightValue) && weightValue > 0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        AnsiConsole.MarkupLine("[red]Invalid weight. Weight must be   greater than 0.[/]");
                                    }
                                }
                                if (newWeight != currentItem.weight)
                                {
                                    inventory[itemName] = (currentItem.batchNumber, currentItem.category, currentItem.location, currentItem.quantity, newWeight, currentItem.weightUnit, currentItem.packaging, currentItem.expirationDate, currentItem.price);
                                    AnsiConsole.MarkupLine($"[green]Weight updated to {newWeight}.[/]");
                                }
                                break;

                            case "weight unit":
                                string newWeightUnit = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title($"Current weight unit: [green]{currentItem.weightUnit}[/]. Enter new weight unit):")
                                        .AddChoices(weightUnit));
                                if (newWeightUnit != "cancel")
                                {
                                    inventory[itemName] = (currentItem.batchNumber, currentItem.category, currentItem.location, currentItem.quantity, currentItem.weight, newWeightUnit, currentItem.packaging, currentItem.expirationDate, currentItem.price);
                                    AnsiConsole.MarkupLine($"[green]Weight unit updated to {newWeightUnit}.[/]");
                                }
                                break;

                            case "packaging":
                                string newPackaging = AnsiConsole.Ask<string>($"Current packaging: [green]{currentItem.packaging}[/]. Enter new packaging (or type [bold red]cancel[/] to cancel):");
                                if (newPackaging != "cancel")
                                {
                                    inventory[itemName] = (currentItem.batchNumber, currentItem.category, currentItem.location, currentItem.quantity, currentItem.weight, currentItem.weightUnit, newPackaging, currentItem.expirationDate, currentItem.price);
                                    AnsiConsole.MarkupLine($"[green]Packaging updated to {newPackaging}.[/]");
                                }
                                break;

                            case "expiration date":
                                DateTime? newExpirationDate = currentItem.expirationDate;
                                while (true)
                                {
                                    string expirationInput = AnsiConsole.Ask<string>(
                                        $"Current expiration date: [green]{(currentItem.expirationDate.HasValue ? currentItem.expirationDate.Value.ToShortDateString() : "No Expiration")}[/]. Enter new expiration date (yyyy-MM-dd) (or type [bold red]cancel[/] to cancel):");

                                    if (expirationInput.ToLower() == "cancel")
                                    {
                                        break;
                                    }

                                    if (DateTime.TryParseExact(expirationInput, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                                    {
                                        newExpirationDate = parsedDate;
                                        break;
                                    }
                                    else
                                    {
                                        AnsiConsole.MarkupLine("[red]Invalid date format. Please enter the date in the format yyyy-MM-dd.[/]");
                                    }
                                }
                                if (newExpirationDate != currentItem.expirationDate)
                                {
                                    inventory[itemName] = (currentItem.batchNumber, currentItem.category, currentItem.location, currentItem.quantity, currentItem.weight, currentItem.weightUnit, currentItem.packaging, newExpirationDate, currentItem.price);
                                    AnsiConsole.MarkupLine($"[green]Expiration date updated to {newExpirationDate?.ToShortDateString() ?? "No Expiration"}.[/]");
                                }
                                break;

                            case "done":
                                fieldUpdateDone = true;
                                AnsiConsole.MarkupLine($"[green]Item '{itemName}' has been updated![/]");
                                break;

                            default:
                                AnsiConsole.MarkupLine("[red]Invalid option.[/]");
                                break;
                        }
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Item not found in inventory.[/]");
                }

                string updateAnother = AnsiConsole.Ask<string>("Would you like to update another item? (y/n):").ToLower();

                Console.Clear();

                if (updateAnother != "y")
                {
                    continueUpdating = false;
                }
            }

            ViewInventory(true);

            SaveInventoryData();

            Console.Clear();
        }
        public void RemoveInventory()
        {
            bool continueRemoving = true;

            while (continueRemoving)
            {
                var table = new Table();
                table.AddColumn("[cyan]Item[/]");
                table.AddColumn("[cyan]Batch Number[/]");
                table.AddColumn("[cyan]Category[/]");
                table.AddColumn("[cyan]Location[/]");
                table.AddColumn("[cyan]Quantity[/]");
                table.AddColumn("[cyan]Weight[/]");
                table.AddColumn("[cyan]Weight Unit[/]");
                table.AddColumn("[cyan]Packaging[/]");
                table.AddColumn("[cyan]Expiration Date[/]");
                table.AddColumn("[cyan]Status[/]");
                table.AddColumn("[cyan]Price[/]");

                foreach (var item in inventory)
                {
                    var details = item.Value;
                    string expiration = details.expirationDate.HasValue ? details.expirationDate.Value.ToShortDateString() : "[yellow]No Expiration[/]";
                    string status = details.expirationDate.HasValue
                        ? (details.expirationDate.Value < DateTime.Now ? "[red]Expired[/]" : "[green]Valid[/]")
                        : "[yellow]N/A[/]";
                    table.AddRow(item.Key, details.batchNumber, details.category, details.location, details.quantity.ToString(), details.weight, details.weightUnit, details.packaging, expiration, status);
                }
                AnsiConsole.Write(table);

                string itemName;
                while (true)
                {
                    itemName = AnsiConsole.Ask<string>("Enter the [cyan]item name[/] (or type [bold red]cancel[/] to exit):");

                    if (itemName.ToLower() == "cancel") return;

                    if (inventory.ContainsKey(itemName))
                    {
                        inventory.Remove(itemName);
                        AnsiConsole.MarkupLine($"[green]Item '{itemName}' removed from inventory.[/]");
                        break;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Item not found in inventory. Please enter a valid item name.[/]");
                    }
                }

                string removeAnother = AnsiConsole.Ask<string>("Would you like to remove another item? (y/n):").ToLower();

                Console.Clear();

                if (removeAnother != "y")
                {
                    continueRemoving = false;
                }
            }

            ViewInventory(true);

            SaveInventoryData();

            Console.Clear();
        }
        public void ViewInventory(bool pauseAfterView)
        {
            Console.Clear();
            if (inventory.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Inventory is empty![/]");
            }
            else
            {
                var table = new Table();
                table.AddColumn("[cyan]Item[/]");
                table.AddColumn("[cyan]Batch Number[/]");
                table.AddColumn("[cyan]Category[/]");
                table.AddColumn("[cyan]Location[/]");
                table.AddColumn("[cyan]Quantity[/]");
                table.AddColumn("[cyan]Weight[/]");
                table.AddColumn("[cyan]Weight Unit[/]");
                table.AddColumn("[cyan]Packaging[/]");
                table.AddColumn("[cyan]Expiration Date[/]");
                table.AddColumn("[cyan]Status[/]");
                table.AddColumn("[cyan]Price[/]");

                foreach (var item in inventory)
                {
                    var details = item.Value;
                    string expiration = details.expirationDate.HasValue ? details.expirationDate.Value.ToShortDateString() : "[yellow]No Expiration[/]";
                    string status = details.expirationDate.HasValue
                        ? (details.expirationDate.Value < DateTime.Now ? "[red]Expired[/]" : "[green]Valid[/]")
                        : "[yellow]N/A[/]";
                    table.AddRow(item.Key, details.batchNumber, details.category, 
                        details.location, details.quantity.ToString(), 
                        details.weight, details.weightUnit, details.packaging, 
                        expiration, status, "PHP " + details.price.ToString("F2"));
                }
                AnsiConsole.Write(table);
            }

            if (pauseAfterView)
            {
                AnsiConsole.MarkupLine("\n[grey]Press any key to return to the menu...[/]");
                Console.ReadKey();
            }
            Console.Clear();
        }
        public void SaveInventoryData()
        {
            string filePath = "inventory.txt";

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var item in inventory)
                    {
                        var details = item.Value;
                        writer.WriteLine($"{item.Key} " +
                            $"| {details.batchNumber} " +
                            $"| {details.category} " +
                            $"| {details.location} " +
                            $"| {details.quantity} " +
                            $"| {details.weight} " +
                            $"| {details.weightUnit} " +
                            $"| {details.packaging} " +
                            $"| {details.expirationDate?.ToString("yyyy-MM-dd") ?? "N/A"} " +
                            $"| {details.price.ToString("C", new System.Globalization.CultureInfo("en-PH"))}");
                    }
                }
                Console.WriteLine("{green}Inventory data saved successfully.[/]");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Error: Access to the file path is denied. Please check your permissions.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O Error while saving the file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while saving the file: {ex.Message}");
            }
        }
        public void LoadInventoryData()
        {
            string filePath = "inventory.txt";

            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string? line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(" | ");

                            if (parts.Length == 10)
                            {
                                try
                                {
                                    string itemName = parts[0];
                                    string batchNumber = parts[1];
                                    string category = parts[2];
                                    string location = parts[3];
                                    int quantity = int.Parse(parts[4]);
                                    string weight = parts[5];
                                    string weightUnit = parts[6];
                                    string packaging = parts[7];
                                    DateTime? expirationDate = DateTime.TryParse(parts[8], out DateTime expDate) ? expDate : (DateTime?)null;
                                    decimal price = decimal.Parse(parts[9].Replace("₱", "").Trim());

                                    inventory[itemName] = (batchNumber, category, location, quantity, weight, weightUnit, packaging, expirationDate, price);
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine($"Invalid data format in line: {line}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error processing line: {line}. {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Skipping invalid data format: {line}");
                            }
                        }
                    }
                    Console.WriteLine("[green]Inventory data loaded successfully.[/]");
                }
                else
                {
                    Console.WriteLine("Error: Inventory file not found.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Error: Access to the file path is denied. Please check your permissions.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O Error while reading the file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while loading the file: {ex.Message}");
            }
        }
    }
}
