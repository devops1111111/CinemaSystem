using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CinemaBookingSystem
{
    public class CinemaSystem
    {
        private readonly List<Movie> _movies;
        private readonly Dictionary<int, CinemaRoom> _rooms;
        private int _nextShowingId;

        public List<Movie> Movies => _movies;
        public Dictionary<int, CinemaRoom> Rooms => _rooms;

        public CinemaSystem()
        {
            _movies = new List<Movie>();
            _rooms = new Dictionary<int, CinemaRoom>();
            _nextShowingId = 1;

            InitializeRooms();
            LoadDataOrCreateDefault();
        }

        private void InitializeRooms()
        {
            _rooms.Add(1, new CinemaRoom(1, isVipRoom: true));
            _rooms.Add(2, new CinemaRoom(2, isVipRoom: true));
            _rooms.Add(3, new CinemaRoom(3, isVipRoom: false));
            _rooms.Add(4, new CinemaRoom(4, isVipRoom: false));
            _rooms.Add(5, new CinemaRoom(5, isVipRoom: false));
        }

        private void LoadDataOrCreateDefault()
        {
            List<Movie> loadedMovies = CsvStorage.LoadMovies();
            foreach (var movie in loadedMovies)
            {
                _movies.Add(movie);
            }

            if (!_movies.Any())
            {
                _movies.Add(new Movie(1, "Inception", "Sci-Fi", "PG-13", TimeSpan.FromHours(2), "A mind-bending thriller about dream invasion."));
                _movies.Add(new Movie(2, "The Dark Knight", "Action", "PG-13", TimeSpan.FromHours(2), "The dark knight faces the Joker in Gotham City."));
                _movies.Add(new Movie(3, "Interstellar", "Sci-Fi", "PG-13", TimeSpan.FromHours(2), "A space exploration journey through a wormhole."));
                _movies.Add(new Movie(4, "Avatar", "Sci-Fi", "PG-13", TimeSpan.FromHours(2), "An epic journey on the alien world of Pandora."));
                _movies.Add(new Movie(5, "Titanic", "Romance", "PG-13", TimeSpan.FromHours(2), "A tragic romance aboard the ill-fated ship."));

                CsvStorage.SaveMovies(_movies);
            }

            List<Showing> loadedShowings = CsvStorage.LoadShowings(_movies);

            if (!loadedShowings.Any())
            {
                DateTime today = DateTime.Today;

                // Room 3
                AddDefaultShowing(_movies[0], 3, today.AddHours(11));
                AddDefaultShowing(_movies[3], 3, today.AddHours(13).AddMinutes(20));
                AddDefaultShowing(_movies[0], 3, today.AddHours(15).AddMinutes(40));
                AddDefaultShowing(_movies[3], 3, today.AddHours(18));
                AddDefaultShowing(_movies[0], 3, today.AddHours(20).AddMinutes(20));

                // Room 4
                AddDefaultShowing(_movies[1], 4, today.AddHours(11).AddMinutes(20));
                AddDefaultShowing(_movies[4], 4, today.AddHours(13).AddMinutes(40));
                AddDefaultShowing(_movies[1], 4, today.AddHours(16));
                AddDefaultShowing(_movies[4], 4, today.AddHours(18).AddMinutes(20));
                AddDefaultShowing(_movies[1], 4, today.AddHours(20).AddMinutes(40));

                // Room 5
                AddDefaultShowing(_movies[2], 5, today.AddHours(11).AddMinutes(40));
                AddDefaultShowing(_movies[2], 5, today.AddHours(14));
                AddDefaultShowing(_movies[2], 5, today.AddHours(16).AddMinutes(20));
                AddDefaultShowing(_movies[2], 5, today.AddHours(18).AddMinutes(40));
                AddDefaultShowing(_movies[2], 5, today.AddHours(21));

                SaveAllShowings();
            }
            else
            {
                foreach (var s in loadedShowings)
                {
                    _rooms[s.RoomId].Showings.Add(s);
                }
                _nextShowingId = loadedShowings.Max(s => s.ShowingId) + 1;
            }
        }

        private void AddDefaultShowing(Movie movie, int roomId, DateTime startTime)
        {
            var showing = new Showing(_nextShowingId++, movie, roomId, startTime);
            _rooms[roomId].Showings.Add(showing);
        }

        private void SaveAllShowings()
        {
            var allShowings = _rooms.Values.SelectMany(r => r.Showings).ToList();
            CsvStorage.SaveShowings(allShowings);
        }

        public void DisplayMoviesAndShowings()
        {
            Console.Clear();
            Console.WriteLine("====================================================================================");
            Console.WriteLine("                                MOVIES & SHOWINGS LIST                              ");
            Console.WriteLine("====================================================================================\n");

            foreach (var movie in _movies)
            {
                Console.WriteLine($"[Movie #{movie.Id}] {movie.Title} | Genre: {movie.Genre} | Rating: {movie.Rating} | Duration: {movie.Duration.TotalMinutes} mins");
                Console.WriteLine($"Description: {movie.Description}");

                var allShowingsForMovie = _rooms.Values
                    .SelectMany(r => r.Showings)
                    .Where(s => s.Movie.Id == movie.Id)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                if (allShowingsForMovie.Any())
                {
                    var firstShowing = allShowingsForMovie.First();
                    var lastShowing = allShowingsForMovie.Last();
                    Console.WriteLine($"  -> First Showing Today: Room {firstShowing.RoomId} at {firstShowing.StartTime:hh:mm tt}");
                    Console.WriteLine($"  -> Last Showing Today : Room {lastShowing.RoomId} at {lastShowing.StartTime:hh:mm tt}");
                    Console.WriteLine("  All Scheduled Showtimes:");

                    foreach (var s in allShowingsForMovie)
                    {
                        Console.WriteLine($"     - Room {s.RoomId} | {s.StartTime:hh:mm tt} to {s.EndTime:hh:mm tt}");
                    }
                }
                else
                {
                    Console.WriteLine("  -> No scheduled showtimes today.");
                }
                Console.WriteLine(new string('-', 84));
            }
        }

        public void ScheduleVipMovie()
        {
            Console.Clear();
            Console.WriteLine("=================================================");
            Console.WriteLine("          SCHEDULE A MOVIE (VIP ROOM)            ");
            Console.WriteLine("=================================================\n");

            var predefinedSlots = new List<(TimeSpan Start, TimeSpan End, string DisplayText)>
            {
                (new TimeSpan(11, 0, 0), new TimeSpan(13, 0, 0), "11:00 AM - 01:00 PM"),
                (new TimeSpan(13, 20, 0), new TimeSpan(15, 20, 0), "01:20 PM - 03:20 PM"),
                (new TimeSpan(15, 40, 0), new TimeSpan(17, 40, 0), "03:40 PM - 05:40 PM"),
                (new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0), "06:00 PM - 08:00 PM")
            };

            Console.WriteLine("--- Room Availability Status (VIP Only) ---");
            foreach (int roomId in new[] { 1, 2 })
            {
                var room = _rooms[roomId];
                bool hasBookings = room.Showings.Any();

                string statusHeader = hasBookings ? $"[ HAS BOOKINGS ({room.Showings.Count}) ]" : "[ AVAILABLE - Entire Day ]";
                Console.WriteLine($"Room {roomId} (VIP): {statusHeader}");

                foreach (var slot in predefinedSlots)
                {
                    DateTime slotStart = DateTime.Today.Add(slot.Start);
                    DateTime slotEnd = DateTime.Today.Add(slot.End);

                    var conflictingShowing = room.Showings.FirstOrDefault(s =>
                    {
                        DateTime bufferedStart = s.StartTime.Subtract(TimeSpan.FromMinutes(20));
                        DateTime bufferedEnd = s.EndTime.Add(TimeSpan.FromMinutes(20));
                        return slotStart < bufferedEnd && slotEnd > bufferedStart;
                    });

                    if (conflictingShowing != null)
                    {
                        Console.WriteLine($"  - {slot.DisplayText} -> [ BOOKED - {conflictingShowing.Movie.Title} ]");
                    }
                    else
                    {
                        Console.WriteLine($"  - {slot.DisplayText} -> [ AVAILABLE ]");
                    }
                }
                Console.WriteLine();
            }

            Console.Write("Select VIP Room (1 or 2): ");
            if (!int.TryParse(Console.ReadLine(), out int selectedRoom) || (selectedRoom != 1 && selectedRoom != 2))
            {
                Console.WriteLine("Invalid room selection. Scheduling cancelled.");
                return;
            }

            Console.WriteLine("\nAvailable Movies:");
            foreach (var m in _movies)
            {
                Console.WriteLine($"[{m.Id}] {m.Title} ({m.Duration.TotalMinutes} mins)");
            }

            Console.Write("Select Movie ID: ");
            if (!int.TryParse(Console.ReadLine(), out int movieId) || !_movies.Any(m => m.Id == movieId))
            {
                Console.WriteLine("Invalid Movie ID.");
                return;
            }

            Movie selectedMovie = _movies.First(m => m.Id == movieId);

            Console.Write("Enter Start Time (e.g., 01:20 PM, 11:00 AM): ");
            string timeInput = Console.ReadLine()?.Trim();

            string[] formats = { "h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt" };
            if (!DateTime.TryParseExact(timeInput, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
            {
                Console.WriteLine("\n[ERROR] Invalid time format! You must explicitly specify AM or PM (e.g., 01:20 PM or 11:00 AM).");
                return;
            }

            DateTime fullStartTime = DateTime.Today.AddHours(parsedTime.Hour).AddMinutes(parsedTime.Minute);
            DateTime fullEndTime = fullStartTime.Add(selectedMovie.Duration);

            TimeSpan openTime = new TimeSpan(10, 0, 0);
            TimeSpan closeTime = new TimeSpan(23, 0, 0);

            if (fullStartTime.TimeOfDay < openTime || fullEndTime.TimeOfDay > closeTime || fullEndTime.Date > fullStartTime.Date)
            {
                Console.WriteLine($"\n[ERROR] Cinema operating hours are 10:00 AM - 11:00 PM.");
                Console.WriteLine($"Proposed showing ({fullStartTime:hh:mm tt} - {fullEndTime:hh:mm tt}) falls outside operating hours.");
                return;
            }

            if (_rooms[selectedRoom].CanSchedule(fullStartTime, selectedMovie.Duration, out string conflictReason))
            {
                var newShowing = new Showing(_nextShowingId++, selectedMovie, selectedRoom, fullStartTime);
                _rooms[selectedRoom].Showings.Add(newShowing);
                SaveAllShowings();

                double vipRentalPrice = 5000.00;
                DateTime purchaseTime = DateTime.Now;

                CsvStorage.SaveTicket(newShowing.ShowingId, selectedMovie.Title, selectedRoom, "FULL VIP ROOM RENTAL", vipRentalPrice, purchaseTime);

                PrintVipReceipt(newShowing, vipRentalPrice, purchaseTime);
            }
            else
            {
                Console.WriteLine($"\n[CONFLICT ERROR] Cannot schedule movie: {conflictReason}");
            }
        }

        public void BuyTicket()
        {
            Console.Clear();
            Console.WriteLine("=================================================");
            Console.WriteLine("                  BUY A TICKET                   ");
            Console.WriteLine("=================================================\n");

            Console.WriteLine("Available Movies:");
            foreach (var movie in _movies)
            {
                Console.WriteLine($"[{movie.Id}] {movie.Title} ({movie.Genre}) - {movie.Duration.TotalMinutes} mins");
            }

            Console.Write("\nSelect Movie ID: ");
            if (!int.TryParse(Console.ReadLine(), out int selectedMovieId) || !_movies.Any(m => m.Id == selectedMovieId))
            {
                Console.WriteLine("Invalid Movie ID.");
                return;
            }

            Movie chosenMovie = _movies.First(m => m.Id == selectedMovieId);

            var availableShowings = _rooms.Values
                .SelectMany(r => r.Showings)
                .Where(s => s.Movie.Id == chosenMovie.Id)
                .OrderBy(s => s.StartTime)
                .ToList();

            if (!availableShowings.Any())
            {
                Console.WriteLine($"\nNo available showtimes scheduled for '{chosenMovie.Title}'.");
                return;
            }

            Console.Clear();
            Console.WriteLine("=================================================");
            Console.WriteLine($"  AVAILABLE SHOWTIMES FOR: {chosenMovie.Title.ToUpper()}");
            Console.WriteLine("=================================================\n");

            for (int i = 0; i < availableShowings.Count; i++)
            {
                var showing = availableShowings[i];
                Console.WriteLine($"[{i + 1}] Room {showing.RoomId} | {showing.StartTime:hh:mm tt} - {showing.EndTime:hh:mm tt}");
            }

            Console.Write("\nSelect Time / Showing Number: ");
            if (!int.TryParse(Console.ReadLine(), out int showingChoice) || showingChoice < 1 || showingChoice > availableShowings.Count)
            {
                Console.WriteLine("Invalid showing selection.");
                return;
            }

            Showing selectedShowing = availableShowings[showingChoice - 1];

            selectedShowing.DisplaySeatMap();

            Console.Write("\nEnter Row Letter (e.g., A): ");
            string rowInput = Console.ReadLine()?.Trim().ToUpper();
            Console.Write("Enter Column Number (1 to 6): ");
            string colInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(rowInput) || !int.TryParse(colInput, out int col) || col < 1 || col > 6)
            {
                Console.WriteLine("Invalid seat input.");
                return;
            }

            int row = rowInput[0] - 'A';
            int colIdx = col - 1;

            if (row < 0 || row >= selectedShowing.SeatGrid.GetLength(0))
            {
                Console.WriteLine("Invalid row choice.");
                return;
            }

            if (selectedShowing.SeatGrid[row, colIdx])
            {
                Console.WriteLine("\n[ERROR] Seat is already occupied (X). Please select an available seat.");
                return;
            }

            selectedShowing.SeatGrid[row, colIdx] = true;
            SaveAllShowings();

            string seatName = $"{rowInput[0]}{col}";
            double ticketPrice = 250.00;
            DateTime purchaseTime = DateTime.Now;

            CsvStorage.SaveTicket(selectedShowing.ShowingId, selectedShowing.Movie.Title, selectedShowing.RoomId, seatName, ticketPrice, purchaseTime);

            PrintTicketReceipt(selectedShowing, rowInput[0], col, ticketPrice, purchaseTime);
        }

        private void PrintTicketReceipt(Showing showing, char row, int col, double price, DateTime purchaseTime)
        {
            Console.Clear();
            Console.WriteLine("=================================================");
            Console.WriteLine("            POS TICKET & RECEIPT (SINGLE)        ");
            Console.WriteLine("=================================================");
            Console.WriteLine($" Date        : {purchaseTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($" Movie       : {showing.Movie.Title}");
            Console.WriteLine($" Genre/Rating: {showing.Movie.Genre} ({showing.Movie.Rating})");
            Console.WriteLine($" Room        : Room {showing.RoomId}");
            Console.WriteLine($" Time        : {showing.StartTime:hh:mm tt} - {showing.EndTime:hh:mm tt}");
            Console.WriteLine($" Seat        : Row {row}, Seat {col}");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($" Price       : PHP {price:N2}");
            Console.WriteLine(" Status      : CONFIRMED / PAID");
            Console.WriteLine("=================================================");
            Console.WriteLine(" Thank you for choosing Cinema Center!");
            Console.WriteLine("=================================================");
        }

        private void PrintVipReceipt(Showing showing, double price, DateTime purchaseTime)
        {
            Console.Clear();
            Console.WriteLine("=================================================");
            Console.WriteLine("         POS RECEIPT - VIP ROOM SCHEDULING      ");
            Console.WriteLine("=================================================");
            Console.WriteLine($" Date        : {purchaseTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($" Movie Scheduled: {showing.Movie.Title}");
            Console.WriteLine($" Genre/Rating   : {showing.Movie.Genre} ({showing.Movie.Rating})");
            Console.WriteLine($" Reserved Room  : Room {showing.RoomId} (VIP EXECUTIVE SUITE)");
            Console.WriteLine($" Scheduled Time : {showing.StartTime:hh:mm tt} - {showing.EndTime:hh:mm tt}");
            Console.WriteLine($" Includes       : Full Private Theater Access");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($" Flat Rate      : PHP {price:N2}");
            Console.WriteLine(" Status         : RESERVED & CONFIRMED / PAID");
            Console.WriteLine("=================================================");
            Console.WriteLine(" Thank you for choosing Cinema VIP Experience!");
            Console.WriteLine("=================================================");
        }
    }
}