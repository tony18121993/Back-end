using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Spotify.Models;

namespace Spotify.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly SpotifyContext _context;
        private readonly JwtConfig _jwtConfig;

        public UsuariosController(SpotifyContext context, IOptions<JwtConfig> jwtConfig)
        {
            _context = context;
            _jwtConfig = jwtConfig.Value;
            //Console.WriteLine("Valor de SecretKey en el controlador: " + _jwtConfig.SecretKey);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Username == loginModel.Username);

                if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginModel.Password, usuario.Password))
                {
                    return Unauthorized(new { message = "Username or password is incorrect" });
                }

                var token = GenerateJwtToken(usuario);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                // Registro detallado del error para diagnóstico
                Console.WriteLine($"Error in Login: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            Console.WriteLine("Valor de SecretKey en GenerateJwtToken: " + _jwtConfig.SecretKey);

            // Verificar si la clave secreta del JWT no es nula
            if (string.IsNullOrEmpty(_jwtConfig.SecretKey))
            {
                throw new InvalidOperationException("La clave secreta del JWT no está configurada correctamente.");
            }

            var key = Encoding.UTF8.GetBytes(_jwtConfig.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = "Spotify",
                Audience = "Spotify"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        public class LoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUsuario,Username,Password,Nombre,Apellidos,FechaNacimiento,Telefono,Premium,Admin,Email")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,Username,Password,Nombre,Apellidos,FechaNacimiento,Telefono,Premium,Admin,Email")] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
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
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }

        //comprobar el tipo de usuario 
        [HttpPost("usuario/tipo")] // Ruta para obtener el tipo de usuario
        [Authorize] // Requiere autenticación para acceder a este método
        public async Task<IActionResult> ObtenerTipoUsuario()
        {
            // Obtener el nombre de usuario desde el contexto de la solicitud
            var username = HttpContext.User.Identity.Name;

            // Consultar la base de datos para obtener el usuario con el nombre de usuario actual
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Obtener el tipo de usuario del usuario encontrado en la base de datos
            var tipoUsuario = usuario.Premium;

            return Ok(new { tipoUsuario });
        }

        [HttpPost("usuario/administrador")] // Ruta para obtener el tipo de usuario
        [Authorize] // Requiere autenticación para acceder a este método
        public async Task<IActionResult> ObtenerAdministrador()
        {
            // Obtener el nombre de usuario desde el contexto de la solicitud
            var username = HttpContext.User.Identity.Name;

            // Consultar la base de datos para obtener el usuario con el nombre de usuario actual
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Obtener el tipo de usuario del usuario encontrado en la base de datos
            var tipoUsuario = usuario.Admin;
            Console.WriteLine(tipoUsuario);
            return Ok(new { tipoUsuario });
        }


        //crear usuario desde front-end
        [AllowAnonymous]
        [HttpPost("/usuario/crearusuario")]
        public async Task<IActionResult> CrearUsuario([FromBody] Usuario usuario)
        {
            Console.WriteLine("Aqui " + usuario);

            // Comprobar si ya existe un usuario con el mismo username
            var existingUser = await _context.Usuarios
                                             .FirstOrDefaultAsync(u => u.Username == usuario.Username);
            if (existingUser != null)
            {
                return Conflict(new { message = "El nombre de usuario ya está en uso" });
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Error: {error.Key} - {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return BadRequest(ModelState);
            }

            Console.WriteLine(usuario.FechaNacimiento);
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
            usuario.Admin = false;
            usuario.Premium = false;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario creado exitosamente" });
        }





    }
}
