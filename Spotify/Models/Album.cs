using System;
using System.Collections.Generic;

namespace Spotify.Models;

public partial class Album
{
    public int IdAlbum { get; set; }

    public int? IdArtista { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public string? Genero { get; set; }

    public string? Imagen { get; set; }

    public virtual ICollection<Cancione> Canciones { get; set; } = new List<Cancione>();

    public virtual Artista? IdArtistaNavigation { get; set; }
}
