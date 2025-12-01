
using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Finance")]
    public class EntitlementsController : ControllerBase
    {
        private readonly IEntitlementService _service;

        public EntitlementsController(IEntitlementService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ents = await _service.GetAllAsync();
            return Ok(ents.Select(e => new EntitlementDto(e)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var ent = await _service.GetByIdAsync(id);
            return ent == null ? NotFound() : Ok(new EntitlementDto(ent));
        }

        [HttpPost]
        public async Task<IActionResult> Assign(Entitlement entitlement)
        {
            try
            {
                var assigned = await _service.AssignAsync(entitlement);
                return Ok(new EntitlementDto(assigned));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

    }
}
