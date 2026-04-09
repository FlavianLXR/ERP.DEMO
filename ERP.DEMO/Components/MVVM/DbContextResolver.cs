using ERP.DEMO.Models.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace ERP.DEMO.Components.MVVM
{
    public interface IDbContextResolver
    {
        DbContext Resolve<T>() where T : class;
    }

    public class DbContextResolver : IDbContextResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public DbContextResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DbContext Resolve<T>() where T : class
        {
            // Résolution basée sur le type
            //if (typeof(T) == typeof(BillingDbContext))
            //    return _serviceProvider.GetService<BillingDbContext>();
            //if (typeof(T) == typeof(CarrierDbContext))
            //    return _serviceProvider.GetService<CarrierDbContext>();
            if (typeof(T) == typeof(TestDbContext))
                return _serviceProvider.GetService<TestDbContext>();
            //if (typeof(T) == typeof(ReportingDbContext))
            //    return _serviceProvider.GetService<ReportingDbContext>();
            //if (typeof(T) == typeof(SpeedPivotDbContext))
            //    return _serviceProvider.GetService<SpeedPivotDbContext>();
            //if (typeof(T) == typeof(SpeedPrdDbContext))
            //    return _serviceProvider.GetService<SpeedPrdDbContext>();

            throw new InvalidOperationException($"No DbContext found for type {typeof(T).Name}");
        }
    }

}
