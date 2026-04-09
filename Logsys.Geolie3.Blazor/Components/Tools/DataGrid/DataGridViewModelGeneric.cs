using ERP.DEMO.Components.MVVM;
using ERP.DEMO.Models.DataAccessLayer;
using ERP.DEMO.ViewModels;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using ERP.DEMO.Models.TestDb;
using System.Linq.Expressions;
using System.Reflection;

namespace ERP.DEMO.Components.Tools.DataGrid
{
    public abstract class DataGridViewModelGeneric<TModel> : BaseService
    {
        public DataGridViewModelGeneric(GenericService<TestDbContext> testService, LoggerService logger)
            : base(testService, logger)
        {
            // db et dbTrans sont déjà accessibles ici
        }
        public string Title { get; set; } = "Données";
        public List<ColumnDefinition<TModel>> Columns { get; set; } = new();

        protected IQueryable<TModel> ItemsQuery { get; set; } = Enumerable.Empty<TModel>().AsQueryable();

        public virtual void LoadData(GridState<TModel> request)
        {
            throw new NotImplementedException("Cette méthode doit être surchargée dans la classe dérivée.");
        }

        public virtual Task<GridData<TModel>> GetDataAsync(GridState<TModel> request)
        {
            throw new NotImplementedException("Cette méthode doit être surchargée dans la classe dérivée.");
        }
        public virtual void SetColumns()
        {

        }

        public IQueryable<TModel> GetItemsQuery() { return ItemsQuery; }

        public virtual List<(string Value, string Description)> PopulateEnum(string columnName)
        {
            throw new NotImplementedException("Cette méthode doit être surchargée dans la classe dérivée.");
        }
    }
}
