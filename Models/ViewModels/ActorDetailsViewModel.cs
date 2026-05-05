using System.Collections.Generic;

namespace Spring2026_Project3_rahandley.Models.ViewModels
{
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; }
        public List<Movie> Movies { get; set; } = new List<Movie>();
        public List<string> Tweets { get; set; } = new List<string>();
        public List<double> Sentiments { get; set; } = new List<double>();
        public double AverageSentiment { get; set; }
    }
}