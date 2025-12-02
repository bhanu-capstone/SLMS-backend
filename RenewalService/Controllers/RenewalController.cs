using Microsoft.AspNetCore.Mvc;
using RenewalService.Models;
using RenewalService.Repositories;
using RenewalService.Services;

namespace RenewalService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RenewalController : ControllerBase
    {
        // 1. UPDATE DEPENDENCIES (Class names, not Interfaces)
        private readonly RenewalRepository _repo;
        private readonly RenewalManager _manager;

        public RenewalController(RenewalRepository repo, RenewalManager manager)
        {
            _repo = repo;
            _manager = manager;
        }

        // --------------------------------------------------------
        // BASIC CRUD OPERATIONS (Handled by Repository)
        // --------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(RenewalRecord record)
        {
            await _repo.AddAsync(record);
            return Ok(record);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repo.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RenewalRecord record)
        {
            if (id != record.Id) return BadRequest("ID mismatch");

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Only update editable fields
            existing.Status = record.Status;
            existing.AdminNotes = record.AdminNotes;
            existing.TargetExpiryDate = record.TargetExpiryDate;

            await _repo.UpdateAsync(existing);
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return Ok("Deleted");
        }

        // --------------------------------------------------------
        // COMPLEX OPERATIONS (Handled by Manager)
        // --------------------------------------------------------

        // GET /api/Renewal/expiring/30
        [HttpGet("expiring/{days}")]
        public async Task<IActionResult> GetExpiring(int days)
        {
            // Calls the Manager which talks to Inventory Service
            var result = await _manager.GetExpiringWithStatusAsync(days);
            return Ok(result);
        }

        // POST /api/Renewal/send-reminders/30
        [HttpPost("send-reminders/{days}")]
        public async Task<IActionResult> SendReminders(int days)
        {
            var count = await _manager.SendRemindersAsync(days);
            return Ok($"Reminders sent to {count} vendors.");
        }

        // PUT /api/Renewal/5/renewed
        [HttpPut("{id}/renewed")]
        public async Task<IActionResult> MarkRenewed(int id, [FromBody] string note)
        {
            try
            {
                await _manager.MarkAsRenewedAsync(id, note);
                return Ok("Marked as renewed.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}