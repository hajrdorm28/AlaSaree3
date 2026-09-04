using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AlaSaree3.Data;
using AlaSaree3.Models;
using AlaSaree3.ViewModels.Contact;

namespace AlaSaree3.Controllers
{
    // Must be reachable by anonymous / signed-out visitors, since a suspended user
    // is signed out automatically before they land on this page.
    [AllowAnonymous]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? reason = null)
        {
            var model = new ContactCreateViewModel();

            if (string.Equals(reason, "suspended", StringComparison.OrdinalIgnoreCase))
            {
                model.IsSuspensionContext = true;
                model.Reason = ContactReason.AccountSuspension;
                model.Subject = "Account Suspension Appeal";
            }

            // If the visitor is still authenticated (e.g. just wants to ask a general
            // question), prefill their known details for convenience.
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    model.Name = currentUser.FullName;
                    model.Email = currentUser.Email ?? string.Empty;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = _userManager.GetUserId(User);
            }
            else
            {
                // The sender may not currently be authenticated (they may have just been
                // signed out for suspension), but we can still link the message to their
                // account if one exists with the email they provided.
                var matchedUser = await _userManager.FindByEmailAsync(model.Email);
                if (matchedUser != null)
                {
                    userId = matchedUser.Id;
                }
            }

            var contactMessage = new ContactMessage
            {
                Name = model.Name.Trim(),
                Email = model.Email.Trim(),
                UserId = userId,
                Reason = model.Reason,
                Subject = model.Subject.Trim(),
                Message = model.Message.Trim(),
                Status = ContactMessageStatus.New,
                CreatedAt = DateTime.UtcNow
            };

            _context.ContactMessages.Add(contactMessage);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your message has been sent to our administrators. We'll get back to you as soon as possible.";
            return RedirectToAction(nameof(ThankYou));
        }

        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
