using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_rahandley.Data;
using Spring2026_Project3_rahandley.Models;
using Azure.AI.OpenAI;
using Azure;
using System.Text.Json;
using VaderSharp2;
using System;
using Spring2026_Project3_rahandley.Models.ViewModels;

namespace Spring2026_Project3_rahandley.Controllers
{
    public class ActorMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorMoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie);

            return View(await data.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actorMovie == null) return NotFound();

            return View(actorMovie);
        }

        public IActionResult Create()
        {
            var vm = new ActorMovieCreateViewModel
            {
                Actors = _context.Actors.ToList(),
                Movies = _context.Movies.ToList(),
                ActorMovie = new ActorMovie()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActorMovieCreateViewModel vm)
        {
            var exists = _context.ActorMovies
                .Any(am => am.ActorId == vm.ActorMovie.ActorId && am.MovieId == vm.ActorMovie.MovieId);

            if (exists)
            {
                ModelState.AddModelError("", "This actor is already assigned to this movie.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(vm.ActorMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            vm.Actors = _context.Actors.ToList();
            vm.Movies = _context.Movies.ToList();

            return View(vm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorMovies.FindAsync(id);
            if (actorMovie == null) return NotFound();

            var vm = new ActorMovieCreateViewModel
            {
                ActorMovie = actorMovie,
                Actors = _context.Actors.ToList(),
                Movies = _context.Movies.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActorMovieCreateViewModel vm)
        {
            if (id != vm.ActorMovie.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vm.ActorMovie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ActorMovies.Any(e => e.Id == vm.ActorMovie.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            vm.Actors = _context.Actors.ToList();
            vm.Movies = _context.Movies.ToList();

            return View(vm);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actorMovie == null) return NotFound();

            return View(actorMovie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actorMovie = await _context.ActorMovies.FindAsync(id);

            if (actorMovie != null)
            {
                _context.ActorMovies.Remove(actorMovie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}