using StudentExercisesAPI.Models;
using System;
using System.Collections.Generic;

namespace StudentExerciseAPI.Models
{
    public class Cohort
    {
        public int Id { get; set; }
        public string CohortName { get; set; }
        public List<Student> ListofStudents { get; set; }
        public List<Instructor> ListofInstructors { get; set; }
    }
}