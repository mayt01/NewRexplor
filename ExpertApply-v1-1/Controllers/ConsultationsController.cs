//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using Rexplor.Data;
//using Rexplor.Models;
//using System.ComponentModel.DataAnnotations;
//using System.Security.Claims;

//namespace Rexplor.Controllers
//{
//    [Authorize]
//    public class ConsultationsController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly UserManager<IdentityUser> _userManager;
//        private readonly IWebHostEnvironment _environment;
//        private readonly IEmailSender _emailSender;

//        public ConsultationsController(
//            ApplicationDbContext context,
//            UserManager<IdentityUser> userManager,
//            IWebHostEnvironment environment,
//            IEmailSender emailSender)
//        {
//            _context = context;
//            _userManager = userManager;
//            _environment = environment;
//            _emailSender = emailSender;
//        }

//        // GET: لیست مشاوره‌های کاربر
//        [HttpGet]
//        public async Task<IActionResult> Index()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var consultations = await _context.Consultations
//                .Where(c => c.UserId == user.Id)
//                .OrderByDescending(c => c.CreatedAt)
//                .ToListAsync();

//            return View(consultations);
//        }

//        // GET: فرم رزرو مشاوره
//        [HttpGet]
//        public IActionResult Create()
//        {
//            var model = new ConsultationViewModel
//            {
//                SessionDate = DateTime.Now.AddDays(1),
//                SessionTime = new TimeSpan(14, 0, 0)
//            };

//            ViewBag.ConsultationTypes = GetConsultationTypes();
//            ViewBag.ContactMethods = GetContactMethods();
//            ViewBag.AvailableTimes = GetAvailableTimeSlots();

//            return View(model);
//        }

//        // POST: ثبت رزرو مشاوره
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(ConsultationViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                ViewBag.ConsultationTypes = GetConsultationTypes();
//                ViewBag.ContactMethods = GetContactMethods();
//                ViewBag.AvailableTimes = GetAvailableTimeSlots();
//                return View(model);
//            }

//            try
//            {
//                var user = await _userManager.GetUserAsync(User);

//                // بررسی تداخل زمان
//                var sessionDateTime = model.SessionDate.Add(model.SessionTime);
//                var isTimeSlotAvailable = await IsTimeSlotAvailable(sessionDateTime, model.Duration);

//                if (!isTimeSlotAvailable)
//                {
//                    ModelState.AddModelError("", "این زمان قبلاً رزرو شده است. لطفاً زمان دیگری انتخاب کنید.");
//                    ViewBag.ConsultationTypes = GetConsultationTypes();
//                    ViewBag.ContactMethods = GetContactMethods();
//                    ViewBag.AvailableTimes = GetAvailableTimeSlots();
//                    return View(model);
//                }

//                // آپلود فایل‌های پیوست
//                var attachments = new List<string>();
//                if (model.AttachmentFiles != null && model.AttachmentFiles.Any())
//                {
//                    foreach (var file in model.AttachmentFiles)
//                    {
//                        if (file.Length > 0)
//                        {
//                            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "consultations");
//                            if (!Directory.Exists(uploadsFolder))
//                            {
//                                Directory.CreateDirectory(uploadsFolder);
//                            }

//                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
//                            var filePath = Path.Combine(uploadsFolder, fileName);

//                            using (var stream = new FileStream(filePath, FileMode.Create))
//                            {
//                                await file.CopyToAsync(stream);
//                            }

//                            attachments.Add($"/uploads/consultations/{fileName}");
//                        }
//                    }
//                }

//                // محاسبه قیمت
//                var price = CalculatePrice(model.Type, model.Duration);

//                // ایجاد رکورد مشاوره
//                var consultation = new Consultation
//                {
//                    UserId = user.Id,
//                    Type = model.Type,
//                    Topic = model.Topic,
//                    Description = model.Description,
//                    Attachments = attachments.Any() ? string.Join(",", attachments) : null,
//                    SessionDate = model.SessionDate,
//                    SessionTime = model.SessionTime,
//                    Duration = model.Duration,
//                    ContactMethod = model.ContactMethod,
//                    ContactInfo = model.ContactInfo,
//                    Price = price,
//                    Status = ConsultationStatus.Pending,
//                    CreatedAt = DateTime.Now,
//                    UpdatedAt = DateTime.Now
//                };

//                _context.Consultations.Add(consultation);
//                await _context.SaveChangesAsync();

//                // ارسال ایمیل تأیید
//                await SendConfirmationEmail(user, consultation);

//                TempData["SuccessMessage"] = $"✅ درخواست مشاوره شما با شماره پیگیری {consultation.TrackingCode} ثبت شد.";
//                return RedirectToAction(nameof(Details), new { id = consultation.Id });
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", $"خطا در ثبت درخواست: {ex.Message}");
//                ViewBag.ConsultationTypes = GetConsultationTypes();
//                ViewBag.ContactMethods = GetContactMethods();
//                ViewBag.AvailableTimes = GetAvailableTimeSlots();
//                return View(model);
//            }
//        }

//        // GET: جزئیات مشاوره
//        [HttpGet]
//        public async Task<IActionResult> Details(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var consultation = await _context.Consultations
//                .Include(c => c.User)
//                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

//            if (consultation == null)
//            {
//                return NotFound();
//            }

//            return View(consultation);
//        }

//        // GET: ویرایش مشاوره
//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var consultation = await _context.Consultations
//                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

//            if (consultation == null)
//            {
//                return NotFound();
//            }

//            if (consultation.Status != ConsultationStatus.Pending)
//            {
//                TempData["ErrorMessage"] = "فقط مشاوره‌های در انتظار تایید قابل ویرایش هستند.";
//                return RedirectToAction(nameof(Details), new { id });
//            }

//            var model = new ConsultationViewModel
//            {
//                Type = consultation.Type,
//                Topic = consultation.Topic,
//                Description = consultation.Description,
//                SessionDate = consultation.SessionDate,
//                SessionTime = consultation.SessionTime,
//                Duration = consultation.Duration,
//                ContactMethod = consultation.ContactMethod,
//                ContactInfo = consultation.ContactInfo
//            };

//            ViewBag.ConsultationTypes = GetConsultationTypes();
//            ViewBag.ContactMethods = GetContactMethods();
//            ViewBag.AvailableTimes = GetAvailableTimeSlots();

//            return View(model);
//        }

//        // POST: ویرایش مشاوره
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, ConsultationViewModel model)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var consultation = await _context.Consultations
//                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

