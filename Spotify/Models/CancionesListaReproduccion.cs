using System;
using System.Collections.Generic;

namespace Spotify.Models;

public partial class CancionesListaReproduccion
{
    public int Id { get; set; }

    public int IdLista { get; set; }

    public int IdCancion { get; set; }

    public virtual Cancione? IdCancionNavigation { get; set; } = null!;

    public virtual ListasReproduccion? IdListaNavigation { get; set; } = null!;
}
