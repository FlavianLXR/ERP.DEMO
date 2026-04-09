using Microsoft.JSInterop;
using System.Reflection;
using static ERP.DEMO.Components.MVVM.BaseService;

namespace ERP.DEMO.Components.Tools
{
    public class Tools
    {
        private readonly IJSRuntime _jsRuntime;

        public Tools(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task LogAsync(string message)
        {
            await _jsRuntime.InvokeVoidAsync("console.log", message);
        }

        /// <summary>
        /// Récupère un <see cref="PropertyInfo"/> pour une propriété imbriquée dans un objet en utilisant un chemin de propriété (séparé par des points).
        /// </summary>
        /// <param name="type">Le type de l'objet sur lequel la recherche de propriété doit être effectuée.</param>
        /// <param name="propertyPath">Le chemin de la propriété imbriquée (par exemple, "Order.Origin.Label").</param>
        /// <returns>Le <see cref="PropertyInfo"/> correspondant à la propriété imbriquée, ou null si la propriété n'existe pas.</returns>
        public static PropertyInfo GetNestedProperty(Type type, string propertyPath)
        {
            PropertyInfo property = null;
            foreach (var part in propertyPath.Split('.'))
            {
                property = type.GetProperty(part);
                if (property == null)
                {
                    return null; // La propriété n'existe pas dans ce type
                }

                type = property.PropertyType; // Passe au type suivant
            }
            return property;
        }

        /// <summary>
        /// Récupère la valeur d'une propriété imbriquée dans un objet en utilisant un chemin de propriété (séparé par des points).
        /// </summary>
        /// <param name="obj">L'objet sur lequel la valeur de la propriété imbriquée doit être récupérée.</param>
        /// <param name="propertyPath">Le chemin de la propriété imbriquée (par exemple, "Order.Origin.Label").</param>
        /// <returns>La valeur de la propriété imbriquée, ou null si l'une des propriétés est introuvable ou si une valeur est null à un niveau quelconque.</returns>
        public static object GetNestedPropertyValue(object obj, string propertyPath)
        {
            if (obj == null) return null;

            var type = obj.GetType();
            foreach (var part in propertyPath.Split('.'))
            {
                var property = type.GetProperty(part);
                if (property == null)
                {
                    return null; // La propriété n'existe pas dans ce type
                }

                obj = property.GetValue(obj); // Accéder à la valeur de la propriété
                if (obj == null)
                {
                    return null; // La valeur est null, retourner null
                }

                type = obj.GetType(); // Passe au type suivant
            }
            return obj; // Retourne la valeur de la dernière propriété
        }

        public static T CloneModel<T>(T source)
        {
            var type = typeof(T);
            var clone = Activator.CreateInstance<T>();

            foreach (var prop in type.GetProperties().Where(p => p.CanRead && p.CanWrite))
            {
                // Ignore navigation properties (ex: collections)
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                    continue;

                // Clone value types or strings or shallow object refs
                var value = prop.GetValue(source);

                // If it's a class (except string), clone recursively or shallow-copy
                if (value is not null && prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    // Avoid recursive loops (e.g. Product.Type.Products)
                    var subClone = Activator.CreateInstance(prop.PropertyType);
                    foreach (var subProp in prop.PropertyType.GetProperties().Where(p => p.CanRead && p.CanWrite))
                    {
                        if (subProp.PropertyType.IsClass && subProp.PropertyType != typeof(string))
                            continue;

                        var subVal = subProp.GetValue(value);
                        subProp.SetValue(subClone, subVal);
                    }
                    prop.SetValue(clone, subClone);
                }
                else
                {
                    prop.SetValue(clone, value);
                }
            }

            return clone;
        }

    }
}
