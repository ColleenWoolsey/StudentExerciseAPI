using System;
using System.Collections.Generic;
using StudentExerciseAPI.Models;

namespace StudentExerciseAPI.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        public string InstructorFirstName { get; set; }
        public string InstructorLastName { get; set; }
        public string InstructorSlackHandle { get; set; }
        public string CohortName { get; set; }
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; }
    }
}