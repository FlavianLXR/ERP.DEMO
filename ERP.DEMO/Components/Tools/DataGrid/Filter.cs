using System.Text.Json.Serialization;

namespace ERP.DEMO.Components.Tools.DataGrid
{
    public class FilterView<T>
    {
        public string Name { get; set; } = string.Empty;
        public List<Filter> Filters { get; set; } = new();
        public List<ColumnState<T>> Columns { get; set; } = new();
        public Dictionary<string, int> ColumnOrder { get; set; } = new();
        public bool IsSelected { get; set; } = false;
        public string OriginalTypeName { get; set; } = typeof(T).AssemblyQualifiedName;

        [JsonIgnore]
        public Type OriginalType => Type.GetType(OriginalTypeName);

    }

    public class Filter
    {
        public string PropertyName { get; set; } = string.Empty;
        public string Operator { get; set; } = "==";
        public object Value { get; set; }
        public DateRangeLabel? DateRangeLabel { get; set; } = null;
    }

    public enum DateRangeLabel
    {
        today,
        last7days,
        last30days,
        last90days,
        last12months,
        custom
    }

    class FilterComparer : IEqualityComparer<Filter>
    {
        public bool Equals(Filter x, Filter y)
        {
            if (x == null || y == null) return false;
            return x.PropertyName == y.PropertyName &&
                   x.Operator == y.Operator &&
                   Equals(x.Value, y.Value);
        }

        public int GetHashCode(Filter obj)
        {
            return HashCode.Combine(obj.PropertyName, obj.Operator, obj.Value);
        }
    }

}
