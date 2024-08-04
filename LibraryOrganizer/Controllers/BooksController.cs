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
        public async Task<IActionResult> Create([Bind("Id,Genre,Name,Height,Thickness,ShelfId,IsPartOfSet, SetId")] Book book)
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

                /// Check if the book fits the shelf height
                // and if the combined thickness of books on the shelf fits within the shelf width
                if (!shelf.CanAddBook(book))
                {
                    ModelState.AddModelError("", "The book cannot fit on the shelf due to height or width constraints.");
                    ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                    return View(book);
                }

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
                if (book.IsPartOfSet)
                {
                    if (!await HandleBooksInSetAsync(book, shelf))
                    {
                        ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                        return View(book);
                    }
                }
                // Add warning message if the book height is significantly smaller than the shelf height
                if (book.Height + 10 <= shelf.Height)
                {
                    ViewBag.WarningMessage = "The book is significantly shorter than the shelf by more than 10 cm. Are you sure you want to add it?";
                    ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                    shelf.AddBook(book);
                    _context.Update(shelf);
                    await _context.SaveChangesAsync();
                    return View(book);
                }

                try
                {
                    shelf.AddBook(book);
                    _context.Update(shelf);
                    await _context.SaveChangesAsync();
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Id", book.ShelfId);
                    return View(book);
                }

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


        private async Task<bool> HandleBooksInSetAsync(Book book, Shelf shelf)
        {
          
            var booksInSet = await _context.Books
                .Where(b => b.SetId == book.SetId && b.Id != book.Id)
                .ToListAsync();

            if (booksInSet.Any(b => b.ShelfId != book.ShelfId))
            {
                ModelState.AddModelError("", "Books in a set must be placed on the same shelf.");
                return false;
            }

            return true;
        }


    }
}

