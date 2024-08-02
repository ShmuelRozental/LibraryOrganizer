namespace LibraryOrganizer.Models
{
    public class Library
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Genre { get; set; }

        public List<Shelf> Shelves { get; set; } = new List<Shelf>();  //ensures that it is never nu

       
    }
}
