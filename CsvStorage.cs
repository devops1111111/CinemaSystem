using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CinemaBookingSystem
{
    public static class CsvStorage
    {
        private static readonly string MoviesFilePath = "movies.csv";
        private static readonly string ShowingsFilePath = "showings.csv";
        private static readonly string TicketsFilePath = "tickets.csv";

        public static void SaveMovies(List<Movie> movies)
        {
            using (var writer = new StreamWriter(MoviesFilePath))
            {
                writer.WriteLine("Id,Title,Genre,Rating,DurationMinutes,Description");
                foreach (var m in movies)
                {
                    writer.WriteLine($"{m.Id},\"{EscapeCsv(m.Title)}\",\"{EscapeCsv(m.Genre)}\",\"{EscapeCsv(m.Rating)}\",{m.Duration.TotalMinutes.ToString(CultureInfo.InvariantCulture)},\"{EscapeCsv(m.Description)}\"");
                }
            }
        }

        public static List<Movie> LoadMovies()
        {
            var movies = new List<Movie>();
            if (!File.Exists(MoviesFilePath)) return movies;

            try
            {
                var lines = File.ReadAllLines(MoviesFilePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = ParseCsvLine(line);
                    if (parts.Count >= 6)
                    {
                        if (int.TryParse(parts[0], out int id) &&
                            double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double minutes))
                        {
                            string title = parts[1];
                            string genre = parts[2];
                            string rating = parts[3];
                            string desc = parts[5];

                            movies.Add(new Movie(id, title, genre, rating, TimeSpan.FromMinutes(minutes), desc));
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on parse error
            }

            return movies;
        }

        public static void SaveShowings(List<Showing> showings)
        {
            using (var writer = new StreamWriter(ShowingsFilePath))
            {
                writer.WriteLine("ShowingId,MovieId,RoomId,StartTime,BookedSeats");
                foreach (var s in showings)
                {
                    string bookedSeats = SerializeBookedSeats(s.SeatGrid);
                    writer.WriteLine($"{s.ShowingId},{s.Movie.Id},{s.RoomId},\"{s.StartTime:hh:mm tt}\",\"{bookedSeats}\"");
                }
            }
        }

        public static List<Showing> LoadShowings(List<Movie> movies)
        {
            var showings = new List<Showing>();
            if (!File.Exists(ShowingsFilePath)) return showings;

            try
            {
                var lines = File.ReadAllLines(ShowingsFilePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = ParseCsvLine(line);
                    if (parts.Count >= 5)
                    {
                        if (int.TryParse(parts[0], out int showingId) &&
                            int.TryParse(parts[1], out int movieId) &&
                            int.TryParse(parts[2], out int roomId) &&
                            DateTime.TryParse(parts[3], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
                        {
                            DateTime fullStartTime = DateTime.Today.AddHours(parsedTime.Hour).AddMinutes(parsedTime.Minute);
                            string bookedSeats = parts[4];
                            var movie = movies.FirstOrDefault(m => m.Id == movieId);

                            if (movie != null)
                            {
                                var showing = new Showing(showingId, movie, roomId, fullStartTime);
                                DeserializeBookedSeats(bookedSeats, showing.SeatGrid);
                                showings.Add(showing);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Return empty on error
            }

            return showings;
        }

        public static void SaveTicket(int showingId, string movieTitle, int roomId, string seat, double price, DateTime purchaseTime)
        {
            bool writeHeader = !File.Exists(TicketsFilePath);

            using (var writer = new StreamWriter(TicketsFilePath, append: true))
            {
                if (writeHeader)
                {
                    writer.WriteLine("PurchaseTime,ShowingId,MovieTitle,RoomId,Seat,PricePHP");
                }
                writer.WriteLine($"{purchaseTime:yyyy-MM-dd hh:mm tt},{showingId},\"{EscapeCsv(movieTitle)}\",{roomId},\"{seat}\",PHP {price.ToString("F2", CultureInfo.InvariantCulture)}");
            }
        }

        private static string SerializeBookedSeats(bool[,] grid)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            var booked = new List<string>();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (grid[r, c])
                    {
                        char rowChar = (char)('A' + r);
                        booked.Add($"{rowChar}{c + 1}");
                    }
                }
            }

            return booked.Count > 0 ? string.Join("|", booked) : "NONE";
        }

        private static void DeserializeBookedSeats(string bookedSeatsStr, bool[,] grid)
        {
            if (string.IsNullOrWhiteSpace(bookedSeatsStr) || bookedSeatsStr == "NONE") return;

            var seats = bookedSeatsStr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var seat in seats)
            {
                string s = seat.Trim().ToUpper();
                if (s.Length >= 2)
                {
                    int r = s[0] - 'A';
                    if (int.TryParse(s.Substring(1), out int c))
                    {
                        int rowIdx = r;
                        int colIdx = c - 1;

                        if (rowIdx >= 0 && rowIdx < grid.GetLength(0) &&
                            colIdx >= 0 && colIdx < grid.GetLength(1))
                        {
                            grid[rowIdx, colIdx] = true;
                        }
                    }
                }
            }
        }

        private static string EscapeCsv(string str) => str.Replace("\"", "\"\"");

        private static List<string> ParseCsvLine(string line)
        {
            var results = new List<string>();
            bool inQuotes = false;
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    results.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            results.Add(current);
            return results;
        }
    }
}