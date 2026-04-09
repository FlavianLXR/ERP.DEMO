using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using System.Linq.Expressions;
using static MudBlazor.CategoryTypes;

namespace ERP.DEMO.Components.Tools.DataGrid
{
    public class ColumnState<T>
    {
        public string PropertyName { get; set; }
        public bool? Sortable { get; set; } = true;
        public bool? Filterable { get; set; } = true;
        public bool? Groupable { get; set; } = true;
        public bool? Hideable { get; set; } = true;
        public bool? Hidden { get; set; } = false;
        public RenderFragment<HeaderContext<T>> HeaderTemplate { get; set; }

        public ColumnState() { }


        public ColumnState(Column<T> column)
        {
            PropertyName = column.PropertyName;
            Sortable = column.Sortable;
            Filterable = column.Filterable;
            Groupable = column.Groupable;
            Hideable = column.Hideable;
            Hidden = column.Hidden;
            HeaderTemplate = column.HeaderTemplate;
        }

        public ColumnState(string propertyName = null, bool? sortable = null, bool? filterable = null, bool? groupable = null,
                   bool? hideable = null, bool? hidden = null, RenderFragment<HeaderContext<T>> headerTemplate = null)
        {
            PropertyName = propertyName;
            Sortable = sortable;
            Filterable = filterable;
            Groupable = groupable;
            Hideable = hideable;
            Hidden = hidden;
            HeaderTemplate = headerTemplate;
        }

    }


    //INUTILE CI DESSOUS (INDEX GENERIQUE)
    public class ColumnDefinition<T>
    {
        public Expression<Func<T, object>> Property { get; set; }

        /// <summary>
        /// Titre de l'index
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Défini si la colonne est disponible à l'affichage sur l'index.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Contenu Html représentant le contenu de la cellule d'une colonne
        /// </summary>
        public RenderFragment<T> CellTemplate { get; set; }

        /// <summary>
        /// Défini si la colonne est filtrable sur l'index.
        /// </summary>
        public bool Filterable { get; set; }

        /// <summary>
        /// Défini si la colonne peut être masquée sur l'index sur l'index.
        /// </summary>
        public bool Hideable { get; set; }

        /// <summary>
        /// Défini si la colonne est cachée par défaut sur l'index.
        /// </summary>
        public bool HiddenDefault { get; set; }

        /// <summary>
        /// Défini si la colonne est déplaçable sur l'index.
        /// </summary>
        public bool Draggable { get; set; }

        public ColumnDefinition(Expression<Func<T, object>> property, string title, bool hidden = false, RenderFragment<T> cellTemplate = null, bool filterable = true, bool hideable = true, bool hiddenDefault = false, bool draggable = true)
        {
            Property = property;
            Title = title;
            Hidden = hidden;
            CellTemplate = cellTemplate;
            Filterable = filterable;
            Hideable = hideable;
            HiddenDefault = hiddenDefault;
            Draggable = draggable;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnSettingsAttribute : Attribute
    {
        /// <summary>
        /// Défini si la colonne est filtrable sur l'index.
        /// </summary>
        public bool Filterable { get; set; }

        /// <summary>
        /// Défini si la colonne peut être masquée sur l'index sur l'index.
        /// </summary>
        public bool Hideable { get; set; }

        /// <summary>
        /// Défini si la colonne est cachée par défaut sur l'index.
        /// </summary>
        public bool HiddenDefault { get; set; }

        /// <summary>
        /// Défini si la colonne est disponible à l'affichage sur l'index.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Défini si la colonne est déplaçable sur l'index.
        /// </summary>
        public bool Draggable { get; set; }

        public ColumnSettingsAttribute(bool filterable = true, bool hideable = true, bool hidden = false, bool hiddenDefault = false, bool draggable = true)
        {
            Filterable = filterable;
            Hideable = hideable;
            Hidden = hidden;
            HiddenDefault = hiddenDefault;
            Draggable = draggable;
        }
    }
}