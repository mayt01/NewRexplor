
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rexplor.Models;
using System.Security.Claims;

namespace Rexplor.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, IEmailSender emailSender, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        // مرحله 1: وارد کردن ایمیل
        [HttpGet]
        public IActionResult Step1() => View();

        [HttpPost]
        public async Task<IActionResult> Step1(RegisterStep1Model model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "این ایمیل قبلاً ثبت شده است");
                    return View(model);
                }

                var code = new Random().Next(100000, 999999).ToString();
                HttpContext.Session.SetString("Email", model.Email);
                HttpContext.Session.SetString("VerificationCode", code);

                await _emailSender.SendEmailAsync(model.Email, "کد تایید", $"کد شما: {code}");

                return RedirectToAction("Step2");
            }
            return View(model);
        }

        // مرحله 2: تایید کد
        [HttpGet]
        public IActionResult Step2() => View();

        [HttpPost]
        [ValidateAntiForgeryToken] // امنیتی برای جلوگیری از CSRF
        public IActionResult Step2(RegisterStep2Model model)
        {
            // بررسی اعتبار مدل
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // دریافت کد تأیید ذخیره شده در Session
            var savedCode = HttpContext.Session.GetString("VerificationCode");

            // مقایسه کد وارد شده با کد ذخیره شده
            if (model.VerificationCode == savedCode)
            {
                // هدایت به مرحله بعد
                return RedirectToAction("Step3");
            }

            // اگر کد اشتباه بود، اضافه کردن خطای عمومی
            ModelState.AddModelError(string.Empty, "کد تأیید اشتباه است");

            // بازگرداندن View با پیام خطا
            return View(model);
        }

        // مرحله 3: ثبت نام نهایی
        [HttpGet]
        public IActionResult Step3() => View();

        [HttpPost]
        public async Task<IActionResult> Step3(RegisterStep3Model model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = HttpContext.Session.GetString("Email");
            if (email == null) return RedirectToAction("Step1");

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new Claim("FullName", model.FullName));

                // ورود خودکار کاربر
                await _signInManager.SignInAsync(user, isPersistent: false);

                HttpContext.Session.Remove("Email");
                HttpContext.Session.Remove("VerificationCode");

                return RedirectToAction("RegisterSuccess");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        public IActionResult RegisterSuccess() => View();

    }

}
