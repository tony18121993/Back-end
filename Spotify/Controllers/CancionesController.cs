using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Transfer;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spotify.Models;
using Amazon.S3.Model;

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

       

        //obtener todas las canciones de un album 
        //modificar para que sea authorize
        [HttpGet]
        [Route("CancionesporAlbum/{idAlbum}")]
        public async Task<IActionResult> CancionesporAlbum(int idAlbum)
        {
            Console.WriteLine(idAlbum);
            var canciones = await _context.Canciones
                  .Where(a => a.IdAlbum == idAlbum)
                   .ToListAsync();

            return Json(canciones);
        }

        //Subir canciones bucket s3 
        // GET: /Canciones/Subir
        //[HttpGet("Canciones/CrearCancion")]
        public IActionResult CrearCancion(int idArtista, int idAlbum)
        {
            ViewBag.IdArtista = idArtista;
            ViewBag.IdAlbum = idAlbum;
            return View();
        }

        // POST: /Canciones/Subir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subir(IFormFile cancion, int idArtista, int idAlbum, string nombreCancion,string duracion)
        {
            try
            {
                // Configurar credenciales de AWS desde el archivo de texto
                var credentialsPath = @"C:\Users\TONY\.aws\credentials";

                // Leer el contenido del archivo


                string lines;
                using (StreamReader reader = new StreamReader(credentialsPath))
                {
                    lines = reader.ReadToEnd();
                }
                // Buscar las claves en el contenido
                string accessKeyId = null;
                string secretAccessKey = null;
                string sessionToken = null;

                string[] linesArray = lines.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Iterar sobre las líneas
                foreach (var line in linesArray)
                {
                    if (line.StartsWith("aws_access_key_id="))
                    {
                        accessKeyId = line.Substring("aws_access_key_id=".Length).Trim();
                    }
                    else if (line.StartsWith("aws_secret_access_key="))
                    {
                        secretAccessKey = line.Substring("aws_secret_access_key=".Length).Trim();
                    }
                    else if (line.StartsWith("aws_session_token="))
                    {
                        sessionToken = line.Substring("aws_session_token=".Length).Trim();
                    }
                }

                // Verificar si todas las credenciales fueron encontradas
                if (accessKeyId == null || secretAccessKey == null)
                {
                    Console.WriteLine("No se encontraron las credenciales necesarias en el archivo.");
                }
                else
                {
                    Console.WriteLine("Credenciales obtenidas exitosamente:");
                    Console.WriteLine($"Access Key ID: {accessKeyId}");
                    Console.WriteLine($"Secret Access Key: {secretAccessKey}");
                    Console.WriteLine($"Session Token: {sessionToken ?? "No hay token de sesión"}");
                }
                // Subir la canción a Amazon S3
                using (var client = new AmazonS3Client(accessKeyId, secretAccessKey, sessionToken, Amazon.RegionEndpoint.USEast1)) // Cambiar la región según corresponda
                {
                    // Verificar si la carpeta del artista existe, si no, crearla
                    var artistaFolderKey = $"artistas/{idArtista}/";
                    if (!await DoesS3FolderExistAsync(client, "spotify-bucket-proyecto", artistaFolderKey))
                    {
                        await CreateS3FolderAsync(client, "spotify-bucket-proyecto", artistaFolderKey);
                    }

                    // Verificar si la carpeta del álbum existe, si no, crearla
                    var albumFolderKey = $"artistas/{idArtista}/albumes/{idAlbum}";
                    if (!await DoesS3FolderExistAsync(client, "spotify-bucket-proyecto", albumFolderKey))
                    {
                        await CreateS3FolderAsync(client, "spotify-bucket-proyecto", albumFolderKey);
                    }

                    // Subir la canción al bucket de S3
                    var key = $"{albumFolderKey}/{nombreCancion}"; // Definir la ruta en el bucket de S3
                    using (var transferUtility = new TransferUtility(client))
                    {
                        using (var stream = cancion.OpenReadStream())
                        {
                            await transferUtility.UploadAsync(stream, "spotify-bucket-proyecto", key); // Cambiar "nombre-del-bucket" por el nombre de tu bucket de S3
                        }
                    }

                    // Guardar la URL de la canción en la base de datos
                    var urlCancion = $"https://spotify-bucket-proyecto.s3.amazonaws.com/{key}";
                    var nuevaCancion = new Cancione { Nombre = nombreCancion, Url = urlCancion, IdAlbum = idAlbum ,Duracion=duracion};
                    _context.Canciones.Add(nuevaCancion);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", "Albums", new { id = idAlbum });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al subir la canción a Amazon S3: {ex.Message}");
            }
        }

        private async Task<bool> DoesS3FolderExistAsync(AmazonS3Client client, string bucketName, string folderKey)
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = folderKey,
                Delimiter = "/"
            };

            var response = await client.ListObjectsV2Async(request);

            return response.CommonPrefixes.Count > 0;
        }

        private async Task CreateS3FolderAsync(AmazonS3Client client, string bucketName, string folderKey)
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = folderKey.TrimEnd('/') + "/",
                ContentBody = ""
            };

            await client.PutObjectAsync(request);
        }
    }
}
