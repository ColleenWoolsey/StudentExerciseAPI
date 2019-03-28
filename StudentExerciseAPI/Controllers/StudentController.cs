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
// Configuration object gives us access to appsettings.json
// We change the default database when want to go from local environment to test to production

    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)            
        {
            _config = config;
            // Andy wrote it as - this.configuration = configuration;
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
                // return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // CODE FOR GETTING A LIST
        // GET: api/Students?include="exercise" ... this is the query parameter

        // First if is to add student and second is to add exercise
        /* Dictionary isn't important when linking to one cohort, but matters for exercise */
        /* GET: api/Students?include=exercise&q=bob */

        [HttpGet]

        public IEnumerable<Student> Get(string include, string q)
        // Previously query string parameters wrote - public async Task<IActionResult> Get()       
        {
            using (SqlConnection conn = Connection)
            // connection we set up line 24?
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                { 
                    // if (include == "exercise") means {Get all the exercises} else {only get student}

                    if (include == "exercise")
                    {
                        cmd.CommandText = @"SELECT s.Id as StudentId,
                                            s.StudentFirstName,
                                            s.StudentLastname,
                                            s.StudentSlackHandle,
                                            s.CohortId,
                                            c.[CohortName] as CohortName,
                                            e.Id as ExerciseId,
                                            e.[Exercisename] as ExerciseName,
                                            e.[Language]
                                    FROM student s
                                            LEFT JOIN Cohort c ON s.CohortId = c.Id
                                            LEFT JOIN ExerciseIntersection ei ON s.Id = ei.studentId
                                            LEFT JOIN Exercise e ON ei.ExerciseId = e.Id
                                    WHERE 1 = 1";
                    }                            
                    else
                    {
                        cmd.CommandText = @"SELECT s.Id as StudentId,
                                                    s.StudentFirstName,
                                                    s.StudentLastName,
                                                    s.StudentSlackHandle,
                                                    s.CohortId,
                                                    c.[CohortName] as CohortName
                                            FROM Student s
                                            LEFT JOIN Cohort c on s.CohortId = c.Id
                                            Where 1 = 1";
                    }

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        // WHERE 1 = 1; is always true, but doesn't afect result - Gives us a "q" in query to tack an "AND" onto
                        // cmd.CommandText += @" AND ... Adding this to the previous commandText string
                        cmd.CommandText += @" AND
                                s.FirstName LIKE @q OR
                                s.LastName Like @q OR
                                s.SlackHandle LIKE @q)";

                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    // Executes CommandText above - Note - in more complex apps there might be stuff between CommandText and execute                    

                    Dictionary<int, Student> students = new Dictionary<int, Student>();

                    while (reader.Read())
                    // Loop and Get the data from each row
                    {
                        int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                        if (!students.ContainsKey(studentId))
                        {
                            Student newStudent = new Student()
                            {
                                // Use the studentId from above
                                Id = studentId,
                                StudentFirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                StudentLastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                StudentSlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    CohortName = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };

                            students.Add(studentId, newStudent);
                        }

                        if (include == "exercise")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {
                                Student currentStudent = students[studentId];
                                currentStudent.Exercises.Add(
                                    new Exercise
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                        ExerciseName = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                        Language = reader.GetString(reader.GetOrdinal("Language"))
                                    });
                            }
                        }
                    }

                    reader.Close();

                    // if use IActionResult have to return with Ok
                    /* if (students.count == 0)
                    {
                        return NoContent();                       
                    }
                    else { 
                    return Ok(students); */

                    return students.Values.ToList();
                }
            }
        }

// CODE FOR GETTING A SINGLE ITEM
        // Get: api/Student/5?include=exercise
        [HttpGet("{id}", Name = "GetOneStudent")]

        public Student Get(int id, string include)
        // public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "exercise")
                    {
                        cmd.CommandText = @"SELECT s.Id as StudentId,
                                               s.StudentFirstName,
                                               s.StudentLastname,
                                               s.StudentSlackHandle,
                                               s.CohortId,
                                               c.[CohortName] as CohortName,
                                               e.Id as ExerciseId,
                                               e.[Exercisename] as ExerciseName,
                                               e.[Language]
                                        FROM student s
                                            LEFT JOIN Cohort c ON s.CohortId = c.Id
                                            LEFT JOIN ExerciseIntersection ei ON s.Id = ei.studentId
                                            LEFT JOIN Exercise e ON ei.ExerciseId = e.Id
                                            WHERE s.Id = @id";                                       
                    }
                    else
                    {
                        cmd.CommandText = @"SELECT s.Id as StudentId,
                                                        s.StudentFirstName,
                                                        s.StudentLastName,
                                                        s.StudentSlackHandle,
                                                        s.CohortId,
                                                        c.[CohortName] as CohortName
                                                FROM Student s
                                                INNER JOIN Cohort c on s.CohortId = c.Id
                                                WHERE s.Id = @id";                        
                    }
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Adding cmd.Parameter for id here is the difference between get all and get single object
                    // Andy used scalar instead of reader ????

                    Student student = null;

                    while (reader.Read())
                    {
                        if (student == null)
                        {
                            student = new Student
                            {
                                Id = id,
                                StudentFirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                StudentLastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                StudentSlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    CohortName = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };
                        }

                        if (include == "exercise")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {
                                student.Exercises.Add(
                                    new Exercise
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                        ExerciseName = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                        Language = reader.GetString(reader.GetOrdinal("Language"))
                                    }
                                );
                            }
                        }
                    }
                    reader.Close();

                    return student;
                }
            }
        }

// CODE FOR ADDING/INSERTING AN ITEM

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student newStudent)
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

                    cmd.Parameters.Add(new SqlParameter("@firstName", newStudent.StudentFirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", newStudent.StudentLastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", newStudent.StudentSlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", newStudent.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    newStudent.Id = newId;
                    // 
                    // Re-route user back to student they created
                    return CreatedAtRoute("GetStudent", new { id = newId }, newStudent);
                }
            }
        }

// CODE FOR EDITING/UPDATING AN ITEM

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Student editedStudent)
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
                        
                        cmd.Parameters.Add(new SqlParameter("@firstName", editedStudent.StudentFirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", editedStudent.StudentLastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", editedStudent.StudentSlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortid", editedStudent.CohortId));
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
