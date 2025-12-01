using ComplianceService.Models;
using ComplianceService.Repositories;

namespace ComplianceService.Services
{
    public class ComplianceEngineService
    {
        private readonly InventoryApiClient _inventory;
        private readonly IComplianceEventRepository _events;
        private readonly MatchingService _matcher;

        public ComplianceEngineService(
            InventoryApiClient inventory,
            IComplianceEventRepository events,
            MatchingService matcher)
        {
            _inventory = inventory;
            _events = events;
            _matcher = matcher;
        }

        public async Task<List<ComplianceEvent>> RunAsync()
        {
            var results = new List<ComplianceEvent>();

            var licenses = await _inventory.GetLicensesAsync();
            var installs = await _inventory.GetInstalledAsync();
            var ents = await _inventory.GetEntitlementsAsync();

            foreach (var lic in licenses)
            {
                int used = _matcher.GetUsageCount(lic, installs);

                // ============================
                //       1) OVERUSE
                // ============================
                if (used > lic.TotalEntitlements)
                {
                    var existing = await _events.GetExistingOpenEventAsync(lic.LicenseId, "overuse");

                    if (existing == null)
                    {
                        var evt = new ComplianceEvent
                        {
                            LicenseId = lic.LicenseId,
                            ProductName = lic.ProductName,
                            EventType = "overuse",
                            Severity = "high",
                            Details = $"Used={used}, Entitlements={lic.TotalEntitlements}"
                        };

                        await _events.AddAsync(evt);
                        results.Add(evt);
                    }
                }

                // ============================
                //       2) UNDERUSE < 25%
                // ============================
                if (lic.TotalEntitlements > 0)
                {
                    double utilization = (double)lic.Assigned / lic.TotalEntitlements;

                    if (utilization < 0.25)
                    {
                        var existing = await _events.GetExistingOpenEventAsync(lic.LicenseId, "underuse");

                        if (existing == null)
                        {
                            var evt = new ComplianceEvent
                            {
                                LicenseId = lic.LicenseId,
                                ProductName = lic.ProductName,
                                EventType = "underuse",
                                Severity = "low",
                                Details = $"Assigned={lic.Assigned}, Total={lic.TotalEntitlements}"
                            };

                            await _events.AddAsync(evt);
                            results.Add(evt);
                        }
                    }
                }

                // ============================
                //       3) EXPIRY RULE
                // ============================
                if (lic.ExpiryDate.HasValue)
                {
                    double days = (lic.ExpiryDate.Value - DateTime.UtcNow).TotalDays;

                    if (days <= 30)
                    {
                        string severity = days <= 7 ? "high" : "medium";

                        var existing = await _events.GetExistingOpenEventAsync(lic.LicenseId, "expiry");

                        if (existing == null)
                        {
                            var evt = new ComplianceEvent
                            {
                                LicenseId = lic.LicenseId,
                                ProductName = lic.ProductName,
                                EventType = "expiry",
                                Severity = severity,
                                Details = $"Expires in {days:F0} days"
                            };

                            await _events.AddAsync(evt);
                            results.Add(evt);
                        }
                    }
                }

                // ============================
                //       4) UNUSED LICENSE
                // ============================
                if (used == 0)
                {
                    var existing = await _events.GetExistingOpenEventAsync(lic.LicenseId, "unused");

                    if (existing == null)
                    {
                        var evt = new ComplianceEvent
                        {
                            LicenseId = lic.LicenseId,
                            ProductName = lic.ProductName,
                            EventType = "unused",
                            Severity = "low",
                            Details = "No installations found for this license"
                        };

                        await _events.AddAsync(evt);
                        results.Add(evt);
                    }
                }

                // ============================
                //       5) MISMATCH RULE
                // ============================
                var mismatches = installs.Where(i =>
                    i.LicenseId != lic.LicenseId &&
                    i.ProductName.Contains(lic.ProductName, StringComparison.OrdinalIgnoreCase)
                );

                foreach (var m in mismatches)
                {
                    var existing = await _events.GetExistingOpenEventAsync(lic.LicenseId, "mismatch");

                    if (existing == null)
                    {
                        var evt = new ComplianceEvent
                        {
                            LicenseId = lic.LicenseId,
                            ProductName = lic.ProductName,
                            EventType = "mismatch",
                            Severity = "high",
                            Details = $"Unlicensed installation on Device {m.DeviceId}"
                        };

                        await _events.AddAsync(evt);
                        results.Add(evt);
                    }
                }
            }

            return results;
        }

        public async Task<IEnumerable<ComplianceEvent>> GetEventsAsync()
        {
            return await _events.GetAllAsync();
        }


    }
}
