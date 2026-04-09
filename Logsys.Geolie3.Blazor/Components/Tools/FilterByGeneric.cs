using Microsoft.Extensions.Primitives;
using MudBlazor;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Metadata;

public static class FilterByGeneric
{
    /// <summary>
    /// Applique les filtres MudBlazor sur une requête IQueryable.
    /// </summary>
    /// <typeparam name="T">Type des objets de la requête.</typeparam>
    /// <param name="query">Requête à filtrer.</param>
    /// <param name="filters">Liste des filtres à appliquer.</param>
    /// <returns>Requête filtrée.</returns>
    public static IQueryable<T> ApplyMudFilters<T>(this IQueryable<T> query, ICollection<IFilterDefinition<T>> filters)
    {
        if (filters == null || !filters.Any()) return query;

        // Grouping filters by PropertyName (Column)
        var groupedFilters = filters.GroupBy(f => f.Column?.PropertyName)
                                    .Where(g => g.Key != null);

        Expression combinedExpressionGlobal = null; // Expression globale pour la recherche
        List<Expression> globalConditions = new List<Expression>();
        var parameter = Expression.Parameter(typeof(T), "x");

        foreach (var group in groupedFilters)
        {
            var propertyName = group.Key;
            var property = GetPropertyExpression(parameter, propertyName);
            var propertyType = property.Type;

            Expression combinedExpression = null;
            bool isDateColumn = propertyType == typeof(DateTime) || propertyType == typeof(DateTime?);
            bool isIntColumn = propertyType == typeof(int) || propertyType == typeof(int?);

            foreach (var filter in group)
            {
                if (filter.Value == null || filter.Operator == null) continue;

                Expression valueExpression = Expression.Constant(filter.Value);
                Expression comparison = ApplyOperator(property, valueExpression, propertyType, filter.Operator);

                if (comparison != null)
                {
                    // Vérifie si c'est la recherche globale
                    if (filter.Title == "Recherche globale" && propertyType == typeof(string))
                    {
                        Expression searchValue = Expression.Constant(filter.Value);
                        Expression globalSearch = ApplyOperator(property, searchValue, typeof(string), "contains");

                        if (globalSearch != null)
                            globalConditions.Add(globalSearch);
                    }
                    else
                    {
                        // Pour les dates et int (possiblement range) -> AND, sinon -> OR
                        if (isDateColumn || isIntColumn)
                            combinedExpression = combinedExpression == null ? comparison : Expression.AndAlso(combinedExpression, comparison);
                        else
                            combinedExpression = combinedExpression == null ? comparison : Expression.OrElse(combinedExpression, comparison);
                    }
                }
            }

            if (combinedExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
                query = query.Where(lambda);
            }
        }

        // Applique la recherche globale si elle existe
        if (globalConditions.Any())
        {
            combinedExpressionGlobal = globalConditions.Aggregate(Expression.OrElse);
            var globalLambda = Expression.Lambda<Func<T, bool>>(combinedExpressionGlobal, parameter);
            query = query.Where(globalLambda);
        }

        return query;

    }

    /// <summary>
    /// Récupère l'expression d'une propriété, y compris les propriétés imbriquées.
    /// </summary>
    /// <param name="parameter">Paramètre d'expression représentant l'entité.</param>
    /// <param name="propertyName">Nom de la propriété (ex. "User.Address.City").</param>
    /// <returns>Expression représentant la propriété.</returns>
    private static Expression GetPropertyExpression(Expression parameter, string propertyName)
    {
        var propertyNames = propertyName.Split('.'); // Handle nested properties
        Expression property = parameter;

        foreach (var prop in propertyNames)
        {
            property = Expression.PropertyOrField(property, prop);
        }

        return property;
    }

    /// <summary>
    /// Applique l'opérateur de comparaison entre une propriété et une valeur donnée.
    /// </summary>
    /// <param name="property">Expression représentant la propriété.</param>
    /// <param name="valueExpression">Expression représentant la valeur à comparer.</param>
    /// <param name="propertyType">Type de la propriété.</param>
    /// <param name="op">Opérateur de comparaison.</param>
    /// <returns>Expression de comparaison.</returns>
    private static Expression ApplyOperator(Expression property, Expression valueExpression, Type propertyType, string op)
    {
        // Handle Nullable Types (e.g. DateTime?, int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType);
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && valueExpression.Type == underlyingType)
        {
            if (valueExpression.Type != propertyType)
            {
                valueExpression = Expression.Convert(valueExpression, propertyType);
            }

            var nullCheck = Expression.AndAlso(
                Expression.Property(property, "HasValue"),
                Expression.Property(valueExpression, "HasValue")
            );

            var propertyValue = Expression.Property(property, "Value");
            var valueExpressionValue = Expression.Property(valueExpression, "Value");

            Expression comparison = GetComparisonExpression(propertyValue, valueExpressionValue, op);
            if (comparison == null) return null;

            return Expression.AndAlso(nullCheck, comparison);
        }

        // Handle Non-Nullable Types (DateTime, int, string, enum, etc.)
        else
        {
            // Convert valueExpression if needed (especially for DateTime from string)
            if (propertyType == typeof(DateTime) /*&& valueExpression.Type != typeof(DateTime)*/)
            {
                try
                {
                    valueExpression = Expression.Convert(valueExpression, typeof(DateTime));
                }
                catch (InvalidCastException)
                {
                    return Expression.Constant(false);
                }
            }
            else if (propertyType == typeof(int) && valueExpression.Type != typeof(int))
            {
                try
                {
                    valueExpression = Expression.Convert(valueExpression, typeof(int));
                }
                catch (InvalidCastException)
                {
                    return Expression.Constant(false);
                }
            }
            else if (underlyingType != null && underlyingType.IsEnum)
            {

                var enumValue = Enum.Parse(underlyingType, (string)((ConstantExpression)valueExpression).Value);
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    valueExpression = Expression.Convert(Expression.Constant(enumValue, underlyingType), propertyType);
                }
                else
                {
                    valueExpression = Expression.Constant(enumValue, underlyingType);
                }
            }

            return GetComparisonExpression(property, valueExpression, op);
        }
    }

    /// <summary>
    /// Génère une expression de comparaison entre une propriété et une valeur selon l'opérateur donné.
    /// </summary>
    /// <param name="property">Expression représentant la propriété.</param>
    /// <param name="valueExpression">Expression représentant la valeur à comparer.</param>
    /// <param name="op">Opérateur de comparaison.</param>
    /// <returns>Expression de comparaison ou exception si l'opérateur n'est pas supporté.</returns>
    private static Expression GetComparisonExpression(Expression property, Expression valueExpression, string op)
    {
        switch (op)
        {
            case "==":
                return Expression.Equal(property, valueExpression);
            case "!=":
                return Expression.NotEqual(property, valueExpression);
            case ">":
                return Expression.GreaterThan(property, valueExpression);
            case ">=":
                return Expression.GreaterThanOrEqual(property, valueExpression);
            case "<":
                return Expression.LessThan(property, valueExpression);
            case "<=":
                return Expression.LessThanOrEqual(property, valueExpression);
            case "contains": // Special case for strings
                if (property.Type == typeof(string))
                {
                    valueExpression = Expression.Call(valueExpression, "Trim", Type.EmptyTypes);
                    return Expression.Call(property, typeof(string).GetMethod("Contains", new[] { typeof(string) }), valueExpression);
                }
                break;

            default:
                throw new NotSupportedException($"Operator {op} not supported for {property.Type.Name}.");
        }

        return null;
    }
}
