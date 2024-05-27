using System;
using System.Collections.Generic;

namespace Spotify.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string PasswordMasked => new string('*', 5);

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public DateOnly FechaNacimiento { get; set; }

    public string Telefono { get; set; } = null!;

    public bool Premium { get; set; }

    public bool Admin { get; set; }

    public string Email { get; set; }


    public virtual ICollection<ListasReproduccion> ListasReproduccions { get; set; } = new List<ListasReproduccion>();

    public virtual ICollection<Tarjetum> Tarjeta { get; set; } = new List<Tarjetum>();
}
