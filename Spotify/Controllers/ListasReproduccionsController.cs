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
                .Include(l => l.CancionesListaReproduccions)
                    .ThenInclude(clr => clr.IdCancionNavigation) // Incluye la canción para la relación n a n
                .FirstOrDefaultAsync(m => m.IdLista == id);

            if (listasReproduccion == null)
            {
                return NotFound();
            }

            // Cargar todas las canciones de la base de datos
            var todasLasCanciones = await _context.Canciones.ToListAsync();

            // Pasar la lista de reproducción y todas las canciones a la vista
            ViewData["TodasLasCanciones"] = todasLasCanciones;
            return View(listasReproduccion);
        }


        // GET: ListasReproduccions/Create
        public IActionResult Create()
        {
            var usuarios = _context.Usuarios
                                  .Select(u => new { u.IdUsuario, u.Username })
                                  .ToList();

            ViewBag.Usuarios = new SelectList(usuarios, "IdUsuario", "Username");
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

            var usuarios = _context.Usuarios
                                  .Select(u => new { u.IdUsuario, u.Username })
                                  .ToList();

            ViewBag.Usuarios = new SelectList(usuarios, "IdUsuario", "Username");
            return View(listasReproduccion);
        }


        // GET: ListasReproduccions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listaReproduccion = await _context.ListasReproduccions
                .Include(l => l.IdUsuarioNavigation) // Incluye la navegación para obtener el nombre de usuario
                .FirstOrDefaultAsync(m => m.IdLista == id);
            if (listaReproduccion == null)
            {
                return NotFound();
            }

            // Cargar el nombre de usuario en una propiedad temporal del modelo o ViewBag
            ViewBag.Username = listaReproduccion.IdUsuarioNavigation.Username; // Asumiendo que el nombre de usuario está en esta propiedad

            return View(listaReproduccion);
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

            ViewBag.Username = _context.Usuarios
                .Where(u => u.IdUsuario == listasReproduccion.IdUsuario)
                .Select(u => u.Username)
                .FirstOrDefault();

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
            var listaReproduccion = await _context.ListasReproduccions.FindAsync(id);
            if (listaReproduccion == null)
            {
                return NotFound();
            }

            // Obtener todas las relaciones entre canciones y la lista de reproducción
            var relaciones = _context.CancionesListaReproduccions
                .Where(clr => clr.IdLista == id)
                .ToList();

            // Eliminar las relaciones
            _context.CancionesListaReproduccions.RemoveRange(relaciones);

            // Eliminar la lista de reproducción
            _context.ListasReproduccions.Remove(listaReproduccion);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool ListasReproduccionExists(int id)
        {
            return _context.ListasReproduccions.Any(e => e.IdLista == id);
        }
        [HttpPost]
        public async Task<IActionResult> AgregarCancion(int idLista, int idCancion)
        {
            var lista = await _context.ListasReproduccions
                .Include(l => l.CancionesListaReproduccions)
                    .ThenInclude(clr => clr.IdCancionNavigation) // Incluye las canciones relacionadas
                .FirstOrDefaultAsync(l => l.IdLista == idLista);

            if (lista == null)
            {
                return NotFound();
            }

            // Verifica si la canción ya está en la lista
            var cancionExistente = lista.CancionesListaReproduccions.Any(clr => clr.IdCancion == idCancion);
            if (cancionExistente)
            {
                // Establece un mensaje de error en TempData
                TempData["ErrorMessage"] = "La canción ya está incluida en la lista.";
            }
            else
            {
                // Agrega la canción a la lista de reproducción
                lista.CancionesListaReproduccions.Add(new CancionesListaReproduccion
                {
                    IdLista = idLista,
                    IdCancion = idCancion
                });

                await _context.SaveChangesAsync();
            }

            // Redirige a la página de detalles
            return RedirectToAction("Details", new { id = idLista });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCancion(int idLista, int idCancion)
        {
            var lista = await _context.ListasReproduccions
                .Include(l => l.CancionesListaReproduccions)
                .FirstOrDefaultAsync(l => l.IdLista == idLista);

            if (lista == null)
            {
                return NotFound();
            }

            // Encuentra la relación de la canción en la lista
            var cancionEnLista = lista.CancionesListaReproduccions.FirstOrDefault(clr => clr.IdCancion == idCancion);
            if (cancionEnLista != null)
            {
                // Elimina la relación de la canción en la lista
                _context.CancionesListaReproduccions.Remove(cancionEnLista);
                await _context.SaveChangesAsync();
            }

            // Redirige a la página de detalles
            return RedirectToAction("Details", new { id = idLista });
        }

        //crear lista reproduccion desde frontend

        [HttpPost("CrearLista")]
        [Authorize]
        public async Task<IActionResult> CrearLista([FromBody] ListasReproduccion listasReproduccion)
        {
            // Obtener el nombre de usuario desde el contexto del token
            var username = HttpContext.User.Identity.Name;

            // Consultar la base de datos para obtener el usuario
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);

            if (usuario == null)
            {
                // Usuario no encontrado, devolver error de autorización
                return Unauthorized(new { message = "Usuario no autorizado." });
            }

            // Verificar si el usuario es premium
            if (usuario.Premium)
            {
                listasReproduccion.IdUsuario = usuario.IdUsuario;
                // Usuario premium, permitir que la solicitud continúe
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.ListasReproduccions.Add(listasReproduccion);
                        await _context.SaveChangesAsync();
                        return Ok(new { message = "Lista creada correctamente." });
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { message = "No se ha podido guardar la lista. Error: " + ex.Message });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Los datos de la lista son inválidos." });
                }
            }
            else
            {
                // Usuario no premium, devolver error de autorización
                return Unauthorized(new { message = "Usuario no premium." });
            }
        }


        //obtener todas las listas
        [HttpPost("ObtenerListas")]
        [Authorize]
        public async Task<IActionResult> ObtenerListas()
        {
            try
            {
                var listas = await _context.ListasReproduccions.ToListAsync();
                return Ok(listas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Se produjo un error al obtener las listas de reproducción.", error = ex.Message });
            }
        }

        //obtener listas publicas 
        [HttpPost("ObtenerListasPublicas")]
        [Authorize]
        public async Task<IActionResult> ObtenerListasPublicas()
        {
            try
            {
                var listas = await _context.ListasReproduccions
                        .Where(a => a.Publica == true)
                        .ToListAsync();
                return Ok(listas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Se produjo un error al obtener las listas de reproducción.", error = ex.Message });
            }
        }


        //obetner listas privadas asociadas a un usuario
        [HttpPost("ObtenerListasprivadas")]
        [Authorize]
        public async Task<IActionResult> ObtenerListasprivadas()
        {
            try
            {
                // Obtener el ID de usuario del token de autenticación
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { message = "No se pudo obtener el ID de usuario del token." });
                }

                // Consultar las listas de reproducción privadas del usuario
                var listas = await _context.ListasReproduccions
                    .Where(a => a.Publica == false && a.IdUsuario == userId)
                    .ToListAsync();

                return Ok(listas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Se produjo un error al obtener las listas de reproducción.", error = ex.Message });
            }
        }

        //obtener todas las liastan tanto publicas como privadas de un usuario
        [HttpPost("ObtenerListasUsuario")]
        [Authorize]
        public async Task<IActionResult> ObtenerListasUsuario()
        {
            try
            {
                // Obtener el ID de usuario del token de autenticación
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { message = "No se pudo obtener el ID de usuario del token." });
                }

                // Consultar las listas de reproducción privadas del usuario
                var listas = await _context.ListasReproduccions
                    .Where(a => a.IdUsuario == userId)
                    .ToListAsync();

                return Ok(listas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Se produjo un error al obtener las listas de reproducción.", error = ex.Message });
            }
        }
    }
}
