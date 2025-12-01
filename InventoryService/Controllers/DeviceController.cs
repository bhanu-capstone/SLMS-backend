using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _service;

        public DevicesController(IDeviceService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Admin, Administrator")]
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var device = await _service.GetByIdAsync(id);
            return device == null ? NotFound() : Ok(device);
        }

        [HttpGet("by-device-id/{deviceId}")]
        public async Task<IActionResult> GetByDeviceId(string deviceId)
        {
            var device = await _service.GetByDeviceIdAsync(deviceId);
            return device == null ? NotFound() : Ok(device);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Device device)
        {
            var created = await _service.CreateAsync(device);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Device update)
        {
            try
            {
                return Ok(await _service.UpdateAsync(id, update));
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

        [HttpGet("{id}/installed-software")]
        public async Task<IActionResult> GetInstalled(int id)
        {
            var list = await _service.GetInstalledSoftwareAsync(id);
            return Ok(list);
        }
    }
}
