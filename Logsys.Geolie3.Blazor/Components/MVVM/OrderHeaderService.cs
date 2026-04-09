
using Azure.Core;
using ERP.DEMO.Components.Tools.DataGrid;
using ERP.DEMO.Models.DataAccessLayer;
using ERP.DEMO.Models.TestDb;
using ERP.DEMO.Toolkit.Extensions;
using ERP.DEMO.ViewModels;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ERP.DEMO.Components.MVVM
{
    public class OrderService : DataGridViewModelGeneric<Order>
    {
        private readonly UserService _userService;

        public OrderService(IDbContextFactory<TestDbContext> testService, LoggerService logger, UserService userService)
            : base(testService, logger)
        {
            _userService = userService;
            Title = "Commandes";
        }

        //public override void LoadData(GridState<Order> request)
        //{
        //    try
        //    {
        //        var currentUser = _userService.GetUser();

        //        using var db = CreateDb();
        //        var query = db.Orders.AsQueryable();
        //        //.Where(x => x.CreationDate >= DateTime.Now.AddDays(-31));

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

        public Order FindById(int id)
        {
            using var db = CreateDb();
            return db.Orders.Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Méthode créant l'objet contenant les données à afficher sur l'index des commandes 
        /// </summary>
        /// <param name="request">Objet MudBlazor DataGridState</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// public override async Task<GridData<Order>> GetDataAsync(GridState<Order> request)

        public override async Task<GridData<Order>> GetDataAsync(GridState<Order> gridState)
        {
            LastGridState = gridState;
            var stopwatch = Stopwatch.StartNew(); // Démarre le chrono

            // Charger les données selon les filtres et le tri
            //LoadData(gridState);
            using var db = CreateDb();
            var query = BuildQuery(db);

            // Obtenir le nombre total d'éléments pour la pagination
            var totalItems = await query.CountAsync();

            var startIndex = (gridState.Page) * gridState.PageSize;
             query = query
                .Skip(startIndex)
                .Take(gridState.PageSize);

            // Obtenir les éléments de la page courante
            var items = await query.ToListAsync();


            stopwatch.Stop(); // Arrête le chrono
            Debug.WriteLine($"Temps de chargement : {stopwatch.ElapsedMilliseconds} ms"); // Affiche le temps

            // Retourner les résultats sous forme de GridData
            return new GridData<Order>()
            {
                Items = items,
                TotalItems = totalItems
            };
        }

        private IQueryable<Order> BuildQuery(TestDbContext db)
        {
            var query = db.Orders.AsQueryable();

            if (LastGridState != null)
            {
                query = query.ApplyMudFilters(LastGridState.FilterDefinitions);
                query = query.OrderByDynamic(LastGridState.SortDefinitions.FirstOrDefault(), x => x.CreationDate);
            }

            return query;
        }

        // Dans OrderService
        public override async Task<List<Order>> GetAllForExportAsync(CancellationToken token = default, int page = -1, int pageSize = -1)
        {
            using var db = CreateDb();
            var query = BuildQuery(db);

            if (page >= 0 && pageSize > 0)
                query = query.Skip(page * pageSize).Take(pageSize);

            return await query.ToListAsync(token);
        }

        public override List<(string Value, string Description)> PopulateEnum(string columnName)
        {
            using var db = CreateDb();

            var properties = columnName.Split('.');
            var parameter = Expression.Parameter(typeof(Order), "x");
            Expression propertyAccess = parameter;

            foreach (var prop in properties)
            {
                propertyAccess = Expression.Property(propertyAccess, prop);
            }

            var type = Nullable.GetUnderlyingType(propertyAccess.Type) ?? propertyAccess.Type;
            if (!type.IsEnum)
            {
                throw new InvalidOperationException("La propriété spécifiée n'est pas un enum.");
            }

            // Convertir la valeur en Enum
            var convert = Expression.Convert(propertyAccess, typeof(object));
            var lambda = Expression.Lambda<Func<Order, object>>(convert, parameter);

            // Récupérer et convertir les valeurs distinctes
            var enumValues = db.Orders
                .Select(lambda)
                .Distinct()
                .ToList()
                .Where(x => x != null)
                .Select(x => ((Enum)x))
                .Select(x => (x.ToString(), x.GetEnumDescription()))
                .OrderBy(x => x.Item2)
                .ToList();

            return enumValues;
        }



        /// <summary>
        /// Méthode définissant les colonnes de l'index
        /// </summary>
        //public override void SetColumns()
        //{
        //    //Columns = GenerateColumns<Order>();
        //    Columns = new()
        //    {
        //        //new (x => x.Id, "Id", hidden: true),
        //        new (x => x.OrderId, "OrderId", draggable: false, filterable: false),
        //        new (x => x.RecipientId, "Référence"),
        //        new (x => x.Order.Operation.Label, "Opération"),
        //        new (x => x.NameOne, "Nom1"),
        //        new (x => x.NameTwo, "Nom2", hiddenDefault: true),
        //        new (x => x.Order.Status, "Status",
        //            cellTemplate: (context) => builder =>
        //            {
        //                builder.OpenComponent(0, typeof(StatusString)); // Créez le composant
        //                builder.AddAttribute(1, "Status", context.Order.Status.GetValueOrDefault());
        //                builder.CloseComponent();
        //            }
        //        ),
        //        new (x => x.Order.Origin.Label, "Origin"),
        //        new (x => x.CreationDate, "Créé le",
        //            cellTemplate: (context) => builder =>
        //            {
        //                builder.AddMarkupContent(0, $"{context.CreationDate?.ToShortDateString()}");
        //            }
        //        ),
        //        new (x => x.PhoneNumber, "Mobile", hiddenDefault: true),
        //        new (x => x.ZipCode, "Code postal", hiddenDefault: true),
        //        new (x => x.Country, "Pays", hiddenDefault: true),
        //        new (x => x.City, "Ville", hiddenDefault: true),
        //        new (x => x.LabelSix, "Label6", hiddenDefault: true),
        //        new (x => x.LabelFive, "Label5"),
        //        new (x => x.LabelFour, "Label4", hiddenDefault: true),
        //        new (x => x.LabelThree, "Label3", hiddenDefault: true),
        //        new (x => x.LabelTwo, "Label2", hiddenDefault: true),
        //        new (x => x.LabelOne, "Label1", hiddenDefault: true),
        //        new (x => x.Order.Description, "Description", hiddenDefault: true)
        //    };
        //}

        //private List<ColumnDefinition<T>> GenerateColumns<T>()
        //{
        //    var columns = new List<ColumnDefinition<T>>();
        //    var properties = typeof(OrderIndexLineViewModel).GetProperties();

        //    foreach (var prop in properties)
        //    {
        //        var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
        //        var columnAttr = prop.GetCustomAttribute<ColumnSettingsAttribute>();

        //        string title = displayAttr?.Name ?? prop.Name;
        //        bool hidden = columnAttr?.Hidden ?? false;
        //        bool filterable = columnAttr?.Filterable ?? true;
        //        bool hideable = columnAttr?.Hideable ?? true;
        //        bool hiddenDefault = columnAttr?.HiddenDefault ?? false;
        //        bool draggable = columnAttr?.Draggable ?? true;

        //        // Créer une instance de OrderIndexLineViewModel pour obtenir l'expression
        //        var modelInstance = Activator.CreateInstance(typeof(OrderIndexLineViewModel));

        //        var propertyExpression = prop.GetValue(modelInstance) as LambdaExpression;

        //        if (propertyExpression != null)
        //        {
        //            // Convertir l'expression à une Expression<Func<T, object>>
        //            var convertedExpression = Expression.Lambda<Func<T, object>>(
        //                Expression.Convert(propertyExpression.Body, typeof(object)),
        //                propertyExpression.Parameters
        //            );

        //            columns.Add(new ColumnDefinition<T>(
        //                property: convertedExpression,
        //                title: title,
        //                hidden: hidden,
        //                filterable: filterable,
        //                hideable: hideable,
        //                hiddenDefault: hiddenDefault,
        //                draggable: draggable
        //            ));
        //        }


        //    }
        //    return columns;
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

            #region OrderChart

            var rawData = db.Orders
                .Where(x => x.CreationDate >= startDate)
                .GroupBy(x => days == 1
                    ? x.CreationDate.Hour // Garde les heures pour les dernières 24h
                    : x.CreationDate.Date.Day) // Normalise pour 7/30 jours
                .Select(g => new
                {
                    Date = g.Key,
                    Number = g.Count()
                })
                .ToList();

            var rawData_ = db.Orders
                .Where(x => x.CreationDate >= (startDate.AddDays(-days)) && x.CreationDate < startDate)
                .GroupBy(x => days == 1
                    ? x.CreationDate.Hour // Garde les heures pour les dernières 24h
                    : x.CreationDate.Date.Day) // Normalise pour 7/30 jours
                .Select(g => new
                {
                    Date = g.Key,
                    Number = g.Count()
                })
                .ToList();

            var total = rawData.Sum(x => x.Number);
            var total_ = rawData_.Sum(x => x.Number);
            int pourcentOrder = 0;
            if (total_ != 0)
            {
                pourcentOrder = (int)Math.Round((double)(total - total_) / total_ * 100);
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

            #region ProductChart
            var rawData2 = db.Orders
                .Where(order => order.CreationDate >= startDate) // Filtrer par la plage de temps
                .SelectMany(order => order.OrderLines, (order, line) => new { order.CreationDate, Line = line })
                .GroupBy(x => days == 1
                    ? x.CreationDate.Hour // Garde les heures pour les dernières 24h
                    : x.CreationDate.Date.Day)
                .Select(group => new
                {
                    Date = group.Key,
                    TotalOrderLines = group.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            var rawData2_ = db.Orders
                .Where(order => order.CreationDate >= (startDate.AddDays(-days)) && order.CreationDate < startDate) // Filtrer par la plage de temps
                .SelectMany(order => order.OrderLines, (order, line) => new { order.CreationDate, Line = line })
                .GroupBy(x => days == 1
                    ? x.CreationDate.Hour // Garde les heures pour les dernières 24h
                    : x.CreationDate.Date.Day)
                .Select(group => new
                {
                    Date = group.Key,
                    TotalOrderLines = group.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            var total2 = rawData2.Sum(x => x.TotalOrderLines);
            var total2_ = rawData2_.Sum(x => x.TotalOrderLines);
            int pourcentProduct = 0;
            if (total2_ != 0)
            {
                pourcentProduct = (int)Math.Round((double)(total2 - total2_) / total2_ * 100);
            }


            var result2 = Enumerable.Range(0, (days == 1 ? 25 : days))
                .Select(i =>
                {
                    var date = days == 1
                        ? DateTime.Now.AddHours(-24).AddHours(i) // Pour 24h : heures glissantes
                        : DateTime.Now.Date.AddDays(-i); // Pour 7/30 jours : jours uniquement

                    var dataForDay = rawData2.FirstOrDefault(d =>
                        days == 1
                            ? d.Date == date.Hour// Précision horaire pour 24h
                            : d.Date == date.Day); // Simple comparaison de date pour 7/30 jours

                    return new MyData
                    {
                        Date = date.ToString(days == 1 ? "dd-MM-yyyy HH:mm" : "dd-MM-yyyy"), // Format adapté
                        Number = dataForDay != null ? dataForDay.TotalOrderLines : 0
                    };
                })
                .ToList();

            #endregion

            #region DeliverChart

            var rawData3 = db.Orders
               .Where(x => x.CreationDate >= startDate/* && x.Status == "Livré conforme"*/)
               .GroupBy(x => days == 1
                   ? x.CreationDate.Hour // Garde les heures pour les dernières 24h
                   : x.CreationDate.Date.Day) // Normalise pour 7/30 jours
               .Select(g => new
               {
                   Date = g.Key,
                   Number = g.Count()
               })
               .ToList();
            var rawData3_ = db.Orders
                .Where(x => x.CreationDate >= (startDate.AddDays(-days)) && x.CreationDate < startDate /*&& x.Status == "Livré conforme"*/)
                .GroupBy(x => days == 1
                    ? x.CreationDate.Hour // Garde les heures pour les dernières 24h
                    : x.CreationDate.Date.Day) // Normalise pour 7/30 jours
                .Select(g => new
                {
                    Date = g.Key,
                    Number = g.Count()
                })
                .ToList();

            var total3 = rawData3.Sum(x => x.Number);
            var total3_ = rawData3_.Sum(x => x.Number);
            int pourcentDeliver = 0;
            if (total3_ != 0)
            {
                pourcentDeliver = (int)Math.Round((double)(total3 - total3_) / total3_ * 100);
            }

            var result3 = Enumerable.Range(0, (days == 1 ? 25 : days))
                .Select(i =>
                {
                    var date = days == 1
                        ? DateTime.Now.AddHours(-24).AddHours(i) // Pour 24h : heures glissantes
                        : DateTime.Now.Date.AddDays(-i); // Pour 7/30 jours : jours uniquement

                    var dataForDay = rawData3.FirstOrDefault(d =>
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
                dataOrder = result.OrderBy(d =>
                DateTime.ParseExact(d.Date, days == 1 ? "dd-MM-yyyy HH:mm" : "dd-MM-yyyy", null)).ToList(),
                dataProduct = result2.OrderBy(d =>
                DateTime.ParseExact(d.Date, days == 1 ? "dd-MM-yyyy HH:mm" : "dd-MM-yyyy", null)).ToList(),
                dataDeliver = result3.OrderBy(d =>
                DateTime.ParseExact(d.Date, days == 1 ? "dd-MM-yyyy HH:mm" : "dd-MM-yyyy", null)).ToList(),
                totalOrder = total,
                totalProduct = total2,
                totalDeliver = total3,
                pourcentOrder = pourcentOrder,
                pourcentProduct = pourcentProduct,
                pourcentDeliver = pourcentDeliver
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
            public List<MyData> dataOrder;
            public List<MyData> dataProduct;
            public List<MyData> dataDeliver;
            public int totalOrder;
            public int totalProduct;
            public int totalDeliver;
            public float pourcentOrder;
            public float pourcentProduct;
            public float pourcentDeliver;
        }
    }
}