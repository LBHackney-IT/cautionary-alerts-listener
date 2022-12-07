using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CautionaryAlertsListener.Infrastructure
{
    public class CautionaryAlertContext : DbContext
    {
        public CautionaryAlertContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<PropertyAlertNew> PropertyAlerts { get; set; }
    }
}