//            if (consultation == null)
//            {
//                return NotFound();
//            }

//            if (consultation.Status != ConsultationStatus.Pending)
//            {
//                TempData["ErrorMessage"] = "فقط مشاوره‌های در انتظار تایید قابل ویرایش هستند.";
//                return RedirectToAction(nameof(Details), new { id });
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    // بررسی تداخل زمان (به جز زمان فعلی خودش)
//                    var sessionDateTime = model.SessionDate.Add(model.SessionTime);
//                    var isTimeSlotAvailable = await IsTimeSlotAvailable(sessionDateTime, model.Duration, id);

//                    if (!isTimeSlotAvailable)
//                    {
//                        ModelState.AddModelError("", "این زمان قبلاً رزرو شده است. لطفاً زمان دیگری انتخاب کنید.");
//                        ViewBag.ConsultationTypes = GetConsultationTypes();
//                        ViewBag.ContactMethods = GetContactMethods();
//                        ViewBag.AvailableTimes = GetAvailableTimeSlots();
//                        return View(model);
//                    }

//                    // آپلود فایل‌های جدید
//                    if (model.AttachmentFiles != null && model.AttachmentFiles.Any())
//                    {
//                        var attachments = new List<string>();

//                        // حذف فایل‌های قبلی
//                        if (!string.IsNullOrEmpty(consultation.Attachments))
//                        {
//                            var oldFiles = consultation.Attachments.Split(',');
//                            foreach (var file in oldFiles)
//                            {
//                                var filePath = Path.Combine(_environment.WebRootPath, file.TrimStart('/'));
//                                if (System.IO.File.Exists(filePath))
//                                {
//                                    System.IO.File.Delete(filePath);
//                                }
//                            }
//                        }

//                        // آپلود فایل‌های جدید
//                        foreach (var file in model.AttachmentFiles)
//                        {
//                            if (file.Length > 0)
//                            {
//                                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "consultations");
//                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
//                                var filePath = Path.Combine(uploadsFolder, fileName);

//                                using (var stream = new FileStream(filePath, FileMode.Create))
//                                {
//                                    await file.CopyToAsync(stream);
//                                }

//                                attachments.Add($"/uploads/consultations/{fileName}");
//                            }
//                        }

//                        consultation.Attachments = attachments.Any() ? string.Join(",", attachments) : null;
//                    }

//                    // محاسبه قیمت جدید
//                    var newPrice = CalculatePrice(model.Type, model.Duration);

//                    // بروزرسانی اطلاعات
//                    consultation.Type = model.Type;
//                    consultation.Topic = model.Topic;
//                    consultation.Description = model.Description;
//                    consultation.SessionDate = model.SessionDate;
//                    consultation.SessionTime = model.SessionTime;
//                    consultation.Duration = model.Duration;
//                    consultation.ContactMethod = model.ContactMethod;
//                    consultation.ContactInfo = model.ContactInfo;
//                    consultation.Price = newPrice;
//                    consultation.UpdatedAt = DateTime.Now;

