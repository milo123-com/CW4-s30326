using System;
using System.Collections.Generic;

namespace ConsoleApp2.Models

{
    public class TripDto
    {
        public int              Id          { get; set; }
        public string           Name        { get; set; }
        public string           Description { get; set; }
        public DateTime         StartDate   { get; set; }
        public DateTime         EndDate     { get; set; }
        public int              MaxPeople   { get; set; }
        public List<CountryDto> Countries   { get; set; } = new();
    }
}