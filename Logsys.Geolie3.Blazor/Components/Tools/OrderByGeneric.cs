using MudBlazor;
using MudBlazor.Charts;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ERP.DEMO.Components.Tools
{
    public static class OrderByGeneric
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">IQueryable de la table à sort</param>
        /// <param name="sortDefinition">Objet provenant de MudDataGrid contenant les colonnes à sort</param>
        /// <param name="defaultSortExpression">Expression lambda de la colonne à sort par défaut</param>
        /// <returns></returns>
        public static IQueryable<T> OrderByDynamic<T>(
        this IQueryable<T> source,
        SortDefinition<T> sortDefinition,
        Expression<Func<T, object>> defaultSortExpression = null)
        {
            if (sortDefinition == null || string.IsNullOrWhiteSpace(sortDefinition.SortBy))
            {
                // Si aucune définition de tri, appliquer le tri par défaut via l'expression lambda
                return ApplyDefaultSort(source, defaultSortExpression ?? (x => x));
            }

            var propertyName = sortDefinition.SortBy;
            var descending = sortDefinition.Descending;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression property = parameter;

            foreach (var member in propertyName.Split('.'))
            {
                property = Expression.PropertyOrField(property, member);
            }

            var lambda = Expression.Lambda(property, parameter);
            var methodName = descending ? "OrderByDescending" : "OrderBy";
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type);

            return (IQueryable<T>)method.Invoke(null, new object[] { source, lambda });
        }

        private static IQueryable<T> ApplyDefaultSort<T>(IQueryable<T> source, Expression<Func<T, object>> defaultSortExpression)
        {
            if (defaultSortExpression == null)
                return source;

            var methodName = "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), typeof(object));

            return (IQueryable<T>)method.Invoke(null, new object[] { source, defaultSortExpression });
        }

    }
}
