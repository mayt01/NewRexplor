using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;

namespace Rexplor.Controllers
{
    public class MainPageSliderImageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MainPageSliderImageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MainPageSliderImages
        public async Task<IActionResult> Index()
        {
            var images = _context.MainPageSliderImages.ToList(); // Retrieve all images from the database
            return View(images); // Pass the images to the view
        }

        // GET: MainPageSliderImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainPageSliderImage = await _context.MainPageSliderImages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mainPageSliderImage == null)
            {
                return NotFound();
            }

            return View(mainPageSliderImage);
        }

        // GET: MainPageSliderImages/Create
        public IActionResult Create()
        {
            var model = new MainPageSliderImage
            {
                Caption = string.Empty // Set a default value
            };

            return View(model);
        }

        // POST: MainPageSliderImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(MainPageSliderImage mainPageSliderImage)
        {
            //if (ModelState.IsValid)
            //{
                using (var memoryStream = new MemoryStream())
                {
                    mainPageSliderImage.ImageFile.CopyTo(memoryStream);
                    mainPageSliderImage.ImageData = memoryStream.ToArray();
                }

                _context.MainPageSliderImages.Add(mainPageSliderImage);
                _context.SaveChanges();

                return RedirectToAction("Index");
            //}
            //return View(mainPageSliderImage);
        }


        // GET: MainPageSliderImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainPageSliderImage = await _context.MainPageSliderImages.FindAsync(id);
            if (mainPageSliderImage == null)
            {
                return NotFound();
            }
            return View(mainPageSliderImage);
        }

        // POST: MainPageSliderImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ImagePath,Caption")] MainPageSliderImage mainPageSliderImage)
        {
            if (id != mainPageSliderImage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mainPageSliderImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MainPageSliderImageExists(mainPageSliderImage.Id))
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
            return View(mainPageSliderImage);
        }

        // GET: MainPageSliderImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainPageSliderImage = await _context.MainPageSliderImages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mainPageSliderImage == null)
            {
                return NotFound();
            }

            return View(mainPageSliderImage);
        }

        // POST: MainPageSliderImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mainPageSliderImage = await _context.MainPageSliderImages.FindAsync(id);
            if (mainPageSliderImage != null)
            {
                _context.MainPageSliderImages.Remove(mainPageSliderImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MainPageSliderImageExists(int id)
        {
            return _context.MainPageSliderImages.Any(e => e.Id == id);
        }
    }
}
