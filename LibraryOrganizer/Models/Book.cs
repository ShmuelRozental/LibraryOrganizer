using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LibraryOrganizer.Attributes;

namespace LibraryOrganizer.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Genre { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public int Height { get; set; }

        [Required]
        public int Thickness { get; set; }

        [ForeignKey(nameof(Shelf))]
        public int ShelfId { get; set; }

        
        public Shelf? Shelf { get; set; }

        [RequiredIfPartOfSet]
        public int? SetId { get; set; }
        public bool IsPartOfSet { get; set; }
    }
}
