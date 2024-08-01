using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryOrganizer.Data;
using LibraryOrganizer.Models;

namespace LibraryOrganizer.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var libraryContext = _context.Books.Include(b => b.Shelf);
            return View(await libraryContext.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Shelf)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {

          

            ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id");
            return View();
        }
        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Genre,Name,Height,Thickness,ShelfId,IsPartOfSet")] Book book)
        {
            if (ModelState.IsValid)
            {
                var shelf = await _context.Shelves
                    .Include(s => s.Books)
                    .FirstOrDefaultAsync(s => s.Id == book.ShelfId);

                if (shelf == null)
                {
                    ModelState.AddModelError("", "Shelf not found.");
                    ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                    return View(book);
                }

                // Check if the book fits the shelf height
                if (book.Height > shelf.Height)
                {
                    ModelState.AddModelError("", "Book height exceeds shelf height.");
                    ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                    return View(book);
                }

             

                // Check if the combined thickness of books on the shelf fits within the shelf width
                double totalThickness = shelf.Books.Sum(b => b.Thickness) + book.Thickness;
                if (totalThickness > shelf.Width)
                {
                    ModelState.AddModelError("", "Combined thickness of books exceeds shelf width.");
                    ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                    return View(book);
                }

                // Check if the book genre is allowed by the library
                var library = await _context.Libraries
                    .FirstOrDefaultAsync(l => l.Genre == book.Genre);

                if (library == null)
                {
                    ModelState.AddModelError("", "Library not found.");
                    ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                    return View(book);
                }

                var libraryByShelf = await _context.Libraries
                    .FirstOrDefaultAsync(l => l.Id == shelf.LibraryId);

                if (libraryByShelf.Genre != null && libraryByShelf.Genre != book.Genre)
                {
                    ModelState.AddModelError("", "Book genre is not allowed in the library.");
                    ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                    return View(book);
                }
                // Check if all books in a set are on the same shelf
                if (book.IsPartOfSet)
                {
                    var booksInSet = await _context.Books
                        .Where(b => b.IsPartOfSet && b.ShelfId == book.ShelfId)
                        .ToListAsync();

                    if (booksInSet.Any())
                    {
                        ModelState.AddModelError("", "Books in a set must be placed on the same shelf.");
                        ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                        return View(book);
                    }
                }

               // Add warning message if the book height is significantly smaller than the shelf height
                if (book.Height + 10 <= shelf.Height)
                {
                    ViewBag.WarningMessage = "The book is significantly shorter than the shelf by more than 10 cm. Are you sure you want to add it?";
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
            return View(book);
        }

    

    // GET: Books/Edit/5
    public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Genre,Name,Height,Thickness,ShelfId,IsPartOfSet")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Shelf)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}



//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using LibraryOrganizer.Interfaces;
//using LibraryOrganizer.Models;
//using Microsoft.EntityFrameworkCore;
//using LibraryOrganizer.Data;

//namespace LibraryOrganizer.Controllers
//{
//    public class BooksController : Controller
//    {
//        private readonly LibraryContext _context;
//        private readonly ILibraryService _libraryService;

//        public BooksController(LibraryContext context, ILibraryService libraryService)
//        {
//            _context = context;
//            _libraryService = libraryService;
//        }


//        // GET: Books
//        public async Task<IActionResult> Index()
//        {
//            var books = await _context.Books.Include(b => b.Shelf).ToListAsync();
//            return View(books);
//        }

//    // GET: Books/Create
//    public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: Books/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        // POST: Books/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("Id,Genre,Name,Height,Thickness")] Book book)
//        {
//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    // Validate genre here
//                    if (book.Genre != "Janer") // Replace with your logic for allowed genres
//                    {
//                        ModelState.AddModelError("", "Only books of genre 'Janer' can be added.");
//                        return View(book);
//                    }

//                    // Call the service to add the book to the library
//                    await _libraryService.AddBookToLibraryAsync(book);
//                    return RedirectToAction(nameof(Index));
//                }
//                catch (Exception ex)
//                {
//                    ModelState.AddModelError("", ex.Message);
//                }
//            }
//            return View(book);
//        }

//        // GET: Books/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            // Implement logic to get the book for editing
//            return View(); // Placeholder
//        }

//        // POST: Books/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("Id,Genre,Name,Height,Thickness")] Book book)
//        {
//            if (id != book.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                // Implement logic to update the book
//                return RedirectToAction(nameof(Index));
//            }
//            return View(book);
//        }

//        // GET: Books/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            // Implement logic to get the book for deletion
//            return View(); // Placeholder
//        }

//        // POST: Books/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            // Implement logic to delete the book
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}

