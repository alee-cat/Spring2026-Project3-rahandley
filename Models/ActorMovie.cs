namespace Spring2026_Project3_rahandley.Models;

public class ActorMovie
{
    public int Id { get; set; }
    public int ActorId { get; set; }
    public int MovieId { get; set; }

    public Actor Actor { get; set; }
    public Movie Movie { get; set; }
}