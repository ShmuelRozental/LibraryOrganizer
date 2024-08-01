using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibraryOrganizer.Models
{
    public class Shelf
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public int Height { get; set; }

        [Required]
        public int Width { get; set; }

        [ForeignKey(nameof(Library))]
        public int LibraryId { get; set; }

        
        public Library? Library { get; set; }// Navigation property to Library

        public ICollection<Book> Books { get; set; } = new List<Book>(); // Navigation property to Books ,ensures that it is never null

        // Calculate remaining space on the shelf
        public int RemainingSpace()
        {
            return Width - Books.Sum(b => b.Thickness);
        }

        // Check if a book can be added to the shelf
        public bool CanAddBook(Book book)
        {
            return book.Height <= Height && book.Thickness <= RemainingSpace();
        }


        public void AddBook(Book book)
        {
            // Validate if the book can fit in the shelf based on height and width
            if (book.Height <= this.Height && Books.Sum(b => b.Thickness) + book.Thickness <= this.Width)
            {
                Books.Add(book);
            }
            else
            {
                throw new InvalidOperationException("The book cannot fit in the shelf.");
            }
        }
    }

}

