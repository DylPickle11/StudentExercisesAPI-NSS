using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StudentExercisesAPI.Models;

namespace StudentExercisesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet] // Code for getting a list of exercises
        public async Task<IActionResult> Get([FromQuery] string firstName, string lastName, string slackHandle)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                
                    cmd.CommandText = @"SELECT s.Id, 
                                      s.FirstName,
                                      s.LastName, 
                                      s.SlackHandle, 
                                      s.CohortId,
                                      c.Id AS CoId,
                                      c.[Name]
                                      FROM Student s LEFT JOIN Cohort c ON c.Id = s.CohortId
                                      WHERE 1=1";

                    if (firstName != null) 
                    {
                        cmd.CommandText += " AND FirstName LIKE @FirstName";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", "%" + firstName +"%"));
                    }

                    if (lastName != null)
                    {
                        cmd.CommandText += " AND LastName LIKE @LastName";
                        cmd.Parameters.Add(new SqlParameter("@LastName", "%" + lastName + "%"));
                    }

                    if (slackHandle != null)
                    {
                        cmd.CommandText += " AND SlackHandle LIKE @SlackHandle";
                        cmd.Parameters.Add(new SqlParameter("@SlackHandle", "%" + slackHandle + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CoId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }

                        };

                        students.Add(student);
                    }
                    reader.Close();

                    return Ok(students);
                }
            }
        }

        [HttpGet("{id}", Name = "GetStudent")] //Code for getting a single exercise
        public async Task<IActionResult> Get([FromRoute] int id, string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    if (include == "exercise")
                    {
                        cmd.CommandText = @"
                        SELECT
                         s.Id,
                        se.StudentId,
                         s.FirstName,
                         s.LastName,
                         s.SlackHandle,
                         e.[Name],
                         e.[Language],
                        se.ExerciseId
                        FROM StudentExercise se LEFT JOIN Student s ON s.Id = se.StudentId LEFT JOIN Exercise e ON e.Id = se.ExerciseId";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();
                         

                        List<Exercise> exercises = new List<Exercise>();
                        Student student = null;

                        if (reader.Read())
                        {
                            Exercise exercise = new Exercise
                            {
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Language = reader.GetString(reader.GetOrdinal("Language"))
                            };

                            exercises.Add(exercise);

                            student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                //CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Exercises = exercises

                            };
                        }
                            reader.Close();
                        return Ok(student);
                    }
                    else
                    {
                        cmd.CommandText = @"
                        SELECT *
                        FROM Student
                        WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Student student1 = null;

                        if (reader.Read())
                        {
                            student1 = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                            };
                        }
                        reader.Close();
                        return Ok(student1);

                    };

                    }
                }
            }
        }
    }



//[HttpGet] // Code for getting a list of exercises
//public async Task<IActionResult> Get()
//{
//    using (SqlConnection conn = Connection)
//    {
//        conn.Open();
//        using (SqlCommand cmd = conn.CreateCommand())
//        {
//            cmd.CommandText = "SELECT * FROM Exercise";
//            SqlDataReader reader = cmd.ExecuteReader();
//            List<Exercise> exercises = new List<Exercise>();

//            while (reader.Read())
//            {
//                Exercise exercise = new Exercise
//                {
//                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
//                    Name = reader.GetString(reader.GetOrdinal("Name")),
//                    Language = reader.GetString(reader.GetOrdinal("Language"))

//                };

//                exercises.Add(exercise);
//            }
//            reader.Close();

//            return Ok(exercises);
//        }
//    }
//}

//[HttpGet] // Code for getting a list of exercises
//public async Task<IActionResult> Get()
//{
//    using (SqlConnection conn = Connection)
//    {
//        conn.Open();
//        using (SqlCommand cmd = conn.CreateCommand())
//        {
//            cmd.CommandText = "SELECT * FROM Exercise";
//            SqlDataReader reader = cmd.ExecuteReader();
//            List<Exercise> exercises = new List<Exercise>();

//            while (reader.Read())
//            {
//                Exercise exercise = new Exercise
//                {
//                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
//                    Name = reader.GetString(reader.GetOrdinal("Name")),
//                    Language = reader.GetString(reader.GetOrdinal("Language"))

//                };

//                exercises.Add(exercise);
//            }
//            reader.Close();

//            return Ok(exercises);
//        }
//    }
//}

