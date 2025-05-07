using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using ConsoleApp2.Models;
namespace ConsoleApp2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly string _connString;
        public ClientsController(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// POST /api/clients
        /// Tworzy nowego klienta.
        /// </summary>
        [HttpPost]
        public IActionResult CreateClient([FromBody] ClientCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const string sql = @"
                INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
OUTPUT INSERTED.IdClient
VALUES (@fn, @ln, @em, @tel, @ps)
";

            try
            {
                using var conn = new SqlConnection(_connString);
                using var cmd  = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@fn", dto.FirstName);
                cmd.Parameters.AddWithValue("@ln", dto.LastName);
                cmd.Parameters.AddWithValue("@em", dto.Email);
                cmd.Parameters.AddWithValue("@tel", (object)dto.Telephone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ps", dto.Pesel);

                conn.Open();
                int newId = (int)cmd.ExecuteScalar();
                return CreatedAtAction(
                    nameof(ClientTripsController.GetClientTrips),
                    "ClientTrips",
                    new { clientId = newId },
                    new { Id = newId }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}