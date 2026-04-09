using MudBlazor;

namespace ERP.DEMO.Components.Tools.DataGrid
{
    public class DataGridParameters
    {
        public bool IsStripedRows { get; set; } = false; // Indicateur du zébrage des lignes
        public bool ShowBorders { get; set; } = true;    // Affichage des bordures des lignes
        public bool Dense { get; set; } = true; // Densité d'affichage (Compact, Normal, Large)

        public DataGridParameters() { }

        public DataGridParameters(bool isStripedRows, bool showBorders, bool dense)
        {
            IsStripedRows = isStripedRows;
            ShowBorders = showBorders;
            Dense = dense;
        }
    }
}
