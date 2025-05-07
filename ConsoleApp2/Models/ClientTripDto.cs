using System;
namespace ConsoleApp2.Models
{
    public class ClientTripDto
    {
        public int      TripId       { get; set; }
        public string   TripName     { get; set; }
        public DateTime RegisteredAt { get; set; }
        public TripDto  TripDetails  { get; set; }
    }
}