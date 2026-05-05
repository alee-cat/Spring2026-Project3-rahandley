using System.Collections.Generic;

namespace Spring2026_Project3_rahandley.Models.ViewModels
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; }
        public List<Actor> Actors { get; set; } = new List<Actor>();
        public List<string> Reviews { get; set; } = new List<string>();
        public List<double> Sentiments { get; set; } = new List<double>();
        public double AverageSentiment { get; set; }
    }
}