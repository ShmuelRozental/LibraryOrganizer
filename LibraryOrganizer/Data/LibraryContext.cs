using System.Collections.Generic;
using LibraryOrganizer.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryOrganizer.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }
        
        public DbSet<Book> Books { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<Library> Libraries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configure Library entity
            modelBuilder.Entity<Library>()
                .HasMany(l => l.Shelves)
                .WithOne(s => s.Library)
                .HasForeignKey(s => s.LibraryId)
                .OnDelete(DeleteBehavior.Cascade); //  defines cascade delete behavior

            // Configure Shelf entity
            modelBuilder.Entity<Shelf>()
                .HasMany(s => s.Books)
                .WithOne(b => b.Shelf)
                .HasForeignKey(b => b.ShelfId)
                .OnDelete(DeleteBehavior.Cascade); // defines cascade delete behavior

            // Configure Book entity
            

            base.OnModelCreating(modelBuilder);
        }


    }


    
}
   