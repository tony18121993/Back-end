using System;
using System.Collections.Generic;

namespace Spotify.Models;

public partial class Tarjetum
{
    public int IdTarjeta { get; set; }

    public int? IdUsuario { get; set; }

    public string NombreTarjeta { get; set; } = null!;

    public string NumeroTarjeta { get; set; } = null!;

    public DateTime FechaExpiracion { get; set; }

    public int Cvv { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
