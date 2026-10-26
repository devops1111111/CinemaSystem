using System;

namespace CinemaBookingSystem
{
    public class Movie
    {
        private readonly int _id;
        private readonly string _title;
        private readonly string _genre;
        private readonly string _rating;
        private readonly TimeSpan _duration;
        private readonly string _description;

        public int Id => _id;
        public string Title => _title;
        public string Genre => _genre;
        public string Rating => _rating;
        public TimeSpan Duration => _duration;
        public string Description => _description;

        public Movie(int id, string title, string genre, string rating, TimeSpan duration, string description)
        {
            _id = id;
            _title = title;
            _genre = genre;
            _rating = rating;
            _duration = duration;
            _description = description;
        }
    }

    public class Showing
    {
        private readonly int _showingId;
        private readonly Movie _movie;
        private readonly int _roomId;
        private readonly DateTime _startTime;
        private readonly bool[,] _seatGrid;

        public int ShowingId => _showingId;
        public Movie Movie => _movie;
        public int RoomId => _roomId;
        public DateTime StartTime => _startTime;
        public DateTime EndTime => _startTime.Add(_movie.Duration);
        public bool[,] SeatGrid => _seatGrid;

        public Showing(int showingId, Movie movie, int roomId, DateTime startTime, int rows = 5, int cols = 6)
        {
            _showingId = showingId;
            _movie = movie;
            _roomId = roomId;
            _startTime = startTime;
            _seatGrid = new bool[rows, cols];
        }

        public void DisplaySeatMap()
        {
            Console.WriteLine("\n=================================");
            Console.WriteLine("          [ CINEMA SCREEN ]      ");
            Console.WriteLine("=================================\n");

            int rows = _seatGrid.GetLength(0);
            int cols = _seatGrid.GetLength(1);

            Console.Write("     ");
            for (int c = 1; c <= cols; c++)
            {
                Console.Write($"Col {c}  ");
            }
            Console.WriteLine("\n");

            for (int r = 0; r < rows; r++)
            {
                char rowLabel = (char)('A' + r);
                Console.Write($"Row {rowLabel} ");
                for (int c = 0; c < cols; c++)
                {
                    string status = _seatGrid[r, c] ? "[ X ]" : "[ O ]";
                    Console.Write($"{status}  ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("\nLegend: [ O ] Available  |  [ X ] Reserved");
        }
    }
}