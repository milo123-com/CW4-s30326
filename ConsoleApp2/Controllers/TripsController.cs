

using Microsoft.AspNetCore.Mvc;

using System.Data.SqlClient;
using ConsoleApp2.Models;
namespace ConsoleApp2.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly string _connString;
        public TripsController(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// GET /api/trips
        /// Pobiera wszystkie wycieczki wraz z krajami.
        /// </summary>
        [HttpGet]
        public IActionResult GetAllTrips()
        {
            var trips = new Dictionary<int, TripDto>();
            const string sql = @"
                SELECT t.IdTrip, t.Name, t.Description, t.DateFrom AS StartDate, t.DateTo AS EndDate, t.MaxPeople,
       c.IdCountry, c.Name AS CountryName
FROM Trip t
LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
LEFT JOIN Country c ON ct.IdCountry = c.IdCountry

    ";

            try
            {
                using var conn = new SqlConnection(_connString);
                using var cmd  = new SqlCommand(sql, conn);
                conn.Open();
                using var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    int id = rdr.GetInt32(rdr.GetOrdinal("Id"));
                    if (!trips.TryGetValue(id, out var trip))
                    {
                        trip = new TripDto
                        {
                            Id          = id,
                            Name        = rdr.GetString(rdr.GetOrdinal("Name")),
                            Description = rdr.GetString(rdr.GetOrdinal("Description")),
                            StartDate   = rdr.GetDateTime(rdr.GetOrdinal("StartDate")),
                            EndDate     = rdr.GetDateTime(rdr.GetOrdinal("EndDate")),
                            MaxPeople   = rdr.GetInt32(rdr.GetOrdinal("MaxPeople"))
                        };
                        trips[id] = trip;
                    }
                    if (!rdr.IsDBNull(rdr.GetOrdinal("CountryId")))
                    {
                        trip.Countries.Add(new CountryDto
                        {
                            Id   = rdr.GetInt32(rdr.GetOrdinal("CountryId")),
                            Name = rdr.GetString(rdr.GetOrdinal("CountryName"))
                        });
                    }
                }

                return Ok(trips.Values);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
