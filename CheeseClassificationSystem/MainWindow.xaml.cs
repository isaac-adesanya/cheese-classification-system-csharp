using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;

namespace CheeseClassificationSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CreateDataBase(); //runs the function to create the table in database
            LoadCheeseList(); //loads the cheese list on startup
        }
        String connectionString = "Data Source=cheese.db"; //connection string for the SQLite database

        private void CreateDataBase() //creates the Cheese table if it doesn't exist
        {
            using (var connection = new SqliteConnection(connectionString)) //creates a new connection to the database
            {
                connection.Open(); //opens the connection to the database

                var tableCmd = connection.CreateCommand(); //creates the command to create the Cheese table
                tableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Cheese (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        Type TEXT NOT NULL,
                        Description TEXT NOT NULL,
                        BatchNumber INTEGER NOT NULL, 
                        CreationDate TEXT NOT NULL,
                        NumberOfCheeses INTEGER NOT NULL
                    );"; //SQL command to create the Cheese table
                tableCmd.ExecuteNonQuery(); //executes the command
            }
        }
        
        private void InsertCheese(Cheese newCheese) //inserts a new cheese into the Cheese table
        {
            using (var connection = new SqliteConnection(connectionString)) //creates a new connection to the database
            {
                connection.Open(); //opens the connection to the database

                var cmd = connection.CreateCommand(); //creates the command to insert a new cheese
                cmd.CommandText = @"
                    INSERT INTO Cheese(Type, Description, BatchNumber, CreationDate, NumberOfCheeses)
                    VALUES ($type, $description, $batch, $date, $number);"; //SQL command to insert a new cheese

                cmd.Parameters.AddWithValue("$type", newCheese.Type); //adds the parameters to the command
                cmd.Parameters.AddWithValue("$description", newCheese.Description);
                cmd.Parameters.AddWithValue("$batch", newCheese.BatchNumber);
                cmd.Parameters.AddWithValue("$date", newCheese.CreationDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("$number", newCheese.NumberOfCheeses);

                cmd.ExecuteNonQuery(); //executes the command
            }
        }

        private void DeleteCheese(int batchNumber) //deletes a cheese from the Cheese table based on batch number
        {
            using (var connection = new SqliteConnection(connectionString)) //creates a new connection to the database
            {
                connection.Open(); //opens the connection to the database

                var cmd = connection.CreateCommand(); //creates the command to delete a cheese
                cmd.CommandText = @"DELETE FROM Cheese WHERE BatchNumber = $batch;"; //SQL command to delete a cheese
                cmd.Parameters.AddWithValue("$batch",batchNumber); //adds the parameter to the command
                cmd.ExecuteNonQuery(); //executes the command
            }
        }
        private void UpdateCheese(Cheese cheese) //updates a cheese in the Cheese table
        {
            using (var connection = new SqliteConnection(connectionString)) //creates a new connection to the database
            {
                connection.Open(); //opens the connection to the database   

                var cmd = connection.CreateCommand(); //creates the command to update a cheese
                cmd.CommandText = @"
                    UPDATE Cheese 
                    SET NumberOfCheeses = $number
                    WHERE BatchNumber = $batch;"; //SQL command to update a cheese

                cmd.Parameters.AddWithValue("$number", cheese.NumberOfCheeses); //adds the parameters to the command
                cmd.Parameters.AddWithValue("$batch", cheese.BatchNumber);

                cmd.ExecuteNonQuery(); //executes the command
            }
        }
        private Cheese GetCheeseByBatch(int batchNumber) //retrieves a cheese from the Cheese table based on batch number
        {
            using (var connection = new SqliteConnection(connectionString)) //creates a new connection to the database
            {
                connection.Open(); //opens the connection to the database

                var cmd = connection.CreateCommand(); //creates the command to retrieve a cheese
                cmd.CommandText = @"
                    SELECT * FROM Cheese
                    WHERE BatchNumber = $batch;"; //SQL command to retrieve a cheese
                cmd.Parameters.AddWithValue("$batch", batchNumber);  //adds the parameter to the command

                using (var reader = cmd.ExecuteReader()) //executes the command and reads the result
                {
                    if (reader.Read()) //if a cheese is found
                    {
                        return new Cheese( //creates a new Cheese object from the retrieved data
                            reader.GetInt32(0),            
                            reader.GetString(1), 
                            reader.GetString(2),
                            reader.GetInt32(3),            
                            DateTime.Parse(reader.GetString(4)),
                            reader.GetInt32(5)
                        );
                    }
                }
                MessageBox.Show("No cheese found with that batch number."); //alerts the user if no cheese is found
                ClearInputs(); //clears the input fields
                return null; //returns null if no cheese is found
            }
        } 

        private void FilterCheeses() //filters the cheese list based on user input
        {
            var cheeses = GetCheeses(); //retrieves all cheeses

            if (cmbCheeseType.SelectedItem is ComboBoxItem selectedItem) //checks if a cheese type is selected
            {
                string type = selectedItem.Content.ToString(); //gets the selected cheese type
                cheeses = cheeses.Where(c => c.Type == type).ToList(); //filters the cheese list based on the selected type
            } 

            if (int.TryParse(txtBatchNumber.Text, out int batchNumber)) //checks if a valid batch number is entered
            {
                cheeses = cheeses.Where(c => c.BatchNumber == batchNumber).ToList(); //filters the cheese list based on the entered batch number
            }

            lstDisplay.ItemsSource = cheeses; //updates the ListBox with the filtered cheese list

            if (lstDisplay.Items.Count == 0) //if no cheeses match the filter criteria
            {
                MessageBox.Show("No cheeses found matching the criteria."); //alerts the user
                ClearInputs(); //clears the input fields
            }
        }
        private void lstDisplay_Selected(object sender, SelectionChangedEventArgs e) //populates the input fields when a cheese is selected from the ListBox
        {
            if (lstDisplay.SelectedItem is Cheese selectedCheese) //checks if a cheese is selected
            {
                cmbCheeseType.SelectedItem = cmbCheeseType.Items 
                .Cast<ComboBoxItem>() //casts the items in the ComboBox to ComboBoxItem
                .FirstOrDefault(i => i.Content.ToString() == selectedCheese.Type); //selects the ComboBoxItem that matches the selected cheese type

                // TextBoxes
                txtDescription.Text = selectedCheese.Description; //sets the description TextBox to the selected cheese's description
                txtBatchNumber.Text = selectedCheese.BatchNumber.ToString(); //sets the batch number TextBox to the selected cheese's batch number
                txtCheeseDate.Text = selectedCheese.CreationDate.ToString("yyyy-MM-dd"); //sets the creation date TextBox to the selected cheese's creation date
                txtNumberOfCheeses.Text = selectedCheese.NumberOfCheeses.ToString(); //sets the number of cheeses TextBox to the selected cheese's number of cheeses
            }
        }


        private void btnSubmitCheese_Click(object sender, RoutedEventArgs e) //handles the click event for the Submit Cheese button
        {
            string type = (cmbCheeseType.SelectedItem as ComboBoxItem)?.Content.ToString(); //gets the selected cheese type
            try 
            {
                if (!int.TryParse(txtBatchNumber.Text, out int batchNumber)) //validates the batch number input
                    throw new Exception("Batch number must be a valid number");
                if (!txtCheeseDate.SelectedDate.HasValue) //validates the creation date input
                    throw new Exception("Please select a creation date.");
                DateTime creationDate = txtCheeseDate.SelectedDate.Value;
                if (!int.TryParse(txtNumberOfCheeses.Text, out int numberOfCheeses))//validates the number of cheeses input
                    throw new Exception("Number of cheeses must be a vlaid number");
                string description = txtDescription.Text; //gets the description input
                Cheese newCheese = new Cheese(type, description, batchNumber, creationDate, numberOfCheeses); //creates a new Cheese object

                InsertCheese(newCheese); //inserts the new cheese into the database
                LoadCheeseList(); //reloads the cheese list

                MessageBox.Show("Cheese created: " + newCheese.Type); //shows a message box confirming the cheese creation
                ClearInputs(); //clears the input fields
            }
            catch (Exception ex) //catches any exceptions that occur during the process
            {
                MessageBox.Show("Input Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

           private void btnDeleteCheese_Click(object sender, RoutedEventArgs e) //handles the click event for the Delete Cheese button
        {
            // Validate inputs
            if (!int.TryParse(txtBatchNumber.Text, out int batchNumber))
            {
                MessageBox.Show("Please enter a valid batch number.");
                return;
            }

            if (!int.TryParse(txtNumberOfCheeses.Text, out int numberToDelete))
            {
                MessageBox.Show("Please enter a valid number to delete.");
                return;
            }

            Cheese cheese = GetCheeseByBatch(batchNumber); // Load cheese by batch

            if (cheese == null) //if no cheese is found with the given batch number
            {
                MessageBox.Show("No cheese found with that batch number."); //
                return;
            }

            cheese.NumberOfCheeses -= numberToDelete; // Decrease the number of cheeses

            if (cheese.NumberOfCheeses <= 0) // If no cheeses left
            {
                // Delete whole batch
                DeleteCheese(batchNumber);
                MessageBox.Show($"Batch {batchNumber} removed entirely.");
            }
            else
            {
                // Update remaining
                UpdateCheese(cheese);
                MessageBox.Show($"Updated batch {batchNumber}. New total: {cheese.NumberOfCheeses}");
            }
            LoadCheeseList();
        }

        private void btnSearchCheese_Click(object sender, RoutedEventArgs e) //handles the click event for the Search Cheese button
        {
            FilterCheeses(); //filters the cheese list based on user input by calling the FilterCheeses method
        }

        // Clears all input fields
        private void ClearInputs()
        {
            txtDescription.Text = string.Empty;
            txtBatchNumber.Text = string.Empty;
            txtCheeseDate.SelectedDate = null;
            txtNumberOfCheeses.Text = string.Empty;
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e) //handles the click event for the Show All button
        {
            LoadCheeseList(); //reloads the cheese list to show all cheeses
            ClearInputs(); //clears the input fields
        }

        public List<Cheese> GetCheeses() //retrieves all cheeses from the Cheese table
        {
            List<Cheese> cheeseList = new List<Cheese>(); //creates a new list to hold the retrieved cheeses
            using (var connection = new SqliteConnection(connectionString)) //creates a new connection to the database
            {
                connection.Open(); //opens the connection to the database

                var cmd = connection.CreateCommand(); //creates the command to retrieve all cheeses
                cmd.CommandText = @"
                    SELECT * FROM Cheese;"; //SQL command to retrieve all cheeses
                cmd.ExecuteNonQuery(); //executes the command

                using (var reader = cmd.ExecuteReader()) //reads the result of the command
                {
                    while (reader.Read()) //loops through the retrieved cheeses
                    {
                        cheeseList.Add(new Cheese( //creates a new Cheese object from the retrieved data and adds it to the list
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetInt32(3),
                            DateTime.Parse(reader.GetString(4)),
                            reader.GetInt32(5)
                            ));
                    }
                }
             return cheeseList; //returns the list of retrieved cheeses
            }
        }

        private void LoadCheeseList() //loads the cheese list into the ListBox
        {
            lstDisplay.ItemsSource = GetCheeses(); //sets the ItemsSource of the ListBox to the list of retrieved cheeses
        }
    }
}