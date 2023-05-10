using System.Data.SqlClient;

namespace Expense_database
{
    internal class Program
    {


        static void Main(string[] args)
        {
            SqlConnection con = new SqlConnection("Data Source=IN-4LSQ8S3; Initial Catalog=ExpenseDB; Integrated Security=true");

            while (true)
            {
                Console.WriteLine("Expense Tracker Menu");
                Console.WriteLine("1. Add Transaction");
                Console.WriteLine("2. View Expenses");
                Console.WriteLine("3. View Income");
                Console.WriteLine("4. Check Available Balance");
                Console.Write("\nEnter your choice (1-4): ");

                int choice;
                if (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 4)
                {
                    Console.WriteLine("\nWrong Choice Entered. Try Again!\n");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        AddTransaction(con);
                        break;
                    case 2:
                        ViewTransactions(con, "Expense");
                        break;
                    case 3:
                        ViewTransactions(con, "Income");
                        break;
                    case 4:
                        Console.WriteLine("\nAvailable Balance: {0}\n", GetBalance(con));
                        break;
                }
            }
        }

        static void AddTransaction(SqlConnection connection)
        {
            Console.Write("\nEnter Title: ");
            string title = Console.ReadLine();

            Console.Write("Enter Description: ");
            string description = Console.ReadLine();

            Console.Write("Enter Amount: ");
            decimal amount;
            if (!decimal.TryParse(Console.ReadLine(), out amount))
            {
                Console.WriteLine("\nInvalid amount entered. Try Again!\n");
                return;
            }

            Console.Write("Enter Date (MM/DD/YYYY): ");
            DateTime date;
            if (!DateTime.TryParse(Console.ReadLine(), out date) || date.Year > 2023)
            {
                Console.WriteLine("\nInvalid date entered. Try Again!\n");
                return;
            }

            string type = amount < 0 ? "Expense" : "Income";

            string query = "INSERT INTO Transactions (Title, Description, Amount, Date, Type) VALUES (@Title, @Description, @Amount, @Date, @Type)";
            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@Date", date);
            command.Parameters.AddWithValue("@Type", type);

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();
            connection.Close();

            if (rowsAffected > 0)
            {
                Console.WriteLine("\nTransaction added successfully.\n");
            }
            else
            {
                Console.WriteLine("\nError adding transaction. Try Again!\n");
            }
        }

        static void ViewTransactions(SqlConnection connection, string type)
        {
            string header = type == "Expense" ? "Expense Transactions" : "Income Transactions";
            Console.WriteLine("\n{0}\n", header);

            string query = "SELECT * FROM Transactions WHERE Type=@Type";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Type", type);

            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine("Title: {0}\nDescription: {1}\nAmount: {2}\nDate: {3}\n",
                 reader["Title"], reader["Description"], reader["Amount"], ((DateTime)reader["Date"]).ToShortDateString());
            }

            reader.Close();
            connection.Close();
        }

        static decimal GetBalance(SqlConnection connection)
        {
            decimal totalIncome = 0;
            decimal totalExpense = 0;
            decimal balance = 0;

            try
            {
                connection.Open();

                SqlCommand cmdIncome = new SqlCommand("SELECT SUM(Amount) FROM Transactions WHERE Type='Income'", connection);
                SqlCommand cmdExpense = new SqlCommand("SELECT SUM(Amount) FROM Transactions WHERE Type='Expense'", connection);

                SqlDataReader readerIncome = cmdIncome.ExecuteReader();
                if (readerIncome.HasRows)
                {
                    while (readerIncome.Read())
                    {
                        if (!readerIncome.IsDBNull(0))
                        {
                            totalIncome = readerIncome.GetDecimal(0);
                        }
                    }
                }
                readerIncome.Close();

                SqlDataReader readerExpense = cmdExpense.ExecuteReader();
                if (readerExpense.HasRows)
                {
                    while (readerExpense.Read())
                    {
                        if (!readerExpense.IsDBNull(0))
                        {
                            totalExpense = readerExpense.GetDecimal(0);
                        }
                    }
                }
                readerExpense.Close();

                
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return totalIncome + totalExpense;
        }
    }
}