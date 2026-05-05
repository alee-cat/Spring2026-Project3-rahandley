namespace Spring2026_Project3_rahandley.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Genre { get; set; }
    public int Year { get; set; }
    public string ImdbLink { get; set; }
    public byte[] Poster { get; set; }  
}