using ERP.DEMO.Components.Tools.DataGrid;
using ERP.DEMO.Models.DataAccessLayer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace ERP.DEMO.Components.MVVM
{
    public abstract class BaseService
    {
        protected readonly IDbContextFactory<TestDbContext> TestDbContext;

        //protected readonly GenericService<TestDbContext> TestDbContext;
        protected readonly LoggerService Logger;

        protected BaseService(IDbContextFactory<TestDbContext> testDbContext, LoggerService logger)
        {
            this.TestDbContext = testDbContext;
            this.Logger = logger;
        }

        protected TestDbContext CreateDb()
       => TestDbContext.CreateDbContext();

        public class SelectItem<T>
        {
            public T Value { get; set; }
            public string Text { get; set; }
            //bool Selected { get; set; } = false;

        }

        public class ValidationError
        {
            public string? FieldName { get; set; } // null = erreur globale
            public string Message { get; set; } = string.Empty;
        }

    }
}



//private async Task PopulateOperationDropDownList()
//{
//    // Simulez une requête pour récupérer les données
//    var data = db.Operations.Where(o => o.CustomerId == UserInfo.CustomerId && o.IsCompleted == false);

//    if (data.Any(o => o.Users.Any(ou => ou.ClientUserId == UserInfo.Id)))
//        data = data.Where(o => o.Users.Any(ou => ou.ClientUserId == UserInfo.Id));

//    Operations = data
//        .OrderByDescending(o => o.Label)
//        .Select(o => new OperationDto { Id = o.Id, IdAndLabel = $"{o.Id} - {o.Label}" })
//        .ToList();
//}
