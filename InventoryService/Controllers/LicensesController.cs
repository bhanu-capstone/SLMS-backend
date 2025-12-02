using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicensesController : ControllerBase
    {
        private readonly ILicenseService _service;

        public LicensesController(ILicenseService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var licenses = await _service.GetAllAsync();
            return Ok(licenses.Select(l => new LicenseDto(l)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var lic = await _service.GetByIdAsync(id);
            return lic == null ? NotFound() : Ok(new LicenseDto(lic));
        }

        [HttpPost]
        public async Task<IActionResult> Create(License license)
        {
            var created = await _service.CreateAsync(license);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, new LicenseDto(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, License update)
        {
            try
            {
                var lic = await _service.UpdateAsync(id, update);
                return Ok(new LicenseDto(lic));
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? product,
            [FromQuery] string? vendor)
        {
            var res = await _service.SearchAsync(product, vendor);
            return Ok(res.Select(l => new LicenseDto(l)));
        }
        [HttpGet("expiring/{days}")]
        public async Task<IActionResult> GetExpiring(int days)
        {
            try
            {
                var result = await _service.GetExpiringLicensesAsync(days);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal error: " + ex.Message);
            }
        }

    }
}
