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
    public class CohortsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortsController(IConfiguration config)
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

        [HttpGet] // Code for getting a list of cohorts
        public async Task<IActionResult> Get([FromQuery] string name)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT
                                          s.Id AS StudentId,
                                          s.FirstName As StudentFirstName,
                                          s.LastName As StudentLastName,
                                          s.SlackHandle As StudentSlackHandle,
                                          s.CohortId AS StudentCohortId,
                                          c.[Name],
                                          c.Id,
                                          i.Id AS InstructorId,
                                          i.FirstName AS InstructorFirstName,
                                          i.LastName AS  InstructorLastName,
                                          i.SlackHandle As InstructorSlackHandle,
                                          i.CohortId As InstructorCohortId
                                          FROM Student s
                                          LEFT JOIN Cohort c ON  s.CohortId = c.Id
                                          LEFT JOIN Instructor i ON i.CohortId = c.Id
                                          WHERE 1=1";

                    if (name != null)
                    {
                        cmd.CommandText += " AND [Name] LIKE @Name";
                        cmd.Parameters.Add(new SqlParameter("@Name", "%" + name + "%"));
                    }
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();
                    List<Student> students = new List<Student>();
                    Cohort cohort = new Cohort();

                    while (reader.Read())
                    {
                        cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Instructors = instructors,
                            Students = students
                        };

                        Instructor instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                            FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohortId")),

                        };
                        instructors.Add(instructor);

                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId"))
                        };
                        students.Add(student);
                    }
                    reader.Close();
                    return Ok(cohort);
                };
            }
        }
    }
}


