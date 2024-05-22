using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Spotify.Models;

public partial class SpotifyContext : DbContext
{
    public SpotifyContext()
    {
    }

    public SpotifyContext(DbContextOptions<SpotifyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Album> Albums { get; set; }

    public virtual DbSet<Artista> Artistas { get; set; }

    public virtual DbSet<Cancione> Canciones { get; set; }

    public virtual DbSet<CancionesListaReproduccion> CancionesListaReproduccions { get; set; }

    public virtual DbSet<ListasReproduccion> ListasReproduccions { get; set; }

    public virtual DbSet<Tarjetum> Tarjeta { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("server=DESKTOP-ET961MS; database=Spotify; integrated security=true; TrustServerCertificate=Yes");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(e => e.IdAlbum).HasName("PK__Album__7414CFD6184957DE");

            entity.ToTable("Album");

            entity.Property(e => e.IdAlbum).HasColumnName("id_album");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Genero)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("genero");
            entity.Property(e => e.IdArtista).HasColumnName("id_artista");
            entity.Property(e => e.Imagen)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("imagen");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdArtistaNavigation).WithMany(p => p.Albums)
                .HasForeignKey(d => d.IdArtista)
                .HasConstraintName("FK__Album__id_artist__440B1D61");
        });

        modelBuilder.Entity<Artista>(entity =>
        {
            entity.HasKey(e => e.IdArtista).HasName("PK__Artistas__98CBB7BB6A5C5875");

            entity.Property(e => e.IdArtista).HasColumnName("id_artista");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Cancione>(entity =>
        {
            entity.HasKey(e => e.IdCancion).HasName("PK__Cancione__A033DA7630C2CB4B");

            entity.Property(e => e.IdCancion).HasColumnName("id_cancion");
            entity.Property(e => e.Duracion).HasColumnName("duracion");
            entity.Property(e => e.IdAlbum).HasColumnName("id_album");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("url");

            entity.HasOne(d => d.IdAlbumNavigation).WithMany(p => p.Canciones)
                .HasForeignKey(d => d.IdAlbum)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Canciones__id_al__46E78A0C");
        });

        modelBuilder.Entity<CancionesListaReproduccion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cancione__3213E83FDEFF90AE");

            entity.ToTable("Canciones_Lista_Reproduccion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdCancion).HasColumnName("id_cancion");
            entity.Property(e => e.IdLista).HasColumnName("id_lista");

            entity.HasOne(d => d.IdCancionNavigation).WithMany(p => p.CancionesListaReproduccions)
                .HasForeignKey(d => d.IdCancion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Canciones__id_ca__4AB81AF0");

            entity.HasOne(d => d.IdListaNavigation).WithMany(p => p.CancionesListaReproduccions)
                .HasForeignKey(d => d.IdLista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Canciones__id_li__49C3F6B7");
        });

        modelBuilder.Entity<ListasReproduccion>(entity =>
        {
            entity.HasKey(e => e.IdLista).HasName("PK__Listas_R__C100E2E53ADF0DCC");

            entity.ToTable("Listas_Reproduccion");

            entity.Property(e => e.IdLista).HasColumnName("id_lista");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Publica).HasColumnName("publica");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ListasReproduccions)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Listas_Re__id_us__3F466844");
        });

        modelBuilder.Entity<Tarjetum>(entity =>
        {
            entity.HasKey(e => e.IdTarjeta).HasName("PK__Tarjeta__E92BCFEA4444634D");

            entity.Property(e => e.IdTarjeta).HasColumnName("id_tarjeta");
            entity.Property(e => e.Cvv).HasColumnName("cvv");
            entity.Property(e => e.FechaExpiracion).HasColumnName("fecha_expiracion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NombreTarjeta)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_tarjeta");
            entity.Property(e => e.NumeroTarjeta)
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("numero_tarjeta");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Tarjeta)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Tarjeta__id_usua__3C69FB99");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__4E3E04AD14A9D7F2");

            entity.ToTable("Usuario");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidos");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Premium).HasColumnName("premium");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
