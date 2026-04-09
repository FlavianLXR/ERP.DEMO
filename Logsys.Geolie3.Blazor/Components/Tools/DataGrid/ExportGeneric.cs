using ClosedXML.Excel;
using ERP.DEMO.Models.TestDb;
using MudBlazor;
using System.Dynamic;
using System.Globalization;
using System.Text;

namespace ERP.DEMO.Components.Tools.DataGrid
{
    public class ExportGeneric
    {
        public static byte[] ExportToCsv(List<ExpandoObject> list)
        {
            var sb = new StringBuilder();
            var headers = list.SelectMany(item => (IDictionary<string, object>)item)
                              .Select(kv => kv.Key).Distinct().ToList();

            sb.AppendLine(string.Join(",", headers));

            foreach (var item in list)
            {
                var values = headers.Select(header =>
                {
                    ((IDictionary<string, object>)item).TryGetValue(header, out var value);
                    return value?.ToString().Replace(",", ";") ?? "";
                }).ToArray();
                sb.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public static byte[] ExportToExcel<T>(List<T> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Export");

                var firstItem = data.FirstOrDefault();
                if (firstItem == null) return Array.Empty<byte>();

                if (firstItem is ExpandoObject expandoItem)
                {
                    var properties = ((IDictionary<string, object>)expandoItem).Keys.ToList();

                    for (int col = 0; col < properties.Count; col++)
                    {
                        worksheet.Cell(1, col + 1).Value = properties[col];
                        worksheet.Cell(1, col + 1).Style.Fill.PatternType = XLFillPatternValues.Solid;
                        worksheet.Cell(1, col + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#90c2ed");
                        worksheet.Cell(1, col + 1).Style.Font.Bold = true;

                    }

                    int currentRow = 2;
                    foreach (var item in data)
                    {
                        if (item is ExpandoObject row)
                        {
                            var dict = (IDictionary<string, object>)row;
                            int col = 1;
                            foreach (var key in properties)
                            {
                                worksheet.Cell(currentRow, col).Value = dict.ContainsKey(key) ? dict[key]?.ToString() : string.Empty;
                                col++;
                            }
                            currentRow++;
                        }
                    }

                    worksheet.Columns().AdjustToContents();
                    worksheet.Columns().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}