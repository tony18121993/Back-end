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
    public class TarjetumsController : Controller
    {
        private readonly SpotifyContext _context;

        public TarjetumsController(SpotifyContext context)
        {
            _context = context;
        }

        // GET: Tarjetums
        public async Task<IActionResult> Index()
        {
            var spotifyContext = _context.Tarjeta.Include(t => t.IdUsuarioNavigation);
            return View(await spotifyContext.ToListAsync());
        }

        // GET: Tarjetums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarjetum = await _context.Tarjeta
                .Include(t => t.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdTarjeta == id);
            if (tarjetum == null)
            {
                return NotFound();
            }

            return View(tarjetum);
        }

        // GET: Tarjetums/Create
        public IActionResult Create()
        {
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario");
            return View();
        }

        // POST: Tarjetums/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTarjeta,IdUsuario,NombreTarjeta,NumeroTarjeta,FechaExpiracion,Cvv")] Tarjetum tarjetum)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tarjetum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", tarjetum.IdUsuario);
            return View(tarjetum);
        }

        // GET: Tarjetums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarjetum = await _context.Tarjeta.FindAsync(id);
            if (tarjetum == null)
            {
                return NotFound();
            }
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", tarjetum.IdUsuario);
            return View(tarjetum);
        }

        // POST: Tarjetums/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTarjeta,IdUsuario,NombreTarjeta,NumeroTarjeta,FechaExpiracion,Cvv")] Tarjetum tarjetum)
        {
            if (id != tarjetum.IdTarjeta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tarjetum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TarjetumExists(tarjetum.IdTarjeta))
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
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", tarjetum.IdUsuario);
            return View(tarjetum);
        }

        // GET: Tarjetums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarjetum = await _context.Tarjeta
                .Include(t => t.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdTarjeta == id);
            if (tarjetum == null)
            {
                return NotFound();
            }

            return View(tarjetum);
        }

        // POST: Tarjetums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tarjetum = await _context.Tarjeta.FindAsync(id);
            if (tarjetum != null)
            {
                _context.Tarjeta.Remove(tarjetum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TarjetumExists(int id)
        {
            return _context.Tarjeta.Any(e => e.IdTarjeta == id);
        }
    }
}
