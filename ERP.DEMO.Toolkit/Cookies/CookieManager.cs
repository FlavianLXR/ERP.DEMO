using System;
using System.Web;

namespace ERP.DEMO.Toolkit.Cookies
{
    public sealed class CookieManager
    {
        public enum CookieDuration { Months, Days, Hours, Minutes }

        /// <summary>
        /// Obtient ou définit le Request Http courant.
        /// </summary>
        private HttpRequestBase Request { get; set; }

        /// <summary>
        /// Obtient ou définit le Response actuel.
        /// </summary>
        private HttpResponseBase Response { get; set; }

        /// <summary>
        /// Initialise une nouvelle instance de la classe CookieManager à partir du HttpContext courant.
        /// </summary>
        /// <param name="request">Le request actuel.</param>
        /// <param name="response">Le response actuel.</param>
        public CookieManager(HttpRequestBase request, HttpResponseBase response)
        {
            Request = request;
            Response = response;
        }

        /// <summary>
        /// Ecrit le cookie nommé pour une durée de vie par défaut d'un an.
        /// </summary>
        /// <param name="name">Nom du cookie à écrire.</param>
        /// <param name="value">Contenu du cookie</param>
        /// <param name="durationType">Le type de durée de vie</param>
        /// <param name="duration">Durée de vie du cookie selon le type de durée de vie choisie.</param>
        public void WriteCookie(string name, string value, string key = null, CookieDuration? durationType = null, double? duration = null)
        {
            HttpCookie cookie = null;
            if (Request.Cookies[name] != null)
                cookie = Request.Cookies[name];
            else
                cookie = new HttpCookie(name);

            if (string.IsNullOrWhiteSpace(key))
                cookie.Value = value;
            else
            {
                cookie.Values.Remove(key);
                cookie.Values.Add(key, value);
            }

            cookie.Secure = true;
            cookie.HttpOnly = true;
            //cookie.Path += ";SameSite=lax";
            cookie.SameSite = SameSiteMode.Lax;
            if (durationType != null && duration == null)
                throw new ArgumentNullException("duration");

            switch (durationType)
            {
                case CookieDuration.Months:
                    if (duration > 12) duration = 12;
                    cookie.Expires = DateTime.Now.AddMonths((int)duration);
                    break;
                case CookieDuration.Days:
                    cookie.Expires = DateTime.Now.AddDays((double)duration);
                    break;
                case CookieDuration.Hours:
                    cookie.Expires = DateTime.Now.AddHours((double)duration);
                    break;
                case CookieDuration.Minutes:
                    cookie.Expires = DateTime.Now.AddMinutes((double)duration);
                    break;
                default:
                    cookie.Expires = DateTime.Now.AddYears(1);
                    break;
            }

            Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Récupère le contenu du cookie nommé.
        /// </summary>
        /// <param name="name">Nom du cookie à lire.</param>
        /// <returns>Chaîne représentant le contenu.</returns>
        public string ReadCookie(string name, string key = null)
        {
            HttpCookie cookie = Request.Cookies[name];
            if (cookie == null)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(key))
                return cookie.Value;
            else
                return cookie.Values[key];
        }

        /// <summary>
        /// Supprime le cookie nommé.
        /// </summary>
        /// <param name="name">Nom du cookie à écrire.</param>
        public void DeleteCookie(string name)
        {
            HttpCookie cookie = Request.Cookies[name];
            if (cookie == null) return;

            cookie.Values.Clear();
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Supprime une clef d'un cookie
        /// </summary>
        /// <param name="name">Le nom du cookie.</param>
        /// <param name="key">La clef à supprimer.</param>
        public void RemoveKeyFromCookie(string name, string key)
        {
            HttpCookie cookie = null;
            if (Request.Cookies[name] != null)
                cookie = Request.Cookies[name];
            else
                return;

            var expires = cookie.Expires;
            cookie.Values.Remove(key);
            cookie.Expires = expires;
            Response.Cookies.Add(cookie);
        }
    }
}