//[HttpGet("{id}", Name = "GetExercise")] //Code for getting a single exercise
//public async Task<IActionResult> Get([FromRoute] int id)
//{
//    using (SqlConnection conn = Connection)
//    {
//        conn.Open();
//        using (SqlCommand cmd = conn.CreateCommand())
//        {
//            cmd.CommandText = @"
//                    SELECT *
//                    FROM Exercise
//                    WHERE Id = @id";
//            cmd.Parameters.Add(new SqlParameter("@id", id));
//            SqlDataReader reader = cmd.ExecuteReader();

//            Exercise exercise = null;

//            if (reader.Read())
//            {
//                exercise = new Exercise
//                {
//                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
//                    Name = reader.GetString(reader.GetOrdinal("Name")),
//                    Language = reader.GetString(reader.GetOrdinal("Language"))

//                };
//            }
//            reader.Close();

//            return Ok(exercise);
//        }
//    }
//}

//[HttpPost] //Code for creating an exercise
//public async Task<IActionResult> Post([FromBody] Exercise exercise)
//{
//    using (SqlConnection conn = Connection)
//    {
//        conn.Open();
//        using (SqlCommand cmd = conn.CreateCommand())
//        {
//            cmd.CommandText = @"INSERT INTO Exercise (Name, Language)
//                                    OUTPUT INSERTED.Id
//                                    VALUES (@Name, @Language)";
//            cmd.Parameters.Add(new SqlParameter("@Name", exercise.Name));
//            cmd.Parameters.Add(new SqlParameter("@Language", exercise.Language));

//            int newId = (int)cmd.ExecuteScalar();
//            exercise.Id = newId;
//            return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
//        }
//    }
//}


//[HttpPut("{id}")] //Code for editing an exercise
//public async Task<IActionResult> Put([FromRoute] int id, [FromBody]Exercise exercise)
//{
//    try
//    {
//        using (SqlConnection conn = Connection)
//        {
//            conn.Open();
//            using (SqlCommand cmd = conn.CreateCommand())
//            {
//                cmd.CommandText = @"UPDATE Exercise
//                                        SET Name = @Name,
//                                        Language = @Language
//                                        WHERE Id = @id";
//                cmd.Parameters.Add(new SqlParameter("@Name", exercise.Name));
//                cmd.Parameters.Add(new SqlParameter("@Language", exercise.Language));
//                cmd.Parameters.Add(new SqlParameter("@id", id));

//                int rowsAffected = cmd.ExecuteNonQuery();
//                if (rowsAffected > 0)
//                {
//                    return new StatusCodeResult(StatusCodes.Status204NoContent);
//                }
//                return BadRequest($"No Exercise with id: {id}");
//            }
//        }
//    }
//    catch (Exception)
//    {
//        if (!ExerciseExists(id))
//        {
//            return NotFound();
//        }
//        else
//        {
//            throw;
//        }
//    }
//}

//[HttpDelete("{id}")] //Code for deleting an exercise
//public async Task<IActionResult> Delete([FromRoute] int id)
//{
//    try
//    {
//        using (SqlConnection conn = Connection)
//        {
//            conn.Open();
//            using (SqlCommand cmd = conn.CreateCommand())
//            {
//                cmd.CommandText = @"DELETE FROM Exercise WHERE Id = @id";
//                cmd.Parameters.Add(new SqlParameter("@id", id));

//                int rowsAffected = cmd.ExecuteNonQuery();
//                if (rowsAffected > 0)
//                {
//                    return new StatusCodeResult(StatusCodes.Status204NoContent);
//                }
//                throw new Exception("No rows affected");
//            }
//        }
//    }
//    catch (Exception)
//    {
//        if (!ExerciseExists(id))
//        {
//            return NotFound();
//        }
//        else
//        {
//            throw;
//        }
//    }
//}

//private bool ExerciseExists(int id)
//{
//    using (SqlConnection conn = Connection)
//    {
//        conn.Open();
//        using (SqlCommand cmd = conn.CreateCommand())
//        {
//            cmd.CommandText = @"
//                    SELECT *
//                    FROM Exercise
//                    WHERE Id = @id";
//            cmd.Parameters.Add(new SqlParameter("@id", id));

//            SqlDataReader reader = cmd.ExecuteReader();
//            return reader.Read();
//        }
//    }
//}



