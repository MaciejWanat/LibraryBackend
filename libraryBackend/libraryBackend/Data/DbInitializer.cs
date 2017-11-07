using libraryBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libraryBackend.Data
{
    public class DbInitializer
    {
        public static void Initialize(LibraryContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var users = new User[]
            {
                new User{FirstName="Carson",LastName="Alexander"},
                new User{FirstName="Meredith",LastName="Alonso"},
                new User{FirstName="Arturo",LastName="Anand"},
                new User{FirstName="Gytis",LastName="Barzdukas"},
                new User{FirstName="Yan",LastName="Li"},
                new User{FirstName="Peggy",LastName="Justice"},
                new User{FirstName="Laura",LastName="Norman"},
                new User{FirstName="Nino",LastName="Olivetto"}
            };
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            var books = new Book[]
            {
                new Book{Title="XML Developer's Guide", Author="Matthew Gambardella", Genre="Computer", Price=44.95m, Description="An in-depth look at creating applications with XML.", ImagePath =@"/images/booksImages/computer.jpg"},
                new Book{Title="Midnight Rain", Author="Kim Ralls", Genre="Fantasy", Price=5.95m, Description="A former architect battles corporate zombies, an evil sorceress, and her own childhood to become queen of the world.", ImagePath =@"/images/booksImages/fantasy.jpg"},
                new Book{Title="Maeve Ascendant", Author="Eva Corets", Genre="Fantasy", Price=5.95m, Description="After the collapse of a nanotechnology society in England, the young survivors lay the foundation for a new society.", ImagePath =@"/images/booksImages/fantasy.jpg"},
                new Book{Title="Oberon's Legacy", Author="Eva Corets", Genre="Fantasy", Price=5.95m, Description="In post-apocalypse England, the mysterious agent known only as Oberon helps to create a new life for the inhabitants of London.Sequel to Maeve Ascendant.", ImagePath =@"/images/booksImages/fantasy.jpg"},
                new Book{Title="The Sundered Grail", Author="Eva Corets", Genre="Fantasy", Price=50.95m, Description="The two daughters of Maeve, half-sisters, battle one another for control of England.Sequel to Oberon's Legacy.", ImagePath =@"/images/booksImages/fantasy.jpg"},
                new Book{Title="Lover Birds", Author="Cynthia Randall", Genre="Romance", Price=4.95m, Description="When Carla meets Paul at an ornithology conference, tempers fly as feathers get ruffled.", ImagePath =@"/images/booksImages/romance.png"},
                new Book{Title="Splish Splash", Author="Paula Thurman", Genre="Romance", Price=10.95m, Description="A deep sea diver finds true love twenty thousand leagues beneath the sea.", ImagePath =@"/images/booksImages/romance.png"},
            };
            foreach (Book b in books)
            {
                context.Books.Add(b);
            }
            context.SaveChanges();
        }
    }
}
