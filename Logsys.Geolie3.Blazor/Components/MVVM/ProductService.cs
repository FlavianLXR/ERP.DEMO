using Azure.Core;
using ERP.DEMO.Components.MVVM;
using ERP.DEMO.Components.Tools.DataGrid;
using ERP.DEMO.Models.DataAccessLayer;
using ERP.DEMO.Models.TestDb;
using ERP.DEMO.Toolkit.Extensions;
using ERP.DEMO.ViewModels;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using static ERP.DEMO.Components.MVVM.BaseService;

namespace ERP.DEMO.Components.MVVM
{
    public class ProductService : DataGridViewModelGeneric<Product>
    {
        private readonly UserService _userService;
        public ProductService(IDbContextFactory<TestDbContext> testService, LoggerService logger, UserService userService)
            : base(testService, logger)
        {
            _userService = userService;
            Title = "Articles";
        }

        //public override void LoadData(GridState<Product> request)
        //{
        //    try
        //    {
        //        var currentUser = _userService.GetUser();

        //        using var db = CreateDb();

        //        var query = db.Products.AsQueryable();
        //        //.Where(x => x.Product.CreationDate >= DateTime.Now.AddDays(-31));

        //        // Appliquer les filtres et le tri
        //        query = query.ApplyMudFilters(request.FilterDefinitions);
        //        query = query.OrderByDynamic(request.SortDefinitions.FirstOrDefault(), x => x.CreationDate);

        //        ItemsQuery = query; // Stocker la requête filtrée
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Une erreur s'est produite lors du traitement de votre demande. \n", ex);
        //    }
        //}


