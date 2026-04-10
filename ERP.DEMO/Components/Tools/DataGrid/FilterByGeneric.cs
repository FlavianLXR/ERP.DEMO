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
        var groupedFilters = filters.GroupBy(f => f.Column?.PropertyName ?? f.Title)
                                    .Where(g => g.Key != null);

        Expression combinedExpressionGlobal = null; // Expression globale pour la recherche
        List<Expression> globalConditions = new List<Expression>();
        var parameter = Expression.Parameter(typeof(T), "x");

        foreach (var group in groupedFilters)
        {
            var propertyName = group.Key;
            var (nullGuard, property) = GetPropertyExpression(parameter, propertyName); // ← tuple
            var propertyType = property.Type;

            Expression combinedExpression = null;
            bool isDateColumn = propertyType == typeof(DateTime) || propertyType == typeof(DateTime?);

            bool isNumericColumn = propertyType == typeof(int) || propertyType == typeof(int?)
                                || propertyType == typeof(decimal) || propertyType == typeof(decimal?)
                                || propertyType == typeof(double) || propertyType == typeof(double?)
                                || propertyType == typeof(float) || propertyType == typeof(float?)
                                || propertyType == typeof(long) || propertyType == typeof(long?)
                                || propertyType == typeof(short) || propertyType == typeof(short?);

            foreach (var filter in group)
            {
                // ← Court-circuiter si contains avec valeur vide → inutile de filtrer
                if (filter.Operator == "contains" && string.IsNullOrEmpty(filter.Value?.ToString()))
                    continue;
                if (filter.Value == null && filter.Operator != "is null" && filter.Operator != "is not null")
                    continue;
                if (filter.Operator == null) continue;

                Expression valueExpression = Expression.Constant(filter.Value);
                Expression comparison = ApplyOperator(property, valueExpression, propertyType, filter.Operator);

                if (comparison != null)
                {
                    // Wrappe la comparaison avec le null guard si propriété imbriquée
                    if (nullGuard != null)
                        comparison = Expression.AndAlso(nullGuard, comparison);

                    if (filter.Title == "Recherche globale" && propertyType == typeof(string))
                    {
                        globalConditions.Add(comparison);
                    }
                    else
                    {
                        if (isDateColumn || isNumericColumn)
                            combinedExpression = combinedExpression == null
                                ? comparison
                                : Expression.AndAlso(combinedExpression, comparison);
                        else
                            combinedExpression = combinedExpression == null
                                ? comparison
                                : Expression.OrElse(combinedExpression, comparison);
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
    private static (Expression nullGuard, Expression property) GetPropertyExpression(
     Expression parameter, string propertyName)
    {
        var propertyNames = propertyName.Split('.');
        Expression property = parameter;
        Expression nullGuard = null;

        foreach (var prop in propertyNames)
        {
            // ← Ne pas ajouter de null guard sur le paramètre racine (x lui-même)
            if (!property.Type.IsValueType && property != parameter)
            {
                var isNotNull = Expression.NotEqual(
                    property,
                    Expression.Constant(null, property.Type));

                nullGuard = nullGuard == null
                    ? isNotNull
                    : Expression.AndAlso(nullGuard, isNotNull);
            }
            property = Expression.PropertyOrField(property, prop);
        }

        return (nullGuard, property);
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
        // ← Gérer is null / is not null en premier, avant tout traitement de type
        if (op == "is null")
            return Expression.Equal(property, Expression.Constant(null, property.Type));
        if (op == "is not null")
            return Expression.NotEqual(property, Expression.Constant(null, property.Type));

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
            else if (propertyType == typeof(decimal) && valueExpression.Type != typeof(decimal))
            {
                valueExpression = Expression.Convert(valueExpression, typeof(decimal));
            }
            else if (propertyType == typeof(double) && valueExpression.Type != typeof(double))
            {
                valueExpression = Expression.Convert(valueExpression, typeof(double));
            }

            else if (underlyingType != null && underlyingType.IsEnum)
            {

                var rawValue = ((ConstantExpression)valueExpression).Value?.ToString();
                if (rawValue == null) return null;
                if (!Enum.TryParse(underlyingType, rawValue, out var enumValue)) return null;
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
            case "is null":
                return Expression.Equal(property, Expression.Constant(null, property.Type));
            case "is not null":
                return Expression.NotEqual(property, Expression.Constant(null, property.Type));
            case "contains":
                if (property.Type == typeof(string))
                {
                    var notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

                    // ToLower sur la propriété
                    var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                    var propertyLower = Expression.Call(property, toLowerMethod);

                    // ToLower sur la valeur
                    var valueLower = Expression.Constant(
                        ((ConstantExpression)valueExpression).Value?.ToString()?.ToLower()
                    );

                    var contains = Expression.Call(
                        propertyLower,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        valueLower
                    );

                    return Expression.AndAlso(notNull, contains);
                }
                break;

            default:
                //Logger.LogWarning($"Opérateur {op} non supporté");
                return null;
        }

        return null;
    }
}
