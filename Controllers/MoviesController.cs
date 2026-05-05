using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_rahandley.Data;
using Spring2026_Project3_rahandley.Models;
using System.Text.Json;
using System.Text;
using VaderSharp2;
using Spring2026_Project3_rahandley.Models.ViewModels;

namespace Spring2026_Project3_rahandley.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            var actors = _context.ActorMovies
                .Where(am => am.MovieId == movie.Id)
                .Select(am => am.Actor)
                .ToList();

            var reviews = new List<string>();
            var sentiments = new List<double>();

            try
            {
                var httpClient = new HttpClient();
                var config = HttpContext.RequestServices.GetService<IConfiguration>();

                httpClient.DefaultRequestHeaders.Add("api-key", config["OpenAI:Key"]);

                var requestBody = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = $"Generate exactly 5 short reviews for the movie {movie.Title}. Each review on a new line. No numbering. No extra text."
                        }
                    },
                    max_tokens = 300
                };

                var contentJson = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync(
                    $"{config["OpenAI:Endpoint"]}/openai/deployments/gpt-4.1-mini/chat/completions?api-version=2024-02-15-preview",
                    contentJson
                );

                var responseString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseString);

                var aiText = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                reviews = aiText
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                var analyzer = new SentimentIntensityAnalyzer();

                foreach (var review in reviews)
                {
                    var score = analyzer.PolarityScores(review);
                    sentiments.Add(score.Compound);
                }
            }
            catch
            {
                reviews = new List<string> { "AI service unavailable." };
                sentiments = new List<double> { 0 };
            }

            var averageSentiment = sentiments.Count > 0 ? sentiments.Average() : 0;

            var vm = new MovieDetailsViewModel
            {
                Movie = movie,
                Actors = actors,
                Reviews = reviews,
                Sentiments = sentiments,
                AverageSentiment = averageSentiment
            };

            return View(vm);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Genre,Year,ImdbLink,Poster")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,Year,ImdbLink,Poster")] Movie movie)
        {
            if (id != movie.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Movies.Any(e => e.Id == movie.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie != null)
            {
                var relations = _context.ActorMovies.Where(am => am.MovieId == id);
                _context.ActorMovies.RemoveRange(relations);

                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}