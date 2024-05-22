using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spotify.Models;

namespace Spotify.Controllers
{
    public class ListasReproduccionsController : Controller
    {
        private readonly SpotifyContext _context;

        public ListasReproduccionsController(SpotifyContext context)
        {
            _context = context;
        }

        // GET: ListasReproduccions
        public async Task<IActionResult> Index()
        {
            var spotifyContext = _context.ListasReproduccions.Include(l => l.IdUsuarioNavigation);
            return View(await spotifyContext.ToListAsync());
        }

        // GET: ListasReproduccions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listasReproduccion = await _context.ListasReproduccions
                .Include(l => l.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdLista == id);
            if (listasReproduccion == null)
            {
                return NotFound();
            }

            return View(listasReproduccion);
        }

        // GET: ListasReproduccions/Create
        public IActionResult Create()
        {
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario");
            return View();
        }

        // POST: ListasReproduccions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLista,IdUsuario,Nombre,Publica")] ListasReproduccion listasReproduccion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listasReproduccion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", listasReproduccion.IdUsuario);
            return View(listasReproduccion);
        }

        // GET: ListasReproduccions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listasReproduccion = await _context.ListasReproduccions.FindAsync(id);
            if (listasReproduccion == null)
            {
                return NotFound();
            }
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", listasReproduccion.IdUsuario);
            return View(listasReproduccion);
        }

        // POST: ListasReproduccions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdLista,IdUsuario,Nombre,Publica")] ListasReproduccion listasReproduccion)
        {
            if (id != listasReproduccion.IdLista)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listasReproduccion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListasReproduccionExists(listasReproduccion.IdLista))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", listasReproduccion.IdUsuario);
            return View(listasReproduccion);
        }

        // GET: ListasReproduccions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listasReproduccion = await _context.ListasReproduccions
                .Include(l => l.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdLista == id);
            if (listasReproduccion == null)
            {
                return NotFound();
            }

            return View(listasReproduccion);
        }

        // POST: ListasReproduccions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listasReproduccion = await _context.ListasReproduccions.FindAsync(id);
            if (listasReproduccion != null)
            {
                _context.ListasReproduccions.Remove(listasReproduccion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListasReproduccionExists(int id)
        {
            return _context.ListasReproduccions.Any(e => e.IdLista == id);
        }
    }
}
