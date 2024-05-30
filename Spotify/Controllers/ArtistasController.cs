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
    public class ArtistasController : Controller
    {
        private readonly SpotifyContext _context;

        public ArtistasController(SpotifyContext context)
        {
            _context = context;
        }

        // GET: Artistas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Artistas.ToListAsync());
        }

        // GET: Artistas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artista = await _context.Artistas
                .Include(a => a.Albums)
                .FirstOrDefaultAsync(m => m.IdArtista == id);
            if (artista == null)
            {
                return NotFound();
            }

            return View(artista);
        }

        // GET: Artistas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Artistas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdArtista,Nombre,Descripcion")] Artista artista)
        {
            if (ModelState.IsValid)
            {
                _context.Add(artista);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(artista);
        }

        // GET: Artistas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null)
            {
                return NotFound();
            }
            return View(artista);
        }

        // POST: Artistas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdArtista,Nombre,Descripcion")] Artista artista)
        {
            if (id != artista.IdArtista)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(artista);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtistaExists(artista.IdArtista))
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
            return View(artista);
        }

        // GET: Artistas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artista = await _context.Artistas
                .FirstOrDefaultAsync(m => m.IdArtista == id);
            if (artista == null)
            {
                return NotFound();
            }

            return View(artista);
        }

        // POST: Artistas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Buscar al artista por su id
            var artista = await _context.Artistas.FindAsync(id);

            if (artista == null)
            {
                return NotFound();
            }

            // Buscar los álbumes del artista
            var albumesDelArtista = await _context.Albums.Where(a => a.IdArtista == id).ToListAsync();

            // Buscar las canciones de los álbumes del artista
            var cancionesDelArtista = new List<Cancione>();
            foreach (var album in albumesDelArtista)
            {
                var cancionesEnAlbum = await _context.Canciones.Where(c => c.IdAlbum == album.IdAlbum).ToListAsync();
                cancionesDelArtista.AddRange(cancionesEnAlbum);
            }

            // Eliminar las entradas en la tabla CancionesListaReproduccion asociadas a las canciones del artista
            foreach (var cancion in cancionesDelArtista)
            {
                var entradasListaReproduccion = await _context.CancionesListaReproduccions
                    .Where(clr => clr.IdCancion == cancion.IdCancion)
                    .ToListAsync();

                _context.CancionesListaReproduccions.RemoveRange(entradasListaReproduccion);
            }

            // Eliminar las canciones del artista
            _context.Canciones.RemoveRange(cancionesDelArtista);

            // Eliminar los álbumes del artista
            _context.Albums.RemoveRange(albumesDelArtista);

            // Finalmente, eliminar al artista
            _context.Artistas.Remove(artista);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }





        private bool ArtistaExists(int id)
        {
            return _context.Artistas.Any(e => e.IdArtista == id);
        }

        [HttpGet]
        [Route("ArtistasTodos")]
        public async Task<IActionResult> ArtistasTodos()
        {
            var artistasConImagen = await _context.Artistas
                .GroupJoin(
                    _context.Albums,
                    artista => artista.IdArtista,
                    album => album.IdArtista,
                    (artista, albums) => new
                    {
                        Artista = artista,
                        PrimerAlbumImagen = albums.OrderBy(album => album.IdArtista).Select(album => album.Imagen).FirstOrDefault()
                    })
                .Select(a => new
                {
                    a.Artista.IdArtista,
                    a.Artista.Nombre,
                    a.Artista.Descripcion,
                    ImagenPrimerAlbum = a.PrimerAlbumImagen
                })
                .ToListAsync();

            return Json(artistasConImagen);
        }
        [HttpGet]
        [Route("ArtistasporNombre/{nombre}")]
        public async Task<IActionResult> ArtistasporNombre(string nombre)
        {
            // Registro para depuración
            Console.WriteLine("Nombre del string: " + nombre);

            // Asegurarse de que el nombre no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest("Nombre cannot be empty.");
            }

            // Convertir el nombre a minúsculas para la búsqueda
            string nombreLower = nombre.ToLower();

            // Buscar artistas cuyo nombre contenga el string dado y obtener la imagen del primer álbum
            var artistasConImagen = await _context.Artistas
                .Where(a => a.Nombre.ToLower().Contains(nombreLower))
                .GroupJoin(
                    _context.Albums,
                    artista => artista.IdArtista,
                    album => album.IdArtista,
                    (artista, albums) => new
                    {
                        Artista = artista,
                        PrimerAlbumImagen = albums.OrderBy(album => album.IdArtista).Select(album => album.Imagen).FirstOrDefault()
                    })
                .Select(a => new
                {
                    a.Artista.IdArtista,
                    a.Artista.Nombre,
                    a.Artista.Descripcion,
                    ImagenPrimerAlbum = a.PrimerAlbumImagen
                })
                .ToListAsync();

            // Registro de depuración para el resultado de la consulta
            Console.WriteLine("Número de artistas encontrados: " + artistasConImagen.Count);

            if (artistasConImagen == null || !artistasConImagen.Any())
            {
                return NotFound("No artists found with the given name.");
            }

            return Ok(artistasConImagen);
        }




    }
}
