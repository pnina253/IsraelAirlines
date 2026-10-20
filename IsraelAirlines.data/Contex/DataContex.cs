using IsraelAirlines.core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


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

        public override int SaveChanges()
        {
            UpdateReservationVersions();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            UpdateReservationVersions();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateReservationVersions()
        {
            var entries = ChangeTracker
                .Entries<Reservation>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.Version = Guid.NewGuid();
            }
        }

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

                entity.Property(a => a.SeatLayout)
                    .IsRequired()
                    .HasMaxLength(20);

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

                entity.HasIndex(f => f.FlightNumber)
                    .IsUnique();

                entity.Property(f => f.Origin)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(f => f.Destination)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(f => f.Price)
                    .HasPrecision(10, 2);

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
                entity.Property(r => r.Version)
                .IsConcurrencyToken();
            });


            modelBuilder.Entity<Aircraft>().HasData(
                new Aircraft
                {
                    Id = 1,
                    Model = "Boeing 737",
                    RegistrationNumber = "4X-AAA",
                    IsActive = true,
                    Rows = 30,
                    SeatLayout = "ABC DEF",
                    BusinessRows = 3
                },
                new Aircraft
                {
                    Id = 2,
                    Model = "Airbus A320",
                    RegistrationNumber = "4X-BBB",
                    IsActive = true,
                    Rows = 30,
                    SeatLayout = "ABC DEF",
                    BusinessRows = 3
                }
            );

            modelBuilder.Entity<Flight>().HasData(
                new Flight
                {
                    Id = 1,
                    FlightNumber = "IA101",
                    AircraftId = 1,
                    Origin = "Tel Aviv",
                    Destination = "London",
                    DepartureTime = new DateTime(2027, 1, 10, 8, 0, 0, DateTimeKind.Utc),
                    ArrivalTime = new DateTime(2027, 1, 10, 13, 0, 0, DateTimeKind.Utc),
                    DurationMinutes = 300,
                    Price = 1250m
                },
                new Flight
                {
                    Id = 2,
                    FlightNumber = "IA202",
                    AircraftId = 2,
                    Origin = "Tel Aviv",
                    Destination = "Paris",
                    DepartureTime = new DateTime(2027, 1, 11, 9, 0, 0, DateTimeKind.Utc),
                    ArrivalTime = new DateTime(2027, 1, 11, 14, 30, 0, DateTimeKind.Utc),
                    DurationMinutes = 330,
                    Price = 1100m
                }
            );


        }
    }
}
