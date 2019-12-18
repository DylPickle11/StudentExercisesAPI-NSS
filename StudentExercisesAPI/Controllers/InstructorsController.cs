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
    public class InstructorsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
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
                    cmd.CommandText = @"SELECT i.Id, 
                                      i.FirstName,
                                      i.LastName, 
                                      i.SlackHandle, 
                                      i.CohortId,
                                      i.Speciality,
                                      c.Id AS CoId,
                                      c.[Name]
                                      FROM instructor i LEFT JOIN Cohort c ON c.Id = i.CohortId ";


                    if (firstName != null)
                    {
                        cmd.CommandText += " AND FirstName LIKE @FirstName";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", "%" + firstName + "%"));
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
                    List<Instructor> instructors = new List<Instructor>();

                    while (reader.Read())
                    {
                        Instructor instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Speciality = reader.GetString(reader.GetOrdinal("Speciality")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),

                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CoId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }

                        };

                        instructors.Add(instructor);
                    }
                    reader.Close();
                    return Ok(instructors);
                }
            }
        }
    }
}
    
