using Microsoft.EntityFrameworkCore;
using RenewalService.Models;
using System.Collections.Generic;

namespace RenewalService.Data
{
    public class RenewalDbContext:DbContext
    {
        public RenewalDbContext(DbContextOptions<RenewalDbContext> options) : base(options) { }
        public DbSet<RenewalRecord> Renewals { get; set; }
    }
}
