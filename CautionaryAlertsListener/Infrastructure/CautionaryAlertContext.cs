using Microsoft.EntityFrameworkCore;

namespace CautionaryAlertsListener.Infrastructure
{
    public class CautionaryAlertContext : DbContext
    {
        public CautionaryAlertContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<PropertyAlert> PropertyAlerts { get; set; }
    }
}
