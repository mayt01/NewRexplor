using Microsoft.AspNetCore.Mvc;
using Rexplor.Models;
using Rexplor.Data;

namespace Rexplor.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly ApplicationDbContext _context; // Reference to your DbContext
        private readonly EmailService _emailService;

        public ContactUsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Contact/Index
        public IActionResult Index()
        {
            var messages = _context.ContactUsMessages.ToList(); // Fetch all messages from the database
            return View(messages);
        }

        // GET: /Contact/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Contact/Create
        [HttpPost]
        public async Task<IActionResult> CreateAsync(ContactUsMessage model)
        {
            if (ModelState.IsValid)
            {
                string subject = "New Contact Us Message from " + model.Name.ToString();
                string body = model.Message.ToString() + "\n" + "email address of sender: " + model.Email.ToString();
                try
                {
                    await _emailService.SendEmailAsync("expertapplyinfo@gmail.com", subject, body);
                    //await _emailService.SendEmailAsync(model.Email.ToString(), "Do Not Reply", "Hi " + model.Name.ToString() + ". We received your message.");
                    ViewBag.Message = "Your message has been sent successfully.";
                }
                catch
                {
                    ViewBag.Message = "There was an error sending your message. Please try again later.";
                }



                _context.ContactUsMessages.Add(model); // Save the message to the database
                _context.SaveChanges();
                TempData["PopupMessage"] = "Thank you for contacting us! Your message has been sent successfully.";
                return RedirectToAction("Index", "Home");
            }
            //return View("Index", "Home");
            return RedirectToAction("Index", "Home");
        }
    }
}
