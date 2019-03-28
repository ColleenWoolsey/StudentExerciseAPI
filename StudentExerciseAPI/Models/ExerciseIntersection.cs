using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExerciseAPI.Models
{
    public class ExerciseIntersection
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ExerciseId { get; set; }

        public object Exercise { get; internal set; }

        public object Student { get; internal set; }
    }
}