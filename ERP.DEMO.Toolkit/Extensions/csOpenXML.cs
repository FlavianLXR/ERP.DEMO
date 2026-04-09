using System;
using System.Linq;
using System.IO;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Table.PivotTable;

public partial class csXLSX: IDisposable
{
    FileInfo newFile = null;
    public csXLSX()
    {
    }

    public csXLSX(string fileName)
    {
        this.newFile = new FileInfo(fileName);
    }

    ~csXLSX()
    {
        this.Dispose();
    }

    public void Dispose()
    {
        this.newFile = null;
        if (this.pck != null)
            try { this.pck.Dispose(); } catch { }
    }
    
    ExcelPackage pck = null;

    public DataSet Get()
    {
        return this.Get(0, false);
    }
    public DataSet Get(int sheet)
    {
        return this.Get(sheet, false);
    }
    public DataSet Get(bool firstIsEntete)
    {
        return this.Get(0, firstIsEntete);
    }
    public DataSet Get(int sheet, bool firstIsEntete)
    {
        try
        {
            if (this.newFile == null)
                throw new Exception("Aucun fichier fourni !");

            this.pck = new ExcelPackage(newFile);

            var wb = pck.Workbook;
            int nbWs = 0;

            try
            {
                nbWs = wb.Worksheets.Count;
            }
            catch
            {
                throw new Exception("Erreur initialisation : Worksheet introuvable !");
            }

            DataSet ds = new DataSet();

            foreach (var ws in wb.Worksheets)
            {
                System.Collections.Generic.IEnumerable<ExcelRangeBase> query1 = null;
                
                try { query1 = (from cell in ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column] select cell); }
                catch { continue; } // passe si vide !!!!

                var dt = new DataTable(ws.Name);
                int lastLine = -1;
                DataRow row = null;

                if (sheet > 0 && sheet != ws.Index)
                    continue;

                foreach (var cell in query1)
                {
                    int line = cell.End.Row,
                        col = cell.End.Column;


                    if (cell.Text != string.Empty)
                    {
                        if (firstIsEntete && line == 1)
                        {
                            while (dt.Columns.Count < (col - 1))
                                dt.Columns.Add(new DataColumn("C" + ws.Index + "_" + (col - 1)));

                            dt.Columns.Add(new DataColumn(cell.Value.ToString()));
                        }
                        else
                        {
                            while (dt.Columns.Count < col)
                                dt.Columns.Add(new DataColumn("C" + ws.Index + "_" + ( col )));

                            if (lastLine != line)
                            {
                                if (row != null)
                                    dt.Rows.Add(row);
                                row = dt.NewRow();
                            }

                            row[col - 1] = cell.Text;
                            lastLine = line;
                        }
                    }
                }

                if (row != null)
                    dt.Rows.Add(row);

                ds.Tables.Add(dt);
            }
            return ds;
        }
        catch (Exception err)
        {
            throw err;
        }
        finally
        {
            if (this.pck != null)
                this.pck.Dispose();
        }
    }

    public MemoryStream DumpExcel(DataSet ds)
    {
        return this.DumpExcel(ds, new Chart[0]);
    }
    public MemoryStream DumpExcel(DataSet ds, params Chart[] myCharts)
    {
        return this.DumpExcel(ds, myCharts.ToList<Chart>());
    }
    public MemoryStream DumpExcel(DataSet ds, System.Collections.Generic.List<Chart> myCharts)
    {
        return DumpExcel(ds, new System.Collections.Generic.List<xCrossTable>(), myCharts);
        //return DumpExcel(ds, new xCrossTable[0].ToList(), myCharts);
    }
    public MemoryStream DumpExcel(DataSet ds, System.Collections.Generic.List<xCrossTable> aCrossTable)
    {
        return DumpExcel(ds, aCrossTable, new System.Collections.Generic.List<Chart>());
    }
    public MemoryStream DumpExcel(DataSet ds, System.Collections.Generic.List<xCrossTable> aCrossTable, System.Collections.Generic.List<Chart> myCharts)
    {
//#if DEBUG
//        using (var pck = this.Set1(ds, aCrossTable, myCharts))
//            return (new System.IO.MemoryStream(pck.GetAsByteArray()));
//#else
        this.Set(ds, aCrossTable, myCharts);
        if (this.pck != null)
        {
            var bytes = this.pck.GetAsByteArray();
            return (new System.IO.MemoryStream(bytes));
        }
        else
            throw new Exception("Aucun flux !");
//#endif
    }

    private void Set(DataSet ds, System.Collections.Generic.List<xCrossTable> aCrossTable, params Chart[] myCharts)
    {
        this.Set(ds, aCrossTable, myCharts.ToList<Chart>());
    }
    private void Set(DataSet ds, System.Collections.Generic.List<xCrossTable> aCrossTable, System.Collections.Generic.List<Chart> myCharts)
    {
        pck = new ExcelPackage();
        pck.Compression = CompressionLevel.BestCompression;
        var wb = pck.Workbook;

        foreach (DataTable dt in ds.Tables)
        {
            int numTable = ds.Tables.IndexOf(dt) + 1;

            wb.Worksheets.Add((!string.IsNullOrEmpty(dt.TableName) || dt.TableName != "Table") ? dt.TableName : "Sheet " + numTable.ToString("00"));
            var ws = wb.Worksheets[numTable];

            var dtData = dt;
            
            if (aCrossTable != null && aCrossTable.Count > 0)
            {
                foreach (var crossTable in aCrossTable)
                {
                    if (crossTable.idxPage != numTable)
                        continue;

                    var pivotCols = (from row in dt.AsEnumerable()
                                     select row[(int)crossTable.idxPivot].ToString()).Distinct<string>();

                    var colsLabel = "";
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (dt.Columns.IndexOf(col.ColumnName) == crossTable.idxPivot || 
                            dt.Columns.IndexOf(col.ColumnName) == crossTable.idxData) continue;
                        colsLabel += "," + col.ColumnName;
                    }

                    var aColsLabel = colsLabel.Substring(1).Split(',');

                    DataTable returnTable = new DataTable();

                    foreach (string col in aColsLabel)
                        returnTable.Columns.Add(col);
                    foreach (string col in pivotCols)
                        returnTable.Columns.Add(col);

                    #region récupération des labels et tri
                    var aLabels = new System.Collections.SortedList();
                    foreach (DataRow row in dt.Rows)
                    {
                        int i = 0;
                        string key = "";

                        DataRow dr = returnTable.NewRow();
                        foreach (var o in row.ItemArray)
                        {
                            if (row.ItemArray.ToList().IndexOf(o) != crossTable.idxPivot &&
                                row.ItemArray.ToList().IndexOf(o) != crossTable.idxData)
                            {
                                dr[i++] = o.ToString();
                                key += o + ",";
                            }
                        }
                        if (aLabels.IndexOfKey(key) < 0)
                            aLabels.Add(key, dr);
                    }
                    #endregion

                    #region ajout des transporteurs et services
                    for (int k = 0; k < aLabels.Count; ++k)
                    {
                        var row = aLabels.GetByIndex(k) as DataRow;

                        int i = 0;
                        DataRow dr = returnTable.NewRow();
                        foreach (var o in row.ItemArray)
                        {
                            if (("" + o) != string.Empty)
                                dr[i++] = o.ToString();
                        }
                        returnTable.Rows.Add(dr);
                    }
                    #endregion

                    #region ajout des données croisées
                    foreach (DataRow row in dt.Rows)
                    {
                        var strFiltre = string.Empty;

                        foreach (var label in aColsLabel)
                        {
                            if (strFiltre.Length > 0) strFiltre += " AND ";
                            strFiltre += "([" + label + "]='" + row[dt.Columns.IndexOf(label)] + "')";
                        }

                        int i = returnTable.Rows.IndexOf(returnTable.Select(strFiltre).First());
                        //if (i > 0)
                        returnTable.Rows[i][row[crossTable.idxPivot].ToString()] = row[crossTable.idxData];
                    }
                    #endregion

                    dtData = returnTable;
                    //wb.Worksheets.Add("Pivot_Sheet " + numTable.ToString("00"));
                    //var _ws = wb.Worksheets[numTable + 1];
                    //_ws.Cells["A1"].LoadFromDataTable(dt, true);
                }
            }
            
            ws.Cells["A1"].LoadFromDataTable(dtData, true).AutoFitColumns();

            //var dataRange = ws.Cells[ws.Dimension.Address.ToString()];
            //dataRange.AutoFitColumns();

            foreach (DataColumn col in dtData.Columns)
            {
                int idx = dtData.Columns.IndexOf(col);
                switch (col.DataType.Name)
                {
                    case "DateTime":
                        try
                        {
                            ws.Cells[2, idx + 1, dtData.Rows.Count + 1, idx + 1].Style.Numberformat.Format = "dd/mm/yyyy HH:mm:ss";
                        }
                        catch { }
                        break;
                }
            }

            if (true)
            {
                var style = ws.Cells[1, 1, 1, dtData.Columns.Count].Style;
                style.Font.Bold = true;
                style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSeaGreen);
            }

            foreach (Chart myChart in myCharts)
            {
                if (myChart.numSheetToChart == numTable)
                {
                    var chart = myChart.SetChart(ws);
                    chart.Series.Add(
                        ExcelRange.GetAddress(2, myChart.data.X, dt.Rows.Count + 1, myChart.data.Y),
                        ExcelRange.GetAddress(2, myChart.labels.X, dt.Rows.Count + 1, myChart.labels.Y));

                    chart.SetPosition(myChart.position.X, 0, myChart.position.Y, 0);
                    chart.SetSize(myChart.size.Width, myChart.size.Height);

                    if (myChart.labels == System.Drawing.Point.Empty)
                        myChart.labels = new System.Drawing.Point(1, 1);
                    if (myChart.data == System.Drawing.Point.Empty)
                        myChart.data = new System.Drawing.Point(2, dt.Columns.Count);

                    chart.Title.Text = myChart.title;
                    chart.Legend.Remove();
                }
            }
        }
    }

    public MemoryStream DumpExcel(System.Collections.Generic.List<Table> aCrossTable)
    {
        this.Set1(aCrossTable);
        if (this.pck != null)
        {
            var bytes = this.pck.GetAsByteArray();
            return (new System.IO.MemoryStream(bytes));
        }
        else
            throw new Exception("Aucune donnée !");
    }
    private ExcelPackage Set1(System.Collections.Generic.List<Table> tables)
    {
        this.pck = new ExcelPackage();
        this.pck.Compression = CompressionLevel.BestCompression;

        var wb = this.pck.Workbook;
        wb.FullCalcOnLoad = true;

        foreach (Table table in tables)
        {
            var prefix = string.Empty;
            if (table.pivots != null) prefix = "DATA_";
            var wsData = wb.Worksheets.Add(prefix + table.data.TableName);
            var range = wsData.Cells["A1"].LoadFromDataTable(table.data, true);
            range.AutoFitColumns();

            var entete = wsData.Cells[1, 1, 1, table.data.Columns.Count].Style;
            entete.Font.Color.SetColor(System.Drawing.Color.White);
            entete.Font.Bold = true;
            entete.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            entete.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 112, 192));

            if (table.pivots != null)
            {
                wsData.Hidden = eWorkSheetHidden.Hidden;

                foreach (Table.pivotTable pivot in table.pivots)
                {
                    if (pivot.rowsFields != null && pivot.colField != null && pivot.rowsFields.Count > 0)
                    {
                        var wsPivot = wb.Worksheets.Add(table.data.TableName);

                        var position = wsPivot.Cells["A1"];
                        if (pivot.position != null) position = wsPivot.Cells[pivot.position];
                        var pivotTable = wsPivot.PivotTables.Add(position, range, "pivot_" + table.data.TableName);
                        pivotTable.CompactData = true;

                        pivotTable.RowHeaderCaption = string.Empty;
                        foreach (string rowField in pivot.rowsFields)
                        {
                            if (pivotTable.RowHeaderCaption.Length > 0) pivotTable.RowHeaderCaption += " - ";
                            pivotTable.RowHeaderCaption += rowField;
                            pivotTable.RowFields.Add(pivotTable.Fields[rowField]).Sort = eSortType.Ascending;
                        }

                        pivotTable.ColumnFields.Add(pivotTable.Fields[pivot.colField]);
                        pivotTable.DataOnRows = true;

                        pivotTable.DataCaption = string.Empty;
                        foreach (Table.pivotTable.Field field in pivot.dataFields)
                        {
                            if (pivotTable.DataCaption.Length > 0) pivotTable.DataCaption += " - ";
                            pivotTable.DataCaption += field.dataField;

                            var pivotField = pivotTable.DataFields.Add(pivotTable.Fields[field.dataField]);
                            pivotField.Name = field.dataField;
                            if (field.function != null)
                                pivotField.Function = field.function;
                            if (field.dataFormat != null && field.dataFormat.Trim().Length > 0)
                                pivotField.Format = field.dataFormat;
                        }

                        wsPivot.Calculate();

                        if (table.pivots.Count == 1)
                        {
                            wsPivot.View.FreezePanes(3, 1);
                            wsPivot.View.FreezePanes(1, 2);
                        }
                    }
                }
            }
        }

        return pck;
    }

    public class Chart
    {
        public int? numSheetToChart;
        public bool
            showCategory = true,
            showPercent = true,
            showLeaderLine = true;
        public System.Drawing.Point
            position, labels, data;
        public xCrossTable
            crossTable;
        public System.Drawing.Size
            size;
        public string title = "Extension Size";
        public Types Type = Types.PieExploded3D;

        // Limiter la sélection aux types gérés
        public enum Types
        {
            PieExploded = OfficeOpenXml.Drawing.Chart.eChartType.PieExploded,
            PieExploded3D = OfficeOpenXml.Drawing.Chart.eChartType.PieExploded3D,
            Pie = OfficeOpenXml.Drawing.Chart.eChartType.Pie,
            Pie3D = OfficeOpenXml.Drawing.Chart.eChartType.Pie3D,
            //Column3D = OfficeOpenXml.Drawing.Chart.eChartType.Column3D,
            ColumnStacked = OfficeOpenXml.Drawing.Chart.eChartType.ColumnStacked,
            Line = OfficeOpenXml.Drawing.Chart.eChartType.Line,
            Line3D = OfficeOpenXml.Drawing.Chart.eChartType.Line3D
        };
        

        public Chart()
        {
            this.position = new System.Drawing.Point(0, 0);
            this.size = new System.Drawing.Size(400, 400);
        }

        public OfficeOpenXml.Drawing.Chart.ExcelChart SetChart(ExcelWorksheet ws)
        {
            switch (this.Type)
            {
                case Types.Pie:
                case Types.Pie3D:
                case Types.PieExploded:
                case Types.PieExploded3D:
                    var pie = ws.Drawings.AddChart("crt_"+title.GetHashCode(), (OfficeOpenXml.Drawing.Chart.eChartType)this.Type) as OfficeOpenXml.Drawing.Chart.ExcelPieChart;
                    pie.DataLabel.ShowCategory = showCategory;
                    pie.DataLabel.ShowPercent = showPercent;
                    pie.DataLabel.ShowLeaderLines = showLeaderLine;
                    return pie;
                case Types.ColumnStacked:
                    //case Types.Column3D:
                    var bar = ws.Drawings.AddChart("crt_" + title.GetHashCode(), (OfficeOpenXml.Drawing.Chart.eChartType)this.Type) as OfficeOpenXml.Drawing.Chart.ExcelBarChart;
                    bar.DataLabel.ShowCategory = showCategory;
                    bar.DataLabel.ShowPercent = showPercent;
                    bar.DataLabel.ShowLeaderLines = showLeaderLine;
                    return bar;
                case Types.Line:
                case Types.Line3D:
                    var line = ws.Drawings.AddChart("crt_" + title.GetHashCode(), (OfficeOpenXml.Drawing.Chart.eChartType)this.Type) as OfficeOpenXml.Drawing.Chart.ExcelLineChart;
                    line.DataLabel.ShowCategory = showCategory;
                    line.DataLabel.ShowPercent = showPercent;
                    line.DataLabel.ShowLeaderLines = showLeaderLine;
                    return line;
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Obsolète
    /// </summary>
    public class xCrossTable
    {
        public int
            idxPage,
            idxPivot,
            idxData;
    }

    /// <summary>
    /// Classe pour tables Pivot
    /// </summary>
    public class Table
    {
        public DataTable
            data;

        public System.Collections.Generic.List<pivotTable>
            pivots;

        public class pivotTable
        {
            public String
                position,
                colField;
            public System.Collections.Generic.List<String>
                rowsFields;
            public System.Collections.Generic.List<Field>
                dataFields;
            System.Collections.Generic.List<Chart>
                charts;

            public class Field
            {
                public String
                    dataField, 
                    dataFormat;
                public DataFieldFunctions function = DataFieldFunctions.Sum;
            }
        }
    }
}