//                    await _context.SaveChangesAsync();

//                    TempData["SuccessMessage"] = "✅ تغییرات با موفقیت ذخیره شد.";
//                    return RedirectToAction(nameof(Details), new { id });
//                }
//                catch (Exception ex)
//                {
//                    ModelState.AddModelError("", $"خطا در ویرایش: {ex.Message}");
//                }
//            }

//            ViewBag.ConsultationTypes = GetConsultationTypes();
//            ViewBag.ContactMethods = GetContactMethods();
//            ViewBag.AvailableTimes = GetAvailableTimeSlots();
//            return View(model);
//        }

//        // POST: لغو مشاوره
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Cancel(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var consultation = await _context.Consultations
//                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

//            if (consultation == null)
//            {
//                return NotFound();
//            }

//            if (consultation.Status != ConsultationStatus.Pending && consultation.Status != ConsultationStatus.Approved)
//            {
//                TempData["ErrorMessage"] = "این مشاوره قابل لغو نیست.";
//                return RedirectToAction(nameof(Details), new { id });
//            }

//            consultation.Status = ConsultationStatus.Cancelled;
//            consultation.UpdatedAt = DateTime.Now;
//            await _context.SaveChangesAsync();

//            TempData["SuccessMessage"] = "✅ مشاوره با موفقیت لغو شد.";
//            return RedirectToAction(nameof(Details), new { id });
//        }

//        // GET: پرداخت مشاوره
//        [HttpGet]
//        public async Task<IActionResult> Payment(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var consultation = await _context.Consultations
//                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

//            if (consultation == null)
//            {
//                return NotFound();
//            }

//            if (consultation.IsPaid)
//            {
//                TempData["InfoMessage"] = "این مشاوره قبلاً پرداخت شده است.";
//                return RedirectToAction(nameof(Details), new { id });
//            }

//            if (consultation.Status != ConsultationStatus.Approved)
//            {
//                TempData["ErrorMessage"] = "فقط مشاوره‌های تایید شده قابل پرداخت هستند.";
//                return RedirectToAction(nameof(Details), new { id });
//            }

//            ViewBag.Consultation = consultation;
//            return View();
//        }

//        // POST: تأیید پرداخت
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ConfirmPayment(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var consultation = await _context.Consultations
//                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

//            if (consultation == null)
//            {
//                return NotFound();
//            }

//            consultation.IsPaid = true;
//            consultation.UpdatedAt = DateTime.Now;
//            await _context.SaveChangesAsync();

//            // ارسال ایمیل تأیید پرداخت
//            await SendPaymentConfirmationEmail(user, consultation);

//            TempData["SuccessMessage"] = "✅ پرداخت با موفقیت ثبت شد.";
//            return RedirectToAction(nameof(Details), new { id });
//        }

//        // ========== متدهای کمکی ==========
//        private SelectList GetConsultationTypes()
//        {
//            var items = Enum.GetValues(typeof(ConsultationType))
//                .Cast<ConsultationType>()
//                .Select(t => new SelectListItem
//                {
//                    Value = ((int)t).ToString(),
//                    Text = t.GetDisplayName()
//                })
//                .ToList();

//            return new SelectList(items, "Value", "Text");
//        }

//        private SelectList GetContactMethods()
//        {
//            var items = Enum.GetValues(typeof(ContactMethod))
//                .Cast<ContactMethod>()
//                .Select(m => new SelectListItem
//                {
//                    Value = ((int)m).ToString(),
//                    Text = m.GetDisplayName()
//                })
//                .ToList();

//            return new SelectList(items, "Value", "Text");
//        }

//        private SelectList GetAvailableTimeSlots()
//        {
//            var slots = new List<SelectListItem>();
//            for (int hour = 9; hour <= 17; hour++)
//            {
//                for (int minute = 0; minute < 60; minute += 30)
//                {
//                    var time = new TimeSpan(hour, minute, 0);
//                    slots.Add(new SelectListItem
//                    {
//                        Value = time.ToString(),
//                        Text = time.ToString(@"hh\:mm")
//                    });
//                }
//            }

//            return new SelectList(slots, "Value", "Text");
//        }

//        private async Task<bool> IsTimeSlotAvailable(DateTime sessionDateTime, int duration, int? excludeId = null)
//        {
//            var endDateTime = sessionDateTime.AddMinutes(duration);

//            var query = _context.Consultations
//                .Where(c => c.Status != ConsultationStatus.Cancelled &&
//                           c.Status != ConsultationStatus.Rejected);

//            if (excludeId.HasValue)
//            {
//                query = query.Where(c => c.Id != excludeId.Value);
//            }

//            var conflictingConsultations = await query
//                .Where(c => (sessionDateTime >= c.SessionDateTime && sessionDateTime < c.EndDateTime) ||
//                           (endDateTime > c.SessionDateTime && endDateTime <= c.EndDateTime) ||
//                           (sessionDateTime <= c.SessionDateTime && endDateTime >= c.EndDateTime))
//                .ToListAsync();

//            return !conflictingConsultations.Any();
//        }

//        private decimal CalculatePrice(ConsultationType type, int duration)
//        {
//            var basePrice = type switch
//            {
//                ConsultationType.Research => 500000,
//                ConsultationType.ResearchProposal => 400000,
//                ConsultationType.ArticleEditing => 600000,
//                ConsultationType.PythonTraining => 300000,
//                ConsultationType.StatisticalAnalysis => 700000,
//                ConsultationType.ScientificWriting => 450000,
//                _ => 350000
//            };

//            var hours = (decimal)duration / 60;
//            return basePrice * hours;
//        }

//        private async Task SendConfirmationEmail(IdentityUser user, Consultation consultation)
//        {
//            var subject = $"تأیید ثبت درخواست مشاوره - {consultation.TrackingCode}";
//            var body = $@"
//                <div style='font-family: Vazir, sans-serif; direction: rtl; text-align: right;'>
//                    <h2 style='color: #034799;'>درخواست مشاوره شما ثبت شد</h2>
//                    <p>سلام {user.UserName}،</p>
//                    <p>درخواست مشاوره شما با مشخصات زیر ثبت شد:</p>
                    
//                    <div style='background: #f8f9fa; padding: 15px; border-radius: 10px; margin: 20px 0;'>
//                        <p><strong>شماره پیگیری:</strong> {consultation.TrackingCode}</p>
//                        <p><strong>نوع مشاوره:</strong> {consultation.Type.GetDisplayName()}</p>
//                        <p><strong>موضوع:</strong> {consultation.Topic}</p>
//                        <p><strong>تاریخ جلسه:</strong> {consultation.SessionDate:yyyy/MM/dd}</p>
//                        <p><strong>ساعت جلسه:</strong> {consultation.SessionTime:hh\:mm}</p>
//                        <p><strong>مدت جلسه:</strong> {consultation.Duration} دقیقه</p>
//                        <p><strong>هزینه:</strong> {consultation.Price.ToString("N0")} تومان</p>
//                        <p><strong>وضعیت:</strong> {consultation.Status.GetDisplayName()}</p>
//                    </div>
                    
//                    <p>پس از تایید توسط کارشناسان، می‌توانید پرداخت را انجام دهید.</p>
//                    <p>با تشکر<br>تیم پشتیبانی Rexplor</p>
//                </div>";

//            await _emailSender.SendEmailAsync(user.Email, subject, body);
//        }

//        private async Task SendPaymentConfirmationEmail(IdentityUser user, Consultation consultation)
//        {
//            var subject = $"تأیید پرداخت مشاوره - {consultation.TrackingCode}";
//            var body = $@"
//                <div style='font-family: Vazir, sans-serif; direction: rtl; text-align: right;'>
//                    <h2 style='color: #27ae60;'>پرداخت شما با موفقیت انجام شد</h2>
//                    <p>سلام {user.UserName}،</p>
//                    <p>پرداخت مشاوره شما تأیید شد. جزئیات جلسه:</p>
                    
//                    <div style='background: #e8f5e9; padding: 15px; border-radius: 10px; margin: 20px 0;'>
//                        <p><strong>شماره پیگیری:</strong> {consultation.TrackingCode}</p>
//                        <p><strong>نوع مشاوره:</strong> {consultation.Type.GetDisplayName()}</p>
//                        <p><strong>تاریخ و زمان:</strong> {consultation.SessionDateTime:yyyy/MM/dd ساعت HH:mm}</p>
//                        <p><strong>روش ارتباط:</strong> {consultation.ContactMethod.GetDisplayName()}</p>
//                        <p><strong>اطلاعات ارتباط:</strong> {consultation.ContactInfo}</p>
//                    </div>
                    
//                    <p>لطفاً ۱۰ دقیقه قبل از زمان مقرر آماده باشید.</p>
//                    <p>با تشکر<br>تیم پشتیبانی Rexplor</p>
//                </div>";

//            await _emailSender.SendEmailAsync(user.Email, subject, body);
//        }
//    }

//    // Extension Method برای نمایش نام enum
//    public static class EnumExtensions
//    {
//        public static string GetDisplayName(this Enum enumValue)
//        {
//            var field = enumValue.GetType().GetField(enumValue.ToString());
//            var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
//                .FirstOrDefault() as DisplayAttribute;

//            return attribute?.Name ?? enumValue.ToString();
//        }
//    }
//}