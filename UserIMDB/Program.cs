using System;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Data;

namespace UserIMDB
{
    class Program
    {
        static string connectionString = "server=localhost;database=IMDB;" +
                                         "user id=oliverpeb;password=Frost3310;TrustServerCertificate=True;";

        static void Main(string[] args)
        {
            PerformUserActions();
        }

        static void PerformUserActions()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                Console.WriteLine("Choose an action:");
                Console.WriteLine("1: Search Titles");
                Console.WriteLine("2: Search Names");
                Console.WriteLine("3: Add Movie");
                Console.WriteLine("4: Add Person");
                Console.WriteLine("5: Update Movie");
                Console.WriteLine("6: Delete Movie");
                var actionChoice = Console.ReadLine();

                switch (actionChoice)
                {
                    case "1":
                        Console.WriteLine("Enter title to search for:");
                        string titleSearchTerm = Console.ReadLine();
                        SearchTitles(connection, titleSearchTerm);
                        break;
                    case "2":
                        Console.WriteLine("Enter name to search for:");
                        string nameSearchTerm = Console.ReadLine();
                        SearchNames(connection, nameSearchTerm);
                        break;
                    case "3":
                        AddMovie(connection);
                        break;
                    case "4":
                        AddPerson(connection);
                        break;
                    case "5":
                        UpdateMovie(connection);
                        break;
                    case "6":
                        DeleteMovie(connection);
                        break;
                    default:
                        Console.WriteLine("Invalid action selected.");
                        break;
                }

