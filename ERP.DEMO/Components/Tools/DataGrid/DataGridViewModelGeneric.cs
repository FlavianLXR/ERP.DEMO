using ERP.DEMO.Components.MVVM;
using ERP.DEMO.Models.DataAccessLayer;
using ERP.DEMO.ViewModels;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using ERP.DEMO.Models.TestDb;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace ERP.DEMO.Components.Tools.DataGrid
{
    public abstract class DataGridViewModelGeneric<TModel> : BaseService
    {
        public DataGridViewModelGeneric(IDbContextFactory<TestDbContext> testService, LoggerService logger)
            : base(testService, logger)
        {
            // db et dbTrans sont déjà accessibles ici
        }
        public string Title { get; set; } = "Données";
        public List<ColumnDefinition<TModel>> Columns { get; set; } = new();
        protected GridState<TModel>? LastGridState { get; set; } // ← stocke l'état


        //public virtual void LoadData(GridState<TModel> request)
        //{
        //    throw new NotImplementedException("Cette méthode doit être surchargée dans la classe dérivée.");
        //}

        public virtual Task<GridData<TModel>> GetDataAsync(GridState<TModel> request)
        {
            throw new NotImplementedException("Cette méthode doit être surchargée dans la classe dérivée.");
        }
        public virtual void SetColumns()
        {

        }

        // Dans DataGridViewModelGeneric
        public virtual Task<List<TModel>> GetAllForExportAsync(CancellationToken token = default, int page = -1, int pageSize = -1)
        {
            throw new NotImplementedException();
        }

        public virtual List<(string Value, string Description)> PopulateEnum(string columnName)
        {
            throw new NotImplementedException("Cette méthode doit être surchargée dans la classe dérivée.");
        }
    }
}
