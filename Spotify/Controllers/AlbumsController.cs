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
    public class AlbumsController : Controller
    {
        private readonly SpotifyContext _context;

        public AlbumsController(SpotifyContext context)
        {
            _context = context;
        }

        // GET: Albums
        public async Task<IActionResult> Index()
        {
            var spotifyContext = _context.Albums.Include(a => a.IdArtistaNavigation);
            return View(await spotifyContext.ToListAsync());
        }

        // GET: Albums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.IdArtistaNavigation)
                .Include(a => a.Canciones)
                .FirstOrDefaultAsync(m => m.IdAlbum == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Albums/Create
        public IActionResult Create(int idArtista)
        {
            var album = new Album { IdArtista = idArtista };
            return View(album);
        }

        // POST: Albums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAlbum,IdArtista,Nombre,Descripcion,Genero,Imagen")] Album album)
        {
            if (ModelState.IsValid)
            {
                var artista = await _context.Artistas.FindAsync(album.IdArtista);
                if (artista == null)
                {
                    ModelState.AddModelError("", "El artista especificado no existe.");
                    return View(album);
                }

                _context.Add(album);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Artistas", new { id = album.IdArtista });
            }
            return View(album);
        }

        // GET: Albums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            ViewData["IdArtista"] = new SelectList(_context.Artistas, "IdArtista", "IdArtista", album.IdArtista);
            return View(album);
        }

        // POST: Albums/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAlbum,IdArtista,Nombre,Descripcion,Genero,Imagen")] Album album)
        {
            if (id != album.IdAlbum)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(album);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.IdAlbum))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = album.IdAlbum });
            }
            ViewData["IdArtista"] = new SelectList(_context.Artistas, "IdArtista", "IdArtista", album.IdArtista);
            return View(album);
        }

        // GET: Albums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.IdArtistaNavigation)
                .FirstOrDefaultAsync(m => m.IdAlbum == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album != null)
            {
                _context.Albums.Remove(album);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlbumExists(int id)
        {
            return _context.Albums.Any(e => e.IdAlbum == id);
        }


        
        [HttpGet]
        [Route("CancionesporArtista/{idArtista}")]
        public async Task<IActionResult> CancionesporArtista(int idArtista)
        {
            Console.WriteLine(idArtista);
            var albums = await _context.Albums
                .Where(a => a.IdArtista == idArtista)
                .Select(a => new
                {
                    a.IdAlbum,
                    a.IdArtista,
                    a.Nombre,
                    a.Descripcion,
                    a.Genero,
                    a.Imagen,
                    Canciones = a.Canciones.Select(c => new
                    {
                        c.IdCancion,
                        c.Nombre,
                        c.Duracion,
                        c.Url
                    }).ToList()
                })
                .ToListAsync();

            return Json(albums);
        }

        //Album por artista
        [HttpGet]
        [Route("AlbumsporArtista/{idArtista}")]
        public async Task<IActionResult> AlbumsporArtista(int idArtista)
        {
            Console.WriteLine(idArtista);
            var albums = await _context.Albums
                .Where(a => a.IdArtista == idArtista)
                .ToListAsync();

            return Json(albums);
        }


        [HttpPost]
        [Authorize]
        [Route("CancionesporAlbum/{idAlbum}")]
        public async Task<IActionResult> CancionesporAlbum(int idAlbum)
        {
            // Obtener el ID del usuario del token
            var userIdString = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

            // Intentar convertir el ID del usuario a un entero
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user ID.");
            }

            // Buscar al usuario en la base de datos
            var user = await _context.Usuarios.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Consultar los álbumes y canciones, e incluir la información del tipo de usuario
            var albums = await _context.Albums
                .Where(a => a.IdAlbum == idAlbum)
                .Select(a => new
                {
                    a.IdAlbum,
                    a.Nombre,
                    a.Genero,
                    a.Imagen,
                    a.Descripcion,
                    Canciones = a.Canciones.Select(c => new
                    {
                        c.IdCancion,
                        c.Nombre,
                        c.Duracion,
                        c.Url
                    }).ToList(),
                    userPremium = user.Premium // Incluir el estado premium del usuario
                })
                .ToListAsync();

            return Json(albums);
        }
                
    }
}
