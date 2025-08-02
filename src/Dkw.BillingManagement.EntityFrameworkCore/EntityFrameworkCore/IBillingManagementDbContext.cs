using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Dkw.BillingManagement.EntityFrameworkCore;

[ConnectionStringName(BillingManagementDbProperties.ConnectionStringName)]
public interface IBillingManagementDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
