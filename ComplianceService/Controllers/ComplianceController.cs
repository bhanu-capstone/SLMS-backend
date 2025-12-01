using ComplianceService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplianceController : ControllerBase
    {
        private readonly ComplianceEngineService _engine;
        private readonly InventoryApiClient _inventory;

        public ComplianceController(ComplianceEngineService engine, InventoryApiClient inventory)
        {
            _engine = engine;
            _inventory = inventory;
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run()
        {
            return Ok(await _engine.RunAsync());
        }

        [HttpGet("events")]
        public async Task<IActionResult> Events()
        {
            return Ok(await _engine.GetEventsAsync());
        }

        /// <summary>
        /// Returns trimmed compliance events with time-left added.
        /// NotificationService will consume this endpoint.
        /// </summary>
        [HttpGet]
        //public async Task<IActionResult> GetTrimmedEvents()
        //{
        //    var events = await _engine.GetEventsAsync();
        //    var results = new List<object>();

        //    foreach (var ev in events)
        //    {
        //        // Get the license to read expiry date
        //        var lic = await _inventory.GetLicenseById(ev.LicenseId); // helper in engine

        //        results.Add(new
        //        {
        //            ev.Id,
        //            ev.LicenseId,
        //            ev.ProductName,
        //            ev.EventType,
        //            ev.Severity,
        //            ev.Details,
        //            ev.CreatedAt,
        //            ExpiryDate = lic?.ExpiryDate   // directly expose expiry date
        //        });
        //    }
        //    return Ok(results);
        //}

        [HttpGet("alerts")]
        public IActionResult GetTrimmedEvents()
        {
            var results = new[]
            {
        new {
            id = 8,
            licenseId = 3,
            productName = "Adobe Flash",
            eventType = "expiry",
            severity = "high",
            details = "Expires in -0 days",
            createdAt = "2025-12-01T09:27:48.2738747",
            resolved = false,
            resolutionNote = (string?)null
        },
        new {
            id = 7,
            licenseId = 3,
            productName = "Adobe Flash",
            eventType = "underuse",
            severity = "low",
            details = "Assigned=0, Total=10",
            createdAt = "2025-12-01T09:27:48.2714087",
            resolved = false,
            resolutionNote = (string?)null
        },
        new {
            id = 6,
            licenseId = 2,
            productName = "Microsoft Word",
            eventType = "unused",
            severity = "low",
            details = "No installations found for this license",
            createdAt = "2025-12-01T09:27:48.2689155",
            resolved = false,
            resolutionNote = (string?)null
        },
        new {
            id = 5,
            licenseId = 2,
            productName = "Microsoft Word",
            eventType = "expiry",
            severity = "high",
            details = "Expires in -0 days",
            createdAt = "2025-12-01T09:27:48.2656841",
            resolved = false,
            resolutionNote = (string?)null
        },
        new {
            id = 4,
            licenseId = 2,
            productName = "Microsoft Word",
            eventType = "underuse",
            severity = "low",
            details = "Assigned=0, Total=100",
            createdAt = "2025-12-01T09:27:48.2631084",
            resolved = false,
            resolutionNote = (string?)null
        },
        new {
            id = 3,
            licenseId = 1,
            productName = "Microsoft Office",
            eventType = "unused",
            severity = "low",
            details = "No installations found for this license",
            createdAt = "2025-12-01T09:27:48.2602439",
            resolved = false,
            resolutionNote = (string?)null
        },
        new {
            id = 2,
            licenseId = 1,
            productName = "Microsoft Office",
            eventType = "expiry",
            severity = "high",
            details = "Expires in -0 days",
            createdAt = "2025-12-01T09:27:48.2546737",
            resolved = false,
            resolutionNote = (string?)null
        },
        new {
            id = 1,
            licenseId = 1,
            productName = "Microsoft Office",
            eventType = "underuse",
            severity = "low",
            details = "Assigned=0, Total=100",
            createdAt = "2025-12-01T09:27:48.0534151",
            resolved = false,
            resolutionNote = (string?)null
        }
    };

            return Ok(results);
        }


    }
}
