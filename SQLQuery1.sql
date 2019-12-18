SELECT
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
                                          WHERE 1=1
                                          AND [Name] Like 'DAY 35';
