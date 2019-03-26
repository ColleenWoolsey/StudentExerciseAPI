using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using StudentExerciseAPI.Models;
using Microsoft.AspNetCore.Http;
using StudentExercisesAPI.Models;

namespace StudentExerciseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            // First have to add connection in appsettings.json
            // "AllowedHosts": "*",
            // "ConnectionStrings": {
            // "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=StudentExercisesDB;Trusted_Connection=True;"
            // }

            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // CODE FOR GETTING A LIST

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())

                {
                    cmd.CommandText = @"SELECT i.Id as InstructorId,
                                               i.InstructorFirstName,
                                               i.InstructorLastname,
                                               i.InstructorSlackHandle,                                               
                                               i.CohortId,
                                               c.[CohortName] as CohortName                                               
                                               
                                          FROM Instructor i
                                          LEFT JOIN Cohort c ON i.CohortId = c.id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor>instructors = new List<Instructor>();

                    while (reader.Read())
                    {
                        Instructor newInstructor = new Instructor()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                            InstructorFirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                            InstructorLastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                            InstructorSlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                            CohortName = reader.GetString(reader.GetOrdinal("CohortName")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                CohortName = reader.GetString(reader.GetOrdinal("CohortName"))
                            }
                        };

                        instructors.Add(newInstructor);
                    }
                    reader.Close();

                    return Ok(instructors);
                }
            }
        }

        // CODE FOR GETTING A SINGLE ITEM

        [HttpGet("{id}", Name = "GetInstructor")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id,
                                               InstructorFirstName,
                                               InstructorLastname,
                                               InstructorSlackHandle,
                                               CohortId
                                        FROM Instructor
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor Instructor = null;

                    if (reader.Read())
                    {
                        Instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            InstructorFirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                            InstructorLastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                            InstructorSlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };
                    }
                    reader.Close();

                    return Ok(Instructor);
                }
            }
        }

        // CODE FOR ADDING/INSERTING AN ITEM

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Instructor (
                                               InstructorFirstName,
                                               InstructorLastname,
                                               InstructorSlackHandle,
                                               CohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName,
                                                @lastName,
                                                @slackHandle,
                                                @cohortId)";

                    cmd.Parameters.Add(new SqlParameter("@firstName", instructor.InstructorFirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", instructor.InstructorLastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", instructor.InstructorSlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", instructor.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    instructor.Id = newId;
                    // 
                    // Re-route user back to Instructor they created
                    return CreatedAtRoute("GetInstructor", new { id = newId }, instructor);
                }
            }
        }

        // CODE FOR EDITING/UPDATING AN ITEM

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Instructor instructor)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Instructor
                                            SET InstructorFirstName = @firstName,
                                                InstructorLastName = @lastName,
                                                InstructorSlackHandle = @slackHandle,
                                                CohortId = @cohortId
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@firstName", instructor.InstructorFirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", instructor.InstructorLastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", instructor.InstructorSlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortid", instructor.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!InstructorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // CODE FOR DELETING AN ITEM

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Instructor WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!InstructorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool InstructorExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, InstructorFirstName, InstructorLastName, InstructorSlackHandle, CohortId
                                        FROM Instructor
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
