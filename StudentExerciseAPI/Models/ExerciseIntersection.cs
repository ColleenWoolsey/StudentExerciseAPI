using System;
using System.Collections.Generic;

namespace StudentExerciseAPI.Models
{
    public class ExerciseIntersection
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ExerciseId { get; set; }
        public object Exercise { get; internal set; }
        public object Student { get; internal set; }
    }
}