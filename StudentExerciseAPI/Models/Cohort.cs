using StudentExercisesAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExerciseAPI.Models
{
    public class Cohort
    {
        public int Id { get; set; }

        // Message "Cohort name should be in the format of [Day|Evening] [number]"


        [Required]
        [StringLength(11, MinimumLength = 5)]
        // [\b(Evening | Day)\b.]
        public string CohortName { get; set; }

        public List<Student> ListofStudents { get; set; } = new List<Student>();

        public List<Instructor> ListofInstructors { get; set; } = new List<Instructor>();
    }
}