        public Product? FindById(int id)
        {
            using var db = CreateDb();

            return db.Products.Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Méthode créant l'objet contenant les données à afficher sur l'index des commandes 
        /// </summary>
        /// <param name="request">Objet MudBlazor DataGridState</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override async Task<GridData<Product>> GetDataAsync(GridState<Product> gridState)
        {
            LastGridState = gridState;
            using var db = CreateDb();

            // Charger les données selon les filtres et le tri
            //LoadData(gridState);
            var query = BuildQuery(db);
            var totalItems = await query.CountAsync();

            var startIndex = (gridState.Page) * gridState.PageSize;
            query = query
                //.Include(x => x.Product)
                .Skip(startIndex)
                .Take(gridState.PageSize);

            // Obtenir le nombre total d'éléments pour la pagination

            // Obtenir les éléments de la page courante
            var items = await query.Where(x => x != null).ToListAsync();
            //var items = await TestDbContext.GetDbContext().Products.ToListAsync();

            // Retourner les résultats sous forme de GridData
            return new GridData<Product>()
            {
                Items = items,
                TotalItems = totalItems
            };
        }

        private IQueryable<Product> BuildQuery(TestDbContext db)
        {
            var query = db.Products.AsQueryable();

            if (LastGridState != null)
            {
                query = query.ApplyMudFilters(LastGridState.FilterDefinitions);
                query = query.OrderByDynamic(LastGridState.SortDefinitions.FirstOrDefault(), x => x.CreationDate);
            }

            return query;
        }

        // Dans OrderService
        public override async Task<List<Product>> GetAllForExportAsync(CancellationToken token = default, int page = -1, int pageSize = -1)
        {
            using var db = CreateDb();
            var query = BuildQuery(db);

            if (page >= 0 && pageSize > 0)
                query = query.Skip(page * pageSize).Take(pageSize);

            return await query.ToListAsync(token);
        }

        public async Task<OperationResult> EditProductAsync(int id, Dictionary<string, object?> modifiedFields)
        {
            using var db = CreateDb();
            var transaction = await db.Database.BeginTransactionAsync(); // Démarre la transaction
            try
            {
                var product = await db.Products.FindAsync(id);
                if (product == null)
                    return OperationResult.Fail(new InvalidOperationException("Produit introuvable."));

                foreach (var entry in modifiedFields)
                {
                    var prop = typeof(Product).GetProperty(entry.Key);
                    if (prop is not null && prop.CanWrite)
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        var safeValue = entry.Value == null ? null : Convert.ChangeType(entry.Value, targetType);
                        prop.SetValue(product, safeValue);
                    }
                }
                product.ModificationDate = DateTime.Now;
                product.ModifiedBy = _userService.GetUser().Id;
                db.Products.Update(product);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return OperationResult.Ok("Article modifié avec succès.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await Logger.LogError(ex);
                return OperationResult.Fail(ex);
            }
        }

        public async Task<OperationResult> CreateProductAsync(Product model, List<int>? selectedRanges = null)
        {
            using var db = CreateDb();
            var transaction = await db.Database.BeginTransactionAsync(); // Démarre la transaction
            try
            {
                //await AddModelStateRulesAsync(model, selectedRanges);

                // Vérification de l'existence du produit (même ID)
                var productExist = await db.Products.FirstOrDefaultAsync(x => x.Id == model.Id);
                if (productExist != null)
                    return OperationResult.Fail("Le code article saisi existe déjà.");

                //if (!ModelState.IsValid)
                //    return;

                model.CreationDate = model.ModificationDate = DateTime.Now;
                model.CreatedBy = _userService.GetUserId();

                db.Products.Add(model);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
                return OperationResult.Ok("Article créé avec succès.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await Logger.LogError(ex);
                return OperationResult.Fail(ex);
            }
        }


        public async Task<OperationResult> DeleteProductAsync(int id)
        {
            using var db = CreateDb();
            var transaction = await db.Database.BeginTransactionAsync(); // Démarre la transaction
            try
            {
                var model = await db.Products.FindAsync(id);
                if (model == null)
                {
                    return OperationResult.Fail("L'article n'existe pas.");
                }

                if (model.OrderLines.Count > 0)
                {
                    return OperationResult.Fail("La suppression n'est pas autorisé sur cet article.");
                }

                db.Products.Remove(model);

                await db.SaveChangesAsync();

                await transaction.CommitAsync();

                return OperationResult.Ok("L'article a été supprimé avec succès.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await Logger.LogError(ex);
                return OperationResult.Fail(ex);
            }
        }


        public async Task<List<ValidationError>> AddModelStateRulesAsync(Product model, List<int>? selectedRanges = null)
        {
            var errors = new List<ValidationError>();
            using var db = CreateDb();

            var productExist = await db.Products
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (productExist != null)
                errors.Add(new ValidationError { FieldName = nameof(model.Id), Message = "Le code article saisi existe déjà." });

            if (selectedRanges == null || selectedRanges.Count == 0)
                errors.Add(new ValidationError { Message = "Le choix d'une gamme est requis" });

            if (model.Price == null)
                errors.Add(new ValidationError { FieldName = nameof(model.Price), Message = "Le champ Prix d'achat est requis" });

            return errors;
        }

        //public override List<(string Value, string Description)> PopulateEnum(string columnName)
        //{
        //    var properties = columnName.Split('.');
        //    var parameter = Expression.Parameter(typeof(ProductRange), "x");
        //    Expression propertyAccess = parameter;

        //    foreach (var prop in properties)
        //    {
        //        propertyAccess = Expression.Property(propertyAccess, prop);
        //    }

        //    var type = Nullable.GetUnderlyingType(propertyAccess.Type) ?? propertyAccess.Type;
        //    if (!type.IsEnum)
        //    {
        //        throw new InvalidOperationException("La propriété spécifiée n'est pas un enum.");
        //    }

        //    // Convertir la valeur en Enum
        //    var convert = Expression.Convert(propertyAccess, typeof(object));
        //    var lambda = Expression.Lambda<Func<ProductRange, object>>(convert, parameter);

        //    // Récupérer et convertir les valeurs distinctes
        //    var enumValues = TestDbContext.GetQueryable<ProductRange>()
        //        .Select(lambda)
        //        .Distinct()
        //        .ToList()
        //        .Where(x => x != null)
        //        .Select(x => ((Enum)x))
        //        .Select(x => (x.ToString(), x.GetEnumDescription()))
        //        .OrderBy(x => x.Item2)
        //        .ToList();

        //    return enumValues;
        //}

        /// <summary>
        /// Méthode récupérant les données pour les charts de l'index des commandes
        /// </summary>
        /// <param name="days">Nombre de jours à afficher sur la chart</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Charts GetData(int days = 7)
        {
            using var db = CreateDb();

            var currentUser = _userService.GetUser();
            var startDate = days == 1
               ? DateTime.Now.AddDays(-1) // Dernières 24 heures
               : DateTime.Now.Date.AddDays(-days); // Derniers 7 ou 30 jours

            #region ProductChart
            var rawData = db.Products.AsQueryable()
                .Where(x => x.CreationDate >= startDate)
                .GroupBy(x => days == 1
                    ? x.CreationDate.Hour // Garde les heures pour les dernières 24h
                    : x.CreationDate.Day) // Normalise pour 7/30 jours
                .Select(g => new
                {
                    Date = g.Key,
                    Number = g.Count()
                })
                .ToList();

            var rawData_ = db.Products.AsQueryable()
                .Where(x => x.CreationDate >= (startDate.AddDays(-days)) && x.CreationDate < startDate)
                .GroupBy(x => days == 1
                    ? x.CreationDate.Hour // Garde les heures pour les dernières 24h
                    : x.CreationDate.Day) // Normalise pour 7/30 jours
                .Select(g => new
                {
                    Date = g.Key,
                    Number = g.Count()
                })
                .ToList();

            var total = rawData.Sum(x => x.Number);
            var total_ = rawData_.Sum(x => x.Number);
            int pourcentProduct = 0;
            if (total_ != 0)
            {
                pourcentProduct = (int)Math.Round((double)(total - total_) / total_ * 100);
            }

            var result = Enumerable.Range(0, (days == 1 ? 25 : days))
                .Select(i =>
                {
                    var date = days == 1
                        ? DateTime.Now.AddHours(-24).AddHours(i) // Pour 24h : heures glissantes
                        : DateTime.Now.Date.AddDays(-i); // Pour 7/30 jours : jours uniquement

                    var dataForDay = rawData.FirstOrDefault(d =>
                        days == 1
                            ? d.Date == date.Hour // Précision horaire pour 24h
                            : d.Date == date.Day); // Simple comparaison de date pour 7/30 jours

                    return new MyData
                    {
                        Date = date.ToString(days == 1 ? "dd-MM-yyyy HH:mm" : "dd-MM-yyyy"), // Format adapté
                        Number = dataForDay != null ? dataForDay.Number : 0,
                    };
                })
                .ToList();
            #endregion

            return new Charts
            {
                dataProduct = result.OrderBy(d =>
                DateTime.ParseExact(d.Date, days == 1 ? "dd-MM-yyyy HH:mm" : "dd-MM-yyyy", null)).ToList(),

                totalProduct = total,

                pourcentProduct = pourcentProduct,

            };
        }



        /// <summary>
        /// Point/data à une date donnée sur la chart
        /// </summary>
        public class MyData
        {
            public string Date { get; set; }
            public int Number { get; set; }
        }

        /// <summary>
        /// Objet contenant les charts de l'index des commandes
        /// </summary>
        public class Charts
        {
            public List<MyData> dataProduct;
            public int totalProduct;
            public float pourcentProduct;
        };

        //    public List<SelectItem<string>> Algorythms = new()
        //        {
        //            new SelectItem <string> { Value = "0", Text = "---" },
        //            new SelectItem <string> { Value = "Luhn", Text = "Luhn" },
        //            new SelectItem <string> { Value = "Increment_1", Text = "Increment_1" }
        //        };

        //    public List<SelectItem<string>> AlcoholType = new()
        //{
        //    new SelectItem<string> { Value = null, Text = "Aucun type" },
        //    new SelectItem<string> { Value = "alcool", Text = "alcool" },
        //    new SelectItem<string> { Value = "bières", Text = "bière" },
        //    new SelectItem<string> { Value = "rhum", Text = "rhum" },
        //    new SelectItem<string> { Value = "vin", Text = "vin" },
        //    new SelectItem<string> { Value = "vin mousseux", Text = "vin mousseux" }
        //};


        //public List<SelectItem<int>> PopulateProductTypeDropDownList()
        //{
        //    var items = TestDbContext.GetDbContext().ProductTypes
        //        .Where(t => t.IsActive)
        //        .OrderBy(t => t.Label)
        //        .Select(t => new SelectItem<int>
        //        {
        //            Value = t.Id,
        //            Text = t.Label
        //        })
        //        .ToList();

        //    return items;
        //}

        //public List<SelectItem<string?>> PopulateOperationDropDownList(bool includeEmpty = false)
        //{
        //    var user = _userService.GetUser();
        //    var data = TestDbContext.GetDbContext().Operations
        //        .Where(o => o.CustomerId == user.GetCustomerId() && !o.IsCompleted);

        //    if (data.Any(o => o.Users.Any(ou => ou.ClientUserId == user.Id)))
        //        data = data.Where(o => o.Users.Any(ou => ou.ClientUserId == user.Id));

        //    var items = data
        //        .OrderByDescending(o => o.Label)
        //        .Select(o => new SelectItem<string?>
        //        {
        //            Value = o.Id,
        //            Text = o.Id + " - " + o.Label
        //        })
        //        .ToList();

        //    if (includeEmpty)
        //        items.Insert(0, new SelectItem<string?> { Value = null, Text = "(Aucune opération)" });

        //    return items;
        //}

        //public static class ProductExtensions
        //{
        //    public static IQueryable<ProductRange> FilterForClientUser(
        //        this IQueryable<ProductRange> query, IUser currentUser)
        //    {
        //        if (!currentUser.IsClientUser())
        //            return query;

        //        var customerId = currentUser.GetCustomerId();
        //        return query.Where(o =>
        //        o.Range.Stock.CustomerId == currentUser.GetCustomerId() 
        //        /*&& pr.Range.Stock.ClientUsers.Any(cu => cu.ClientUserId == currentUser.GetCustomerId())*/
        //        );
        //    }
        //}
    }
}