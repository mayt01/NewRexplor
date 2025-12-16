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
    public class MainPageVoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MainPageVoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MainPageVoices
        public async Task<IActionResult> Index()
        {
            return View(await _context.MainPageVoices.ToListAsync());
        }

        // GET: MainPageVoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainPageVoice = await _context.MainPageVoices
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mainPageVoice == null)
            {
                return NotFound();
            }

            return View(mainPageVoice);
        }

        // GET: MainPageVoices/Create
        public IActionResult Create()
        {
            var model = new MainPageVoice
            {
                Caption = string.Empty,
                Name = string.Empty
                // Set a default value
            };
            return View(model);
        }

        // POST: MainPageVoices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(MainPageVoice mainPageVoice)
        {
            //if (ModelState.IsValid)
            //{
            using (var memoryStream = new MemoryStream())
            {
                mainPageVoice.VoiceFile.CopyTo(memoryStream);
                mainPageVoice.VoiceData = memoryStream.ToArray();
            }

            _context.MainPageVoices.Add(mainPageVoice);
            _context.SaveChanges();

            return RedirectToAction("Index", "MainPageVoices");
            //}
            //return View(mainPageSliderImage);
        }

        // GET: MainPageVoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainPageVoice = await _context.MainPageVoices.FindAsync(id);
            if (mainPageVoice == null)
            {
                return NotFound();
            }
            return View(mainPageVoice);
        }

        // POST: MainPageVoices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Caption,VoiceData")] MainPageVoice mainPageVoice)
        {
            if (id != mainPageVoice.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mainPageVoice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MainPageVoiceExists(mainPageVoice.Id))
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
            return View(mainPageVoice);
        }

        // GET: MainPageVoices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainPageVoice = await _context.MainPageVoices
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mainPageVoice == null)
            {
                return NotFound();
            }

            return View(mainPageVoice);
        }

        // POST: MainPageVoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mainPageVoice = await _context.MainPageVoices.FindAsync(id);
            if (mainPageVoice != null)
            {
                _context.MainPageVoices.Remove(mainPageVoice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MainPageVoiceExists(int id)
        {
            return _context.MainPageVoices.Any(e => e.Id == id);
        }
    }
}
