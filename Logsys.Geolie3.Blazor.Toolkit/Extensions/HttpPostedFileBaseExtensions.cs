using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ERP.DEMO.Toolkit.Extensions
{
    public static class HttpPostedFileBaseExtensions
    {
        private const int ImageMinimumBytes = 512;

        /// <summary>
        /// Vérifie si un fichier téléchargé est de type image.
        /// </summary>
        /// <param name="postedFile">Le fichier à vérifier.</param>
        /// <returns>True si le fichier est une image, sinon false.</returns>
        //public static bool IsImage(this System.Web.HttpPostedFileBase postedFile)
        //{
        //    if (postedFile.ContentType.ToLower() != "image/jpg" &&
        //                postedFile.ContentType.ToLower() != "image/jpeg" &&
        //                postedFile.ContentType.ToLower() != "image/pjpeg" &&
        //                postedFile.ContentType.ToLower() != "image/gif" &&
        //                postedFile.ContentType.ToLower() != "image/x-png" &&
        //                postedFile.ContentType.ToLower() != "image/png")
        //    {
        //        return false;
        //    }

        //    if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
        //        && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
        //        && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
        //        && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
        //    {
        //        return false;
        //    }

        //    try
        //    {
        //        if (!postedFile.InputStream.CanRead)
        //        {
        //            return false;
        //        }

        //        if (postedFile.ContentLength < ImageMinimumBytes)
        //        {
        //            return false;
        //        }

        //        byte[] buffer = new byte[512];
        //        postedFile.InputStream.Read(buffer, 0, 512);
        //        string content = System.Text.Encoding.UTF8.GetString(buffer);
        //        if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
        //            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //    try
        //    {
        //        using (var bitmap = new System.Drawing.Bitmap(postedFile.InputStream)) { }
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        postedFile.InputStream.Position = 0;
        //    }

        //    return true;
        //}
    }
}
