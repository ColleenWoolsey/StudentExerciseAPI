using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StudentExerciseAPI.Models;

namespace StudentExerciseAPI.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        [Required]
        public string InstructorFirstName { get; set; }

        [Required]
        public string InstructorLastName { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string InstructorSlackHandle { get; set; }

        public string CohortName { get; set; }

        [Required]
        public int CohortId { get; set; }

        public Cohort Cohort { get; set; }
    }
}