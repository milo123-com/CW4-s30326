

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ConsoleApp2.Models;
namespace ConsoleApp2.Controllers

{
    [ApiController]
    [Route("api/clients/{clientId}/trips")]
    public class ClientTripsController : ControllerBase
    {
        private readonly string _connString;
        public ClientTripsController(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// GET /api/clients/{clientId}/trips
        /// Pobiera wycieczki klienta.
        /// </summary>
        [HttpGet]
        public IActionResult GetClientTrips(int clientId)
        {
            const string checkClient = "SELECT COUNT(1) FROM Client WHERE Id = @cid";
            const string sql = @"
                SELECT ct.IdTrip, t.Name AS TripName, ct.RegisteredAt,
       t.Description, t.DateFrom AS StartDate, t.DateTo AS EndDate, t.MaxPeople,
       c.IdCountry, c.Name AS CountryName
FROM Client_Trip ct
JOIN Trip t ON ct.IdTrip = t.IdTrip
LEFT JOIN Country_Trip ctp ON t.IdTrip = ctp.IdTrip
LEFT JOIN Country c ON ctp.IdCountry = c.IdCountry
WHERE ct.IdClient = @cid
";

            try
            {
                using var conn = new SqlConnection(_connString);
                conn.Open();

                // Sprawdź istnienie klienta
                using (var chk = new SqlCommand(checkClient, conn))
                {
                    chk.Parameters.AddWithValue("@cid", clientId);
                    if ((int)chk.ExecuteScalar() == 0)
                        return NotFound($"Client with ID {clientId} not found.");
                }

                var trips = new Dictionary<int, TripDto>();
                var regs  = new List<ClientTripDto>();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@cid", clientId);
                    using var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        int tid = rdr.GetInt32(rdr.GetOrdinal("TripId"));
                        if (!trips.TryGetValue(tid, out var trip))
                        {
                            trip = new TripDto
                            {
                                Id          = tid,
                                Name        = rdr.GetString(rdr.GetOrdinal("TripName")),
                                Description = rdr.GetString(rdr.GetOrdinal("Description")),
                                StartDate   = rdr.GetDateTime(rdr.GetOrdinal("StartDate")),
                                EndDate     = rdr.GetDateTime(rdr.GetOrdinal("EndDate")),
                                MaxPeople   = rdr.GetInt32(rdr.GetOrdinal("MaxPeople"))
                            };
                            trips[tid] = trip;
                            regs.Add(new ClientTripDto
                            {
                                TripId       = tid,
                                TripName     = trip.Name,
                                RegisteredAt = rdr.GetDateTime(rdr.GetOrdinal("RegisteredAt")),
                                TripDetails  = trip
                            });
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
                }

                return Ok(regs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// PUT /api/clients/{clientId}/trips/{tripId}
        /// Rejestruje klienta na wycieczkę.
        /// </summary>
        [HttpPut("{tripId}")]
        public IActionResult RegisterClient(int clientId, int tripId)
        {
            // Sprawdzenie klienta:
            const string checkClient = "SELECT COUNT(1) FROM Client WHERE IdClient = @cid";
            //Sprawdzenie wycieczki:
            const string checkTrip   = "SELECT MaxPeople FROM Trip WHERE IdTrip = @tid";
            //Policz zarejestrowanych:
            const string countRegs   = "SELECT COUNT(1) FROM Client_Trip WHERE IdTrip = @tid";
            //Rejestracja:
            const string insertReg   = @"
                INSERT INTO Client_Trip (INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
VALUES (@cid, @tid, GETDATE())";

            try
            {
                using var conn = new SqlConnection(_connString);
                conn.Open();

                // Weryfikacja klienta i wycieczki
                using (var chk = new SqlCommand(checkClient, conn))
                {
                    chk.Parameters.AddWithValue("@cid", clientId);
                    if ((int)chk.ExecuteScalar() == 0)
                        return NotFound($"Client {clientId} not found.");
                }
                int max, taken;
                using (var chk = new SqlCommand(checkTrip, conn))
                {
                    chk.Parameters.AddWithValue("@tid", tripId);
                    var obj = chk.ExecuteScalar();
                    if (obj == null) return NotFound($"Trip {tripId} not found.");
                    max = (int)obj;
                }
                using (var cnt = new SqlCommand(countRegs, conn))
                {
                    cnt.Parameters.AddWithValue("@tid", tripId);
                    taken = (int)cnt.ExecuteScalar();
                }
                if (taken >= max)
                    return BadRequest("Trip capacity reached.");

                // Dodanie rejestracji
                using (var ins = new SqlCommand(insertReg, conn))
                {
                    ins.Parameters.AddWithValue("@cid", clientId);
                    ins.Parameters.AddWithValue("@tid", tripId);
                    ins.ExecuteNonQuery();
                }

                return Ok("Client registered successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// DELETE /api/clients/{clientId}/trips/{tripId}
        /// Usuwa rejestrację klienta.
        /// </summary>
        [HttpDelete("{tripId}")]
        public IActionResult UnregisterClient(int clientId, int tripId)
        {
            const string deleteReg = "DELETE FROM Client_Trip WHERE IdClient = @cid AND IdTrip = @tid";
            try
            {
                using var conn = new SqlConnection(_connString);
                using var cmd  = new SqlCommand(deleteReg, conn);
                cmd.Parameters.AddWithValue("@cid", clientId);
                cmd.Parameters.AddWithValue("@tid", tripId);
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                    return NotFound("Registration not found.");
                return Ok("Client unregistered successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
