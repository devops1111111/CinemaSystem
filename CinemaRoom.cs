using System;
using System.Collections.Generic;

namespace CinemaBookingSystem
{
    public class CinemaRoom
    {
        private readonly int _roomId;
        private readonly bool _isVipRoom;
        private readonly List<Showing> _showings;

        public int RoomId => _roomId;
        public bool IsVipRoom => _isVipRoom;
        public List<Showing> Showings => _showings;

        public CinemaRoom(int roomId, bool isVipRoom)
        {
            _roomId = roomId;
            _isVipRoom = isVipRoom;
            _showings = new List<Showing>();
        }

        public bool CanSchedule(DateTime requestedStart, TimeSpan movieDuration, out string conflictReason)
        {
            DateTime requestedEnd = requestedStart.Add(movieDuration);
            TimeSpan buffer = TimeSpan.FromMinutes(20);

            foreach (var showing in _showings)
            {
                DateTime bufferedStart = showing.StartTime.Subtract(buffer);
                DateTime bufferedEnd = showing.EndTime.Add(buffer);

                if (requestedStart < bufferedEnd && requestedEnd > bufferedStart)
                {
                    conflictReason = $"Time conflict or missing mandatory 20-min cleaning gap. Existing showing: {showing.StartTime:hh:mm tt} - {showing.EndTime:hh:mm tt} ({showing.Movie.Title})";
                    return false;
                }
            }

            conflictReason = string.Empty;
            return true;
        }
    }
}