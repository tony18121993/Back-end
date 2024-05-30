using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spotify.Models;

namespace Spotify.Controllers
{
    public class CancionesListaReproduccionsController : Controller
    {
        private readonly SpotifyContext _context;

        public CancionesListaReproduccionsController(SpotifyContext context)
        {
            _context = context;
        }

        // GET: CancionesListaReproduccions
        public async Task<IActionResult> Index()
        {
            var spotifyContext = _context.CancionesListaReproduccions.Include(c => c.IdCancionNavigation).Include(c => c.IdListaNavigation);
            return View(await spotifyContext.ToListAsync());
        }

        // GET: CancionesListaReproduccions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancionesListaReproduccion = await _context.CancionesListaReproduccions
                .Include(c => c.IdCancionNavigation)
                .Include(c => c.IdListaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cancionesListaReproduccion == null)
            {
                return NotFound();
            }

            return View(cancionesListaReproduccion);
        }

        // GET: CancionesListaReproduccions/Create
        public IActionResult Create()
        {
            ViewData["IdCancion"] = new SelectList(_context.Canciones, "IdCancion", "IdCancion");
            ViewData["IdLista"] = new SelectList(_context.ListasReproduccions, "IdLista", "IdLista");
            return View();
        }

        // POST: CancionesListaReproduccions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdLista,IdCancion")] CancionesListaReproduccion cancionesListaReproduccion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cancionesListaReproduccion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCancion"] = new SelectList(_context.Canciones, "IdCancion", "IdCancion", cancionesListaReproduccion.IdCancion);
            ViewData["IdLista"] = new SelectList(_context.ListasReproduccions, "IdLista", "IdLista", cancionesListaReproduccion.IdLista);
            return View(cancionesListaReproduccion);
        }

        // GET: CancionesListaReproduccions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancionesListaReproduccion = await _context.CancionesListaReproduccions.FindAsync(id);
            if (cancionesListaReproduccion == null)
            {
                return NotFound();
            }
            ViewData["IdCancion"] = new SelectList(_context.Canciones, "IdCancion", "IdCancion", cancionesListaReproduccion.IdCancion);
            ViewData["IdLista"] = new SelectList(_context.ListasReproduccions, "IdLista", "IdLista", cancionesListaReproduccion.IdLista);
            return View(cancionesListaReproduccion);
        }

        // POST: CancionesListaReproduccions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdLista,IdCancion")] CancionesListaReproduccion cancionesListaReproduccion)
        {
            if (id != cancionesListaReproduccion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cancionesListaReproduccion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CancionesListaReproduccionExists(cancionesListaReproduccion.Id))
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
            ViewData["IdCancion"] = new SelectList(_context.Canciones, "IdCancion", "IdCancion", cancionesListaReproduccion.IdCancion);
            ViewData["IdLista"] = new SelectList(_context.ListasReproduccions, "IdLista", "IdLista", cancionesListaReproduccion.IdLista);
            return View(cancionesListaReproduccion);
        }

        // GET: CancionesListaReproduccions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancionesListaReproduccion = await _context.CancionesListaReproduccions
                .Include(c => c.IdCancionNavigation)
                .Include(c => c.IdListaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cancionesListaReproduccion == null)
            {
                return NotFound();
            }

            return View(cancionesListaReproduccion);
        }

        // POST: CancionesListaReproduccions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cancionesListaReproduccion = await _context.CancionesListaReproduccions.FindAsync(id);
            if (cancionesListaReproduccion != null)
            {
                _context.CancionesListaReproduccions.Remove(cancionesListaReproduccion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CancionesListaReproduccionExists(int id)
        {
            return _context.CancionesListaReproduccions.Any(e => e.Id == id);
        }

        //añadir cancion a playlist desde front-end
        [HttpPost]
        [Authorize]
        [Route("AnadirCancionAPlaylist")]
        public async Task<IActionResult> AnadirCancionAPlaylist([FromBody] CancionesListaReproduccion canclist)
        {
            try
            {
                // Verificar si la canción ya está en la lista de reproducción
                var existingEntry = await _context.CancionesListaReproduccions
                    .FirstOrDefaultAsync(c => c.IdLista == canclist.IdLista && c.IdCancion == canclist.IdCancion);

                // Si ya existe una entrada con la misma combinación de IdLista e IdCancion, devolver un mensaje de error
                if (existingEntry != null)
                {
                    return BadRequest(new { message = "Esta canción ya se encuentra en esta lista de reproducción." });
                }

                // Si la canción no está en la lista de reproducción, añadir la nueva instancia a la base de datos
                _context.CancionesListaReproduccions.Add(canclist);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Canción añadida correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"No se ha podido añadir la canción: {ex.Message}" });
            }
        }

        //eliminar cancion de playlist 
        [HttpPost]
        [Authorize]
        [Route("EliminarCancionAPlaylist")]
        public async Task<IActionResult> EliminarCancionAPlaylist([FromBody] CancionesListaReproduccion canclist)
        {
            try
            {
                // Obtener el ID del usuario autenticado desde los claims
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Usuario no autenticado." });
                }

                // Convertir el ID del usuario autenticado a entero
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "ID de usuario no válido." });
                }

                // Obtener la lista de reproducción y verificar el propietario
                var propietariolista = await _context.ListasReproduccions
                    .FirstOrDefaultAsync(a => a.IdLista == canclist.IdLista);

                if (propietariolista == null)
                {
                    return NotFound(new { message = "Lista de reproducción no encontrada." });
                }

                if (propietariolista.IdUsuario != userId)
                {
                    return BadRequest(new { message = "No eres el dueño de la lista. No puedes eliminar canciones." });
                }

                // Verificar si la canción ya está en la lista de reproducción
                var existingEntry = await _context.CancionesListaReproduccions
                    .FirstOrDefaultAsync(c => c.IdLista == canclist.IdLista && c.IdCancion == canclist.IdCancion);

                // Si no existe una entrada con la misma combinación de IdLista e IdCancion, devolver un mensaje de error
                if (existingEntry == null)
                {
                    return BadRequest(new { message = "Esta canción no se encuentra en esta lista de reproducción." });
                }

                // Si la canción está en la lista de reproducción, eliminarla de la base de datos
                _context.CancionesListaReproduccions.Remove(existingEntry);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Canción eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"No se ha podido eliminar la canción: {ex.Message}" });
            }
        }


    }
}
