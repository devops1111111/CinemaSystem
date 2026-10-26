using System;

namespace CinemaBookingSystem
{
    public class Program
    {
        private static CinemaSystem cinemaSystem;

        public static void Main(string[] args)
        {
            try
            {
                cinemaSystem = new CinemaSystem();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing Cinema System: " + ex.Message);
                Console.WriteLine("If CSV files exist, try deleting them (movies.csv, showings.csv) and run again.");
                Pause();
                return;
            }

            bool running = true;

            while (running)
            {
                DisplayMenu();
                string input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1":
                        cinemaSystem.DisplayMoviesAndShowings();
                        Pause();
                        break;
                    case "2":
                        cinemaSystem.BuyTicket();
                        Pause();
                        break;
                    case "3":
                        cinemaSystem.ScheduleVipMovie();
                        Pause();
                        break;
                    case "4":
                        running = false;
                        Console.WriteLine("Exiting Cinema System. Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please select 1-4.");
                        Pause();
                        break;
                }
            }
        }

        private static void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("=================================================");
            Console.WriteLine("          CINEMA CENTER BOOKING SYSTEM           ");
            Console.WriteLine("=================================================");
            Console.WriteLine("1. Movie List | Showings");
            Console.WriteLine("2. Buy a Ticket");
            Console.WriteLine("3. Schedule a Movie (VIP)");
            Console.WriteLine("4. Exit");
            Console.WriteLine("=================================================");
            Console.Write("Select an option (1-4): ");
        }

        private static void Pause()
        {
            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }
    }
}