                connection.Close();
            }
            ExitPrompt();
        }


        // Add the second parameter 'searchQuery' which is passed from the PerformUserActions method
        static void SearchTitles(SqlConnection connection, string searchQuery)
        {
            string searchCommand = "SELECT tconst, primaryTitle FROM Titles WHERE primaryTitle LIKE @SearchQuery ORDER BY primaryTitle ASC";
            using (var command = new SqlCommand(searchCommand, connection))
            {
                command.Parameters.AddWithValue("@SearchQuery", "%" + searchQuery + "%");

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"ID: {reader["tconst"]}, Title: {reader["primaryTitle"]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No titles found.");
                    }
                }
            }
        }



        static void SearchNames(SqlConnection connection, string searchTerm)
        {
            Console.WriteLine("Enter name to search for:");

            string searchCommand = "SELECT nconst, primaryName FROM Names WHERE primaryName LIKE @SearchParam ORDER BY primaryName ASC";
            using (var command = new SqlCommand(searchCommand, connection))
            {
                command.Parameters.AddWithValue("@SearchParam", "%" + searchTerm + "%");

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"ID: {reader["nconst"]}, Name: {reader["primaryName"]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No names found.");
                    }
                }
            }
        }

        static void AddMovie(SqlConnection connection)
        {
            Console.WriteLine("Enter new movie details:");

            // Gather all the inputs from the user
            Console.Write("Title Type: ");
            string titleType = Console.ReadLine();
            Console.Write("Primary Title: ");
            string primaryTitle = Console.ReadLine();
            Console.Write("Original Title: ");
            string originalTitle = Console.ReadLine();
            Console.Write("Is Adult (1 for Yes, 0 for No): ");
            bool isAdult = Convert.ToBoolean(Convert.ToInt32(Console.ReadLine()));
            Console.Write("Start Year: ");
            int startYear = Convert.ToInt32(Console.ReadLine());
            Console.Write("End Year (or press Enter if unknown): ");
            int? endYear = null;
            string endYearInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(endYearInput))
            {
                endYear = Convert.ToInt32(endYearInput);
            }
            Console.Write("Runtime Minutes: ");
            int runtimeMinutes = Convert.ToInt32(Console.ReadLine());

            // Call the stored procedure
            using (var command = new SqlCommand("AddMovie", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@TitleType", titleType);
                command.Parameters.AddWithValue("@PrimaryTitle", primaryTitle);
                command.Parameters.AddWithValue("@OriginalTitle", originalTitle);
                command.Parameters.AddWithValue("@IsAdult", isAdult);
                command.Parameters.AddWithValue("@StartYear", startYear);
                command.Parameters.AddWithValue("@EndYear", (object)endYear ?? DBNull.Value);
                command.Parameters.AddWithValue("@RuntimeMinutes", runtimeMinutes);

                // Check if the connection is open
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                // Execute the command and retrieve the new movie ID
                var result = command.ExecuteScalar();
                
                if (result != null)
                {
                    Console.WriteLine($"Movie added successfully with ID: {result}");
                }
                else
                {
                    Console.WriteLine("Failed to add movie.");
                }
            }
        }


        static void AddPerson(SqlConnection connection)
        {
            Console.WriteLine("Enter new person details:");

            Console.Write("Primary Name: ");
            string primaryName = Console.ReadLine();

            Console.Write("Birth Year (or press Enter if unknown): ");
            int? birthYear = null;
            string birthInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(birthInput))
            {
                birthYear = Convert.ToInt32(birthInput);
            }

            Console.Write("Death Year (or press Enter if unknown): ");
            int? deathYear = null;
            string deathInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(deathInput))
            {
                deathYear = Convert.ToInt32(deathInput);
            }

            using (var command = new SqlCommand("AddPerson", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@PrimaryName", primaryName);
                command.Parameters.AddWithValue("@BirthYear", birthYear.HasValue ? (object)birthYear : DBNull.Value);
                command.Parameters.AddWithValue("@DeathYear", deathYear.HasValue ? (object)deathYear : DBNull.Value);

                // Check if the connection is already open
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                try
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Person added successfully.");
                }
                catch (SqlException e)
                {
                    // Handle any errors returned by the stored procedure here
                    Console.WriteLine("Failed to add person. Error: " + e.Message);
                }
            }
        }

        static void UpdateMovie(SqlConnection connection)
        {
            Console.WriteLine("Enter the ID of the movie to update:");
            string tconst = Console.ReadLine();

            Console.WriteLine("Enter new movie details (leave blank to keep current value):");

            Console.Write("Primary Title: ");
            string primaryTitle = Console.ReadLine();

            Console.Write("Title Type: ");
            string titleType = Console.ReadLine();

            Console.Write("Original Title: ");
            string originalTitle = Console.ReadLine();

            Console.Write("Is Adult (1 for Yes, 0 for No): ");
            string isAdult = Console.ReadLine();

            Console.Write("Start Year: ");
            string startYear = Console.ReadLine();

            Console.Write("End Year (or press Enter if not applicable): ");
            string endYear = Console.ReadLine();

            Console.Write("Runtime Minutes: ");
            string runtimeMinutes = Console.ReadLine();



            using (var command = new SqlCommand("UpdateMovie", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Tconst", tconst);
                command.Parameters.AddWithValue("@PrimaryTitle", string.IsNullOrEmpty(primaryTitle) ? (object)DBNull.Value : primaryTitle);
                command.Parameters.AddWithValue("@TitleType", string.IsNullOrEmpty(titleType) ? (object)DBNull.Value : titleType);
                command.Parameters.AddWithValue("@OriginalTitle", string.IsNullOrEmpty(originalTitle) ? (object)DBNull.Value : originalTitle);
                command.Parameters.AddWithValue("@IsAdult", string.IsNullOrEmpty(isAdult) ? (object)DBNull.Value : isAdult);
                command.Parameters.AddWithValue("@StartYear", string.IsNullOrEmpty(startYear) ? (object)DBNull.Value : startYear);
                command.Parameters.AddWithValue("@EndYear", string.IsNullOrEmpty(endYear) ? (object)DBNull.Value : endYear);
                command.Parameters.AddWithValue("@RuntimeMinutes", string.IsNullOrEmpty(runtimeMinutes) ? (object)DBNull.Value : runtimeMinutes);

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                int result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    Console.WriteLine("Movie updated successfully.");
                }
                else
                {
                    Console.WriteLine("No changes were made.");
                }
            }
        }

        static void DeleteMovie(SqlConnection connection)
        {
            Console.WriteLine("Enter the ID of the movie to delete:");
            string tconst = Console.ReadLine();

            using (var command = new SqlCommand("DeleteMovie", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add parameter with value
                command.Parameters.AddWithValue("@Tconst", tconst);

                // Execute the stored procedure
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                int result = command.ExecuteNonQuery();
                if (result > 0)
                    Console.WriteLine("Movie deleted successfully.");
                else
                    Console.WriteLine("Failed to delete movie.");
            }
        }


        static void ExitPrompt()
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }


