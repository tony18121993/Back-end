using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spotify.Models;

namespace Spotify.Controllers
{
    public class CancionesController : Controller
    {
        private readonly SpotifyContext _context;

        public CancionesController(SpotifyContext context)
        {
            _context = context;
        }

        // GET: Canciones
        public async Task<IActionResult> Index()
        {
            var spotifyContext = _context.Canciones.Include(c => c.IdAlbumNavigation);
            return View(await spotifyContext.ToListAsync());
        }

        // GET: Canciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancione = await _context.Canciones
                .Include(c => c.IdAlbumNavigation)
                .FirstOrDefaultAsync(m => m.IdCancion == id);
            if (cancione == null)
            {
                return NotFound();
            }

            return View(cancione);
        }

        // GET: Canciones/Create
        public IActionResult Create(int idAlbum)
        {
            ViewBag.IdAlbum = idAlbum;  // Pasar el idAlbum a la vista
            return View();
        }

        // POST: Canciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCancion,Nombre,Duracion,Url,IdAlbum")] Cancione cancione)
        {
            if (!ModelState.IsValid)
            {
                ViewData["IdAlbum"] = new SelectList(_context.Albums, "IdAlbum", "IdAlbum", cancione.IdAlbum);
                return View(cancione);
            }

            try
            {
                // Agregar la canción al contexto y guardar los cambios
                _context.Add(cancione);
                await _context.SaveChangesAsync();

                // Redireccionar al detalle del álbum
                return RedirectToAction("Details", "Albums", new { id = cancione.IdAlbum });
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir al guardar en la base de datos
                Console.WriteLine($"Error al guardar la canción: {ex.Message}");
                ViewData["IdAlbum"] = new SelectList(_context.Albums, "IdAlbum", "IdAlbum", cancione.IdAlbum);
                return View(cancione);
            }
        }

        // GET: Canciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancione = await _context.Canciones.FindAsync(id);
            if (cancione == null)
            {
                return NotFound();
            }
            ViewData["IdAlbum"] = new SelectList(_context.Albums, "IdAlbum", "IdAlbum", cancione.IdAlbum);
            return View(cancione);
        }

        // POST: Canciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Canciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCancion,Nombre,Duracion,Url,IdAlbum")] Cancione cancione)
        {
            if (id != cancione.IdCancion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cancione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CancioneExists(cancione.IdCancion))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Redireccionar al detalle de la canción
                return RedirectToAction("Details", new { id = cancione.IdCancion });
            }
            ViewData["IdAlbum"] = new SelectList(_context.Albums, "IdAlbum", "IdAlbum", cancione.IdAlbum);
            return View(cancione);
        }


        // GET: Canciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancione = await _context.Canciones
                .Include(c => c.IdAlbumNavigation)
                .FirstOrDefaultAsync(m => m.IdCancion == id);
            if (cancione == null)
            {
                return NotFound();
            }

            return View(cancione);
        }

        // POST: Canciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cancione = await _context.Canciones.FindAsync(id);
            if (cancione != null)
            {
                _context.Canciones.Remove(cancione);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        private bool CancioneExists(int id)
        {
            return _context.Canciones.Any(e => e.IdCancion == id);
        }


        [HttpGet]
        [Route("Canciones/listas/{idListaReproduccion}")]
        public async Task<IActionResult> ListaReproduccion(int idListaReproduccion)
        {
            Console.WriteLine(idListaReproduccion);
            var canciones = await _context.Canciones
                   .FromSqlInterpolated($@"
                        SELECT c.id_cancion, c.nombre, c.duracion, c.url, c.id_album
                        FROM Canciones c
                        JOIN Canciones_Lista_Reproduccion clr ON c.id_cancion = clr.id_cancion
                        JOIN Listas_Reproduccion l ON l.id_lista = clr.id_lista
                        WHERE l.id_lista = {idListaReproduccion}")
                   .ToListAsync();

            return Json(canciones);
        }
       
        [HttpGet]
        [Route("CancionesporAlbum/{idAlbum}")]
        public async Task<IActionResult> CancionesporAlbum(int idAlbum)
        {
            Console.WriteLine(idAlbum);
            var canciones = await _context.Canciones
                   .FromSqlInterpolated($@"
                        SELECT c.* 
                        FROM Canciones c
                        JOIN Album art ON c.id_album = art.id_album
                        
                        WHERE c.id_album = {idAlbum}")
                   .ToListAsync();

            return Json(canciones);
        }



    }
}
