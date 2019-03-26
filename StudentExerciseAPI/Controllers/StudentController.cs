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
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
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
                    cmd.CommandText = @"SELECT s.Id as StudentId,
                                               s.StudentFirstName,
                                               s.StudentLastname,
                                               s.StudentSlackHandle,
                                               s.CohortId,
                                               c.[CohortName] as CohortName,
                                               e.id as ExerciseId,
                                               e.[Exercisename] as ExerciseName,
                                               e.[Language]
                                        FROM student s
                                                left join Cohort c on s.CohortId = c.id
                                                left join ExerciseIntersection ie on s.id = ei.studentid
                                                left join Exercise e on ei.cohortid = e.id";                    

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {
                        Student newStudent = new Student()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            StudentFirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                            StudentLastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                            StudentSlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                            CohortName = reader.GetString(reader.GetOrdinal("CohortName")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),                            
                            Cohort = new Cohort                            
                            {
                              Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                              CohortName = reader.GetString(reader.GetOrdinal("CohortName"))
                            }
                        };

                        students.Add(newStudent);
                    }
                    reader.Close();

                    return Ok(students);
                }
            }
        }

// CODE FOR GETTING A SINGLE ITEM

        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id,
                                               StudentFirstName,
                                               StudentLastname,
                                               StudentSlackHandle,
                                               CohortId
                                        FROM Student
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student Student = null;

                    if (reader.Read())
                    {
                        Student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            StudentFirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                            StudentLastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                            StudentSlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };
                    }
                    reader.Close();

                    return Ok(Student);
                }
            }
        }

// CODE FOR ADDING/INSERTING AN ITEM

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Student (
                                               StudentFirstName,
                                               StudentLastname,
                                               StudentSlackHandle,
                                               CohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName,
                                                @lastName,
                                                @slackHandle,
                                                @cohortId)";

                    cmd.Parameters.Add(new SqlParameter("@firstName", student.StudentFirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", student.StudentLastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", student.StudentSlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", student.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    student.Id = newId;
                    // 
                    // Re-route user back to student they created
                    return CreatedAtRoute("GetStudent", new { id = newId }, student);
                }
            }
        }

// CODE FOR EDITING/UPDATING AN ITEM

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Student
                                            SET StudentFirstName = @firstName,
                                                StudentLastName = @lastName,
                                                StudentSlackHandle = @slackHandle,
                                                CohortId = @cohortId
                                            WHERE Id = @id";
                        
                        cmd.Parameters.Add(new SqlParameter("@firstName", student.StudentFirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", student.StudentLastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", student.StudentSlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortid", student.CohortId));
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
                if (!StudentExists(id))
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
                        cmd.CommandText = @"DELETE FROM Student WHERE Id = @id";
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
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool StudentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, StudentFirstName, StudentLastName, StudentSlackHandle, CohortId
                                        FROM Student
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
