using System.Collections.Generic;
using Spring2026_Project3_rahandley.Models;

namespace Spring2026_Project3_rahandley.Models.ViewModels
{
    public class ActorMovieCreateViewModel
    {
        public ActorMovie ActorMovie { get; set; }
        public List<Actor> Actors { get; set; }
        public List<Movie> Movies { get; set; }
    }
}