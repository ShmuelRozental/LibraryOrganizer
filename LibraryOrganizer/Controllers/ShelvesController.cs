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
    public class ShelvesController : Controller
    {
        private readonly LibraryContext _context;

        public ShelvesController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Shelves
        public async Task<IActionResult> Index()
        {
            var libraryContext = _context.Shelves.Include(s => s.Library);
            return View(await libraryContext.ToListAsync());
        }

        // GET: Shelves/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shelf = await _context.Shelves
                .Include(s => s.Books)  // Ensure Books are included
                .FirstOrDefaultAsync(s => s.Id == id);
            if (shelf == null)
            {
                return NotFound();
            }

            return View(shelf);
        }

        // GET: Shelves/Create
        public IActionResult Create()
        {
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Height,Width,LibraryId")] Shelf shelf)
        {
           
            if (ModelState.IsValid)
            {

                _context.Add(shelf);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "Id", shelf.LibraryId);
            return View(shelf);
        }

        // GET: Shelves/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shelf = await _context.Shelves.FindAsync(id);
            if (shelf == null)
            {
                return NotFound();
            }
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "Id", shelf.LibraryId);
            return View(shelf);
        }

        // POST: Shelves/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Height,Width,LibraryId")] Shelf shelf)
        {
            if (id != shelf.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shelf);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShelfExists(shelf.Id))
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
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "Id", shelf.LibraryId);
            return View(shelf);
        }

        // GET: Shelves/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shelf = await _context.Shelves
                .Include(s => s.Library)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shelf == null)
            {
                return NotFound();
            }

            return View(shelf);
        }

        // POST: Shelves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shelf = await _context.Shelves.FindAsync(id);
            if (shelf != null)
            {
                _context.Shelves.Remove(shelf);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShelfExists(int id)
        {
            return _context.Shelves.Any(e => e.Id == id);
        }
    }
}
