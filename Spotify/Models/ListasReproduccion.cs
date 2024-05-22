using System;
using System.Collections.Generic;

namespace Spotify.Models;

public partial class ListasReproduccion
{
    public int IdLista { get; set; }

    public int? IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Publica { get; set; }

    public virtual ICollection<CancionesListaReproduccion> CancionesListaReproduccions { get; set; } = new List<CancionesListaReproduccion>();

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
