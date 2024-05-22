using System;
using System.Collections.Generic;

namespace Spotify.Models;

public partial class Artista
{
    public int IdArtista { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
}
