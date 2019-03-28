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
    public class CohortController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            // First have to add connection in appsettings.json
            // "AllowedHosts": "*",
            // "ConnectionStrings": {
            // "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=StudentCohortsDB;Trusted_Connection=True;"
            // }

            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // CODE FOR GETTING A LIST OF COHORTS
        // GET: api/Cohorts?include=student||instructor&q=Day
        [HttpGet]
        public IActionResult Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // if (include == "student" || "instructor") { }

                    cmd.CommandText = @"SELECT c.Id AS CohortId,
		                                     c.CohortName,
		                                     s.Id as StudentId, 
		                                     s.StudentFirstName,
		                                     s.StudentLastName,
		                                     s.StudentSlackHandle,
                                             i.Id AS InstructorId,
		                                     i.InstructorFirstName,
		                                     i.InstructorLastname,
                                             i.InstructorSlackHandle		                                    
                                        FROM Cohort c
                                        INNER JOIN Student s on c.Id = s.CohortId
                                        INNER JOIN Instructor i on c.Id = i.CohortId";

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Cohort> dictCohorts = new Dictionary<int, Cohort>();

                    while (reader.Read())
                    {
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!dictCohorts.ContainsKey(cohortId))
                        {
                            Cohort newCohort = new Cohort
                            {
                                Id = cohortId,
                                CohortName = reader.GetString(reader.GetOrdinal("CohortName")),
                                ListofStudents = new List<Student>(),
                                ListofInstructors = new List<Instructor>()
                            };

                            dictCohorts.Add(cohortId, newCohort);
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("CohortId")))
                        {
                            Cohort currentCohort = dictCohorts[cohortId];
                            if (!currentCohort.ListofStudents.Exists(ex => ex.Id == reader.GetInt32(reader.GetOrdinal("StudentId"))))
                            {
                                currentCohort.ListofStudents.Add(
                                new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    StudentFirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    StudentLastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    CohortId = cohortId
                                }
                            );
                            }
                            if (!currentCohort.ListofInstructors.Exists(ex => ex.Id == reader.GetInt32(reader.GetOrdinal("InstructorId"))))

                            {
                                currentCohort.ListofInstructors.Add(
                                    new Instructor
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                        InstructorFirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                        InstructorLastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                        CohortId = cohortId
                                    }
                                );
                            }
                        }
                    }
                    reader.Close();
                    return Ok(dictCohorts);
                    // return cohorts.Values.ToList();
                }
            }
        }

        // CODE FOR GETTING A SINGLE COHORT

        [HttpGet("{id}", Name = "GetCohort")]
        // public Cohort Get(int id, string include)
        public IActionResult Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id,
		                                     c.CohortName,                                            
		                                     s.Id AS StudentId, 
		                                     s.StudentFirstName,
		                                     s.StudentLastName,
		                                     s.StudentSlackHandle,
                                             s.CohortId,
                                             i.Id AS InstructorId,
		                                     i.InstructorFirstName,
		                                     i.InstructorLastname,
                                             i.InstructorSlackHandle,
                                             i.CohortId
                                        FROM Cohort c
                                        INNER JOIN Student s on c.Id = s.CohortId
                                        INNER JOIN Instructor i on c.Id = i.CohortId
                                        WHERE c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    while (reader.Read())
                    {
                        if (cohort == null)
                        {
                            cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CohortName = reader.GetString(reader.GetOrdinal("CohortName")),
                                // ListofStudents = new List<Student>(),
                                // ListofInstructors = new List<Instructor>()
                            };
                        }

                        int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));


                        if (!cohort.ListofStudents.Any(s => s.Id == studentId))
                        {
                            Student student = new Student
                            {
                                Id = studentId,
                                StudentFirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                StudentLastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                StudentSlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                CohortId = cohort.Id
                            };
                            cohort.ListofStudents.Add(student);
                        }


                        int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                        if (!cohort.ListofInstructors.Any(i => i.Id == instructorId))
                        {
                            Instructor instructor = new Instructor
                            {
                                Id = instructorId,
                                InstructorFirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                InstructorLastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                InstructorSlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                CohortId = cohort.Id
                            };

                            cohort.ListofInstructors.Add(instructor);
                        }
                    }

                    reader.Close();

                    return Ok(cohort);
                }
            }
        }

        // CODE FOR CREATING A COHORT

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cohort cohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Cohort (CohortName)
                                        OUTPUT INSERTED.Id
                                        VALUES (@eName)";
                    cmd.Parameters.Add(new SqlParameter("@eName", cohort.CohortName));
                    
                    int newId = (int)cmd.ExecuteScalar();
                    cohort.Id = newId;
                    // 
                    // Re-route user back to student they created
                    return CreatedAtRoute("GetCohort", new { id = newId }, cohort);
                }
            }
        }

        // CODE FOR EDITING/UPDATING A COHORT

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Cohort cohort)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Cohort
                                            SET CohortName = @eName
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@eName", cohort.CohortName));                        
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
                if (!CohortExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // CODE FOR DELETING A COHORT

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
                        cmd.CommandText = @"DELETE FROM Cohort WHERE Id = @id";
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
                if (!CohortExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CohortExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, CohortName
                                        FROM Cohort
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

