using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using libraryBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace libraryBackend.Data
{
    public class LibraryContext : IdentityDbContext<LibraryUser>
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        //public DbSet<LibraryUser> LibraryUsers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<LibraryUser>().ToTable("Users");
            modelBuilder.Entity<Book>().ToTable("Books");
            modelBuilder.Entity<Rental>().ToTable("Rentals");
        }
    }
}
