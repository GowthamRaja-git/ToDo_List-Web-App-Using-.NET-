using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Data;
using TodoListApp.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Mail;


namespace TodoListApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            var todos = await _context.Todos.ToListAsync();
            return View(todos);
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            return View(new Todo());
        }

        // POST: Home/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Task,DeadlineDate,DeadlineTime")] Todo todo)
        {
            if (string.IsNullOrWhiteSpace(todo.Task))
            {
                ModelState.AddModelError("Task", "The Task field is required.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(todo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }



        // GET: Home/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }
            return View(todo);
        }

        // POST: Home/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Task,Done,DeadlineDate,DeadlineTime")] Todo todo)
        {
            if (id != todo.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(todo.Task))
            {
                ModelState.AddModelError("Task", "The Task field is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(todo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TodoExists(todo.Id))
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
            return View(todo);
        }



        // POST: Home/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo != null)
            {
                _context.Todos.Remove(todo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Home/ToggleDone/5
        [HttpPost]
        public async Task<IActionResult> ToggleDone(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo != null)
            {
                todo.Done = !todo.Done;
                _context.Update(todo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Home/Assign/5
        [HttpPost]
        public async Task<IActionResult> Assign([FromBody] AssignTaskModel model)
        {
            // Save assignment to the database
            var todo = await _context.Todos.FindAsync(model.Id);
            if (todo == null)
            {
                return NotFound();
            }

            todo.AssignedTo = model.Email;
            _context.Todos.Update(todo);
            await _context.SaveChangesAsync();

            // Send email
            try
            {
                var smtpClient = new SmtpClient("your_email_server")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("your_email", "your_password"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("Your_email"),
                    Subject = "Task Assigned",
                    Body = $"<p>Task: {model.Task}</p><p>Deadline Date: {model.DeadlineDate}</p><p>Deadline Time: {model.DeadlineTime}</p>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(model.Email);

                await smtpClient.SendMailAsync(mailMessage);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log exception or handle it accordingly
                return Json(new { success = false, message = "Failed to send email" });
            }
        }

        private bool TodoExists(int id)
        {
            return _context.Todos.Any(e => e.Id == id);
        }
    }

    public class AssignmentModel
    {
        public string? Email { get; set; }
    }

    public class AssignTaskModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Task { get; set; }
        public string DeadlineDate { get; set; }
        public string DeadlineTime { get; set; }
    }
}
