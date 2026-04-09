using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ERP.DEMO.Toolkit.Extensions
{
    public static class FileExtensions
    {
        /// <summary>
        /// Récupère les données d'un fichier Excel et les retourne sous forme d'une collection du type T.
        /// </summary>
        /// <typeparam name="T">Le type retourné.</typeparam>
        /// <param name="file">Le fichier à traiter.</param>
        /// <param name="target">Le répertoire de travail cible contenant le fichier.</param>
        /// <returns>Une collection instances de T représentant les données extraites.</returns>
        /// <exception cref="InvalidOperationException">Le fichier n'est pas un fichier excel.</exception>
        /// 
        //public static IEnumerable<T> ExtractXlsData<T>(this HttpPostedFileBase file, string target)
        //{
        //    if (!(file.ContentType == "application/vnd.ms-excel" || file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
        //        throw new InvalidOperationException(string.Format("Le fichier \"{0}\" n'est pas un fichier Excel&trade;", Path.GetFileName(file.FileName)));

        //    var fileName = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(file.FileName), DateTime.Now.ToString("yyyyMMdd_hhmm"), Path.GetExtension(file.FileName));
        //    var excelFilePath = Path.Combine(target, fileName);

        //    try
        //    {
        //        var dir = new DirectoryInfo(target);
        //        if (!dir.Exists) dir.Create();
        //        file.SaveAs(excelFilePath);
        //        #region Get excel connexion string.
        //        string conString = string.Empty;
        //        if (fileName.EndsWith(".xls"))
        //            conString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0; data source={0}; Extended Properties=\"Excel 8.0 Xml;HDR=YES;IMEX=1\";", excelFilePath);
        //        else if (fileName.EndsWith(".xlsx"))
        //            conString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", excelFilePath);
        //        else
        //            throw new Exception(string.Format("L'extention du fichier \"{0}\" n'a pas été reconnue comme étant un fichier Excel&trade;.", Path.GetFileName(file.FileName)));
        //        #endregion

        //        #region Querrying data.
        //        var adapter = new System.Data.OleDb.OleDbDataAdapter("SELECT * FROM [Feuil1$]", conString);
        //        var ds = new System.Data.DataSet();
        //        adapter.Fill(ds, "ExcelTable");
        //        System.Data.DataTable dtable = ds.Tables["ExcelTable"];
        //        string sheetName = "Feuil1";
        //        var excelFile = new LinqToExcel.ExcelQueryFactory(excelFilePath);
        //        var data = from a
        //               in excelFile.Worksheet<T>(sheetName)
        //                   select a;
        //        #endregion

        //        return data.ToList();
        //    }
        //    finally
        //    {
        //        if (!string.IsNullOrWhiteSpace(excelFilePath) && File.Exists(excelFilePath))
        //        {
        //            var directoryPath = Path.Combine(Path.GetDirectoryName(excelFilePath), "Archives");
        //            var dir = new DirectoryInfo(directoryPath);
        //            if (!dir.Exists) dir.Create();

        //            var archiveFileFullname = Path.Combine(directoryPath, Path.GetFileName(excelFilePath));
        //            if (!File.Exists(archiveFileFullname))
        //                File.Copy(excelFilePath, archiveFileFullname);

        //            File.Delete(excelFilePath);
        //        }
        //    }
        //}
    }
}
