using IsraelAirlines.core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.data.Contex
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Aircraft> Aircrafts => Set<Aircraft>();
        public DbSet<Flight> Flights => Set<Flight>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(254);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.PasswordHash)
                    .IsRequired();
            });

            modelBuilder.Entity<Aircraft>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Model)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(a => a.RegistrationNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(a => a.RegistrationNumber)
                    .IsUnique();

                entity.HasMany(a => a.Flights)
                    .WithOne(f => f.Aircraft)
                    .HasForeignKey(f => f.AircraftId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.Property(f => f.FlightNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(f => f.Origin)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(f => f.Destination)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasMany(f => f.Reservations)
                    .WithOne(r => r.Flight)
                    .HasForeignKey(r => r.FlightId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.SeatNumber)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.HasIndex(r => new { r.FlightId, r.SeatNumber })
                    .IsUnique();

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
