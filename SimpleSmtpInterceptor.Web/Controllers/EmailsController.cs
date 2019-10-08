using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSmtpInterceptor.Data;
using SimpleSmtpInterceptor.Data.Entities;

namespace SimpleSmtpInterceptor.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailsController : ControllerBase
    {
        private readonly InterceptorModel _context;

        public EmailsController(InterceptorModel context)
        {
            _context = context;
        }

        // GET: api/Emails
        [HttpGet]
        public IEnumerable<Email> GetEmails()
        {
            var q = _context
                .Emails
                .OrderByDescending(x => x.CreatedOnUtc);

            return q;
        }

        // GET: api/Emails/TopN/50
        [HttpGet("TopN/{top}")]
        public IEnumerable<Email> GetEmailsTopN([FromRoute] int top)
        {
            var q = GetEmails()
                .Take(top);
            
            return q;
        }

        // GET: api/Emails/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmail([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = await _context.Emails.FindAsync(id);

            if (email == null)
            {
                return NotFound();
            }

            return Ok(email);
        }

        // PUT: api/Emails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmail([FromRoute] long id, [FromBody] Email email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != email.EmailId)
            {
                return BadRequest();
            }

            _context.Entry(email).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Emails
        [HttpPost]
        public async Task<IActionResult> PostEmail([FromBody] Email email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Emails.Add(email);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmail", new { id = email.EmailId }, email);
        }

        // DELETE: api/Emails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmail([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = await _context.Emails.FindAsync(id);
            if (email == null)
            {
                return NotFound();
            }

            _context.Emails.Remove(email);
            await _context.SaveChangesAsync();

            return Ok(email);
        }

        [HttpDelete("DeleteAll")]
        public IActionResult DeleteAllEmail()
        {
            _context.Database.ExecuteSqlCommand("TRUNCATE TABLE [dbo].[Email]");

            return Ok();
        }

        private bool EmailExists(long id)
        {
            return _context.Emails.Any(e => e.EmailId == id);
        }
    }
}