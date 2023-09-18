using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace sampleapi
{
    [ApiController]
    public class MyController : Controller
    {
        readonly NpgsqlDataSource _dbsource;

        public MyController(NpgsqlDataSource dbsource)
        {
            _dbsource = dbsource;
        }

        [HttpGet("someget")]
        [Produces("application/json")]
        public async Task<IActionResult> SomeGet()
        {
            var ret = new List<SimpleMessage>();
            await using var db = await _dbsource.OpenConnectionAsync();
            await using var cmd = db.CreateCommand();
            cmd.CommandText = "select id,username,message,created_at from messages";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ret.Add(new SimpleMessage
                {
                    Id = reader.GetInt64(0),
                    Username = reader.GetString(1),
                    Message = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3)
                });
            }
            return Ok(ret);
        }

        [HttpPost("somepost")]
        [Produces("application/json")]
        public async Task<IActionResult> SomePost([FromQuery][Required] string username, [FromBody][Required] string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return BadRequest("Msg is empty");
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is empty");

            await using var db = await _dbsource.OpenConnectionAsync();
            await using var cmd = db.CreateCommand();
            cmd.CommandText = "insert into messages (created_at,message,username) values (:ts,:msg,:user) returning id";
            cmd.Parameters.AddWithValue("ts", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("msg", msg);
            cmd.Parameters.AddWithValue("user", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return StatusCode(500);

            return Ok(reader.GetInt64(0));
        }
    }
}
