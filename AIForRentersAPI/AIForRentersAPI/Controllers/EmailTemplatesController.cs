using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIForRentersAPI.Models;

namespace AIForRentersAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplatesController : ControllerBase
    {
        private readonly AIForRentersDbContext _context;

        public EmailTemplatesController(AIForRentersDbContext context)
        {
            _context = context;
        }

        // GET: api/EmailTemplates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailTemplate>>> GetEmailTemplate()
        {
            return await _context.EmailTemplate.ToListAsync();
        }

        // GET: api/EmailTemplates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmailTemplate>> GetEmailTemplate(int id)
        {
            var emailTemplate = await _context.EmailTemplate.FindAsync(id);

            if (emailTemplate == null)
            {
                return NotFound();
            }

            return emailTemplate;
        }

        // PUT: api/EmailTemplates/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmailTemplate(int id, EmailTemplate emailTemplate)
        {
            if (id != emailTemplate.EmailTemplateId)
            {
                return BadRequest();
            }

            _context.Entry(emailTemplate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmailTemplateExists(id))
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

        // POST: api/EmailTemplates
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<EmailTemplate>> PostEmailTemplate(EmailTemplate emailTemplate)
        {
            _context.EmailTemplate.Add(emailTemplate);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmailTemplate", new { id = emailTemplate.EmailTemplateId }, emailTemplate);
        }

        // DELETE: api/EmailTemplates/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<EmailTemplate>> DeleteEmailTemplate(int id)
        {
            var emailTemplate = await _context.EmailTemplate.FindAsync(id);
            if (emailTemplate == null)
            {
                return NotFound();
            }

            _context.EmailTemplate.Remove(emailTemplate);
            await _context.SaveChangesAsync();

            return emailTemplate;
        }

        private bool EmailTemplateExists(int id)
        {
            return _context.EmailTemplate.Any(e => e.EmailTemplateId == id);
        }
    }
}
