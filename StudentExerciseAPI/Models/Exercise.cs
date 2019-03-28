using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentExerciseAPI.Models

{
    public class Exercise
    {
        public int Id { get; set; }

        [Required]
        public string ExerciseName { get; set; }

        [Required]
        public string Language { get; set; }
    }
}