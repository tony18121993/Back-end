using System;
using System.Collections.Generic;

namespace Spotify.Models;

public partial class Cancione
{
    public int IdCancion { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Duracion { get; set; }

    public string Url { get; set; } = null!;

    public int IdAlbum { get; set; }

    public virtual ICollection<CancionesListaReproduccion>? CancionesListaReproduccions { get; set; } = new List<CancionesListaReproduccion>();

    public virtual Album? IdAlbumNavigation { get; set; } = null!;
}
