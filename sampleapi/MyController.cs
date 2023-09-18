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
            using var cmd = db.CreateCommand();
            cmd.CommandText = "select ts,msg from messages";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ret.Add(new SimpleMessage
                {
                    Message = reader.GetString(1),
                    Timestamp = reader.GetDateTime(0)
                });
            }
            return new OkObjectResult(ret);
        }

        [HttpPost("somepost")]
        [Produces("application/json")]
        public async Task<IActionResult> SomePost([FromBody][Required] string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return BadRequest("Msg is empty");

            await using var db = await _dbsource.OpenConnectionAsync();
            using var cmd = db.CreateCommand();
            cmd.CommandText = "insert into messages (ts,msg) values (:ts,:msg)";
            cmd.Parameters.AddWithValue("ts", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("msg", msg);
            await cmd.ExecuteNonQueryAsync();
            return Ok();
        }
    }
}
