using MudBlazor;

namespace ERP.DEMO.Components.Tools.DataGrid
{
    public class DataGridParameters
    {
        // Affichage de base du DataGrid
        public bool IsStripedRows { get; set; } = false; // Indicateur du zébrage des lignes
        public bool ShowBorders { get; set; } = true;    // Affichage des bordures des lignes
        public bool Dense { get; set; } = true; // Densité d'affichage (Compact, Normal, Large)
        public bool Hover { get; set; } = true; // Mise en surbrillance de la ligne au survol de la souris

        // Persistance et Vues
        public bool SaveGlobalSearch { get; set; } = true; // Sauvegarder la recherche global dans la vue

        public DataGridParameters() { }

        public DataGridParameters(bool isStripedRows, bool showBorders, bool dense, bool hover, bool saveGlobalSearch)
        {
            IsStripedRows = isStripedRows;
            ShowBorders = showBorders;
            Dense = dense;
            Hover = hover;
            SaveGlobalSearch = saveGlobalSearch;
        }
    }
}
