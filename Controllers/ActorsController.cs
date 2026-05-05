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
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null) return NotFound();

            var movies = _context.ActorMovies
                .Where(am => am.ActorId == actor.Id)
                .Select(am => am.Movie)
                .ToList();

            var tweets = new List<string>();
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
                            content = $"Generate exactly 10 short tweets about the actor {actor.Name}. Each tweet on a new line. No numbering. No extra text."
                        }
                    },
                    max_tokens = 400
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

                tweets = aiText
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                var analyzer = new SentimentIntensityAnalyzer();

                foreach (var tweet in tweets)
                {
                    var score = analyzer.PolarityScores(tweet);
                    sentiments.Add(score.Compound);
                }
            }
            catch
            {
                tweets = new List<string> { "AI service unavailable." };
                sentiments = new List<double> { 0 };
            }

            var averageSentiment = sentiments.Count > 0 ? sentiments.Average() : 0;

            var vm = new ActorDetailsViewModel
            {
                Actor = actor,
                Movies = movies,
                Tweets = tweets,
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
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,ImdbLink,Photo")] Actor actor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,ImdbLink,Photo")] Actor actor)
        {
            if (id != actor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Actors.Any(e => e.Id == actor.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);

            if (actor != null)
            {
                var relations = _context.ActorMovies.Where(am => am.ActorId == id);
                _context.ActorMovies.RemoveRange(relations);

                _context.Actors.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}