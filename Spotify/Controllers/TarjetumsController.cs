using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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

            var tarjeta = await _context.Tarjeta
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (tarjeta == null)
            {
                return NotFound();
            }

            return View(tarjeta);
        }

        // GET: Tarjetums/Create
        public IActionResult Create(int idUsuario)
        {
            var tarjetum = new Tarjetum { IdUsuario = idUsuario };

            // Pasar el objeto Tarjetum a la vista
            return View(tarjetum);
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

                // Cambiar el estado del usuario a premium si aún no lo es
                var usuario = await _context.Usuarios.FindAsync(tarjetum.IdUsuario);
                if (usuario != null && !usuario.Premium)
                {
                    usuario.Premium = true;
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }

                // Redireccionar a la vista de detalles del usuario
                return RedirectToAction("Details", "Usuarios", new { id = tarjetum.IdUsuario });
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
                return RedirectToAction("Details", "Usuarios", new { id = tarjetum.IdUsuario });
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
                var usuario = await _context.Usuarios.FindAsync(tarjetum.IdUsuario);
                if (usuario != null)
                {
                    usuario.Premium = false;  // Actualizar el atributo Premium del usuario a false
                    _context.Update(usuario); // Guardar los cambios en el contexto
                }

                _context.Tarjeta.Remove(tarjetum);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Usuarios", new { id = tarjetum.IdUsuario });
        }

        private bool TarjetumExists(int id)
        {
            return _context.Tarjeta.Any(e => e.IdTarjeta == id);
        }


        //añadir tarjeta desde font-end
        [HttpPost]
        [Authorize]
        [Route("Agregartarjeta")]
        public async Task<IActionResult> Agregartarjeta([FromBody] Tarjetum tarjetum)
        {
            try
            {
                // Obtener el nombre de usuario del usuario autenticado
                var username = HttpContext.User.Identity.Name;

                // Consultar la base de datos para obtener el usuario
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);

                if (usuario == null)
                {
                    // Usuario no encontrado, devolver error de autorización
                    return Unauthorized(new { message = "Usuario no autorizado." });
                }

                // Verificar si el usuario es premium
                if (!usuario.Premium)
                {
                   

                    // Crear una nueva instancia de Tarjetum con la fecha de expiración adecuada
                    var nuevaTarjeta = new Tarjetum
                    {
                        IdUsuario = usuario.IdUsuario,
                        NombreTarjeta = tarjetum.NombreTarjeta,
                        NumeroTarjeta = tarjetum.NumeroTarjeta,
                        FechaExpiracion = tarjetum.FechaExpiracion,
                        Cvv = tarjetum.Cvv
                    };

                    // Añadir la nueva tarjeta a la base de datos
                    _context.Add(nuevaTarjeta);
                    await _context.SaveChangesAsync();

                    // Cambiar al usuario a premium
                    usuario.Premium = true;
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Tarjeta agregada y usuario ahora es premium." });
                }
                else
                {
                    return BadRequest(new { message = "El usuario ya es premium." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"No se ha podido agregar la tarjeta: {ex.Message}" });
            }
        }




    }
}
