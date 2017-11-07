using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using libraryBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace libraryBackend.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Book>().ToTable("Book");
            modelBuilder.Entity<Rental>().ToTable("Rental");
        }
    }
}
