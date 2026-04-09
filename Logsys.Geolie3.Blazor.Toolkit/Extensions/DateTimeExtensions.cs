using System;
using System.Collections.Generic;
using System.Globalization;

namespace ERP.DEMO.Toolkit.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Obtient la date du premier jour de la semaine à laquelle la date courrante appartient.
        /// </summary>
        /// <param name="dt">La date courrante</param>
        /// <returns>Une instance de DateTime représentant le lundi courrant.</returns>
        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            int diff = dt.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0)
                diff += 7;
            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Obtient la date du dernier jour de la semaine à laquelle la date courrante appartient.
        /// </summary>
        /// <param name="dt">La date courrante</param>
        /// <returns>Une instance de DateTime représentant le dimanche courrant.</returns>
        public static DateTime LastDayOfWeek(this DateTime dt)
        {
            int diff = DayOfWeek.Sunday - dt.DayOfWeek;
            if (diff < 0)
                diff += 7;
            return dt.AddDays(1 * diff).Date;
        }

        /// <summary>
        /// Obtient le N° de semaine, au format Iso8601, de la date.
        /// </summary>
        /// <param name="dt">La date pour laquelle on veut connaître le N° de semaine.</param>
        /// <returns>Un entier représentant le N° de semaine.</returns>
        public static int GetWeekNumber(this DateTime dt)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dt);
            DateTime date = dt;
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                date = date.AddDays(3);

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /// <summary>
        /// Obtient la date du lundi d'une semaine selon la norme Iso8601.
        /// </summary>
        /// <param name="year">L'année de la semaine.</param>
        /// <param name="weekOfYear">Le N° de la semaine.</param>
        /// <returns>Une instance de DateTime représantant le lundi de la semaine.</returns>
        public static DateTime FirstDayOfWeek(this DateTime dt, int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }

        /// <summary>
        /// Obtient le jour ouvré précédant le jour représenté par l'instance courante.
        /// </summary>
        /// <param name="dt">L'instance représentant le jour courant.</param>
        /// <param name="pentecostMondayAsHolliday">True si on souhaite ajouter le lundi de pentecôte à la liste des jours fériés (par défaut), sinon false</param>
        /// <returns>Retourne une instance de DateTime représentant le jour ouvré précédant.</returns>
        public static DateTime PreviousWorkingDay(this DateTime dt, bool pentecostMondayAsHolliday = true)
        {
            HashSet<DateTime> publicHollidays = GetFixedHollidays(dt);
            publicHollidays.UnionWith(ComputeEaster(dt.Year, pentecostMondayAsHolliday));
            return GetPreviousWorkingDay(dt, publicHollidays);
        }

        /// <summary>
        /// Obtient le jour ouvré suivant le jour représenté par l'instance courante.
        /// </summary>
        /// <param name="dt">L'instance représentant le jour courant.</param>
        /// <param name="pentecostMondayAsHolliday">True si on souhaite ajouter le lundi de pentecôte à la liste des jours fériés (par défaut), sinon false</param>
        /// <returns>Retourne une instance de DateTime représentant le jour ouvré suivant.</returns>
        public static DateTime NextWorkingDay(this DateTime dt, bool pentecostMondayAsHolliday = true)
        {
            HashSet<DateTime> publicHollidays = GetFixedHollidays(dt);
            publicHollidays.UnionWith(ComputeEaster(dt.Year, pentecostMondayAsHolliday));
            return GetNextWorkingDay(dt, publicHollidays);
        }

        /// <summary>
        /// Permet de savoir si la date courrante est un jour ouvré.
        /// </summary>
        /// <param name="dt">L'instance représentant le jour courant.</param>
        /// <param name="pentecostMondayAsHolliday">True si on souhaite ajouter le lundi de pentecôte à la liste des jours fériés (par défaut), sinon false</param>
        /// <returns>Retourne True si le jour courant est un jour ouvré, sinon false.</returns>
        public static bool IsWorkingDay(this DateTime dt, bool pentecostMondayAsHolliday = true)
        {
            HashSet<DateTime> publicHollidays = GetFixedHollidays(dt);
            publicHollidays.UnionWith(ComputeEaster(dt.Year, pentecostMondayAsHolliday));
            return !publicHollidays.Contains(dt);
        }

        /// <summary>
        /// Obtient le nombre de jours ouvrés entre l'instance de DateTime et la valeur.
        /// </summary>
        /// <param name="dt">L'instance de DateTime.</param>
        /// <param name="value">Une instance de DateTime représentant la valeur à comparer supérieure à l'instance courante</param>
        /// <param name="pentecostMondayAsHolliday">True si on souhaite ajouter le lundi de pentecôte à la liste des jours fériés (par défaut), sinon false</param>
        /// <exception cref="ArgumentOutOfRangeException">La valeur passée pour le comparateur est supérieure à la valeur de l'instance.</exception>
        /// <returns>Un entier représentant le nombre de jours ouvrés entre l'instance de DateTime et la valeur</returns>
        public static double WorkingDayGap(this DateTime dt, DateTime value, bool pentecostMondayAsHolliday = true)
        {
            if (value < dt)
                throw new ArgumentOutOfRangeException("value", "Value ne peut pas être supérieure à l'instance passée.");

            double remove = 0;
            for (DateTime d = dt; d < value; d = d.AddDays(1))
                if (!d.IsWorkingDay(pentecostMondayAsHolliday))
                    remove++;

            return (value - dt).TotalDays - remove;
        }

        /// <summary>
        /// Obtient le jour travaillé précédant le jour de réféence (Méthode récursive).
        /// </summary>
        /// <param name="dt">Le jour de référence.</param>
        /// <param name="publicHollidays">La liste des jours fériés à exclure.</param>
        /// <returns>Une instance de DateTime représentant le jour ouvré précédent.</returns>
        private static DateTime GetPreviousWorkingDay(DateTime dt, HashSet<DateTime> publicHollidays)
        {
            DateTime previous = dt.AddDays(dt.DayOfWeek == DayOfWeek.Monday ? -3 : dt.DayOfWeek == DayOfWeek.Sunday ? -2 : -1);
            if (publicHollidays.Contains(previous))
                previous = GetPreviousWorkingDay(previous, publicHollidays);

            return previous;
        }

        /// <summary>
        /// Obtient le jour travaillé suivant le jour de réféence (Méthode récursive).
        /// </summary>
        /// <param name="dt">Le jour de référence.</param>
        /// <param name="publicHollidays">La liste des jours fériés à exclure.</param>
        /// <returns>Une instance de DateTime représentant le jour ouvré suivant.</returns>
        private static DateTime GetNextWorkingDay(DateTime dt, HashSet<DateTime> publicHollidays)
        {
            DateTime next = dt.AddDays(dt.DayOfWeek == DayOfWeek.Friday ? 3 : dt.DayOfWeek == DayOfWeek.Saturday ? 2 : 1);
            if (publicHollidays.Contains(next))
                next = GetNextWorkingDay(next, publicHollidays);

            return next;
        }

        /// <summary>
        /// Obtient la liste des jours fériés fixes en France.
        /// </summary>
        /// <param name="dt">Le jour de référence.</param>
        /// <returns>Une instance de Hashset de DateTimes représentant la liste dews jours fériés.</returns>
        private static HashSet<DateTime> GetFixedHollidays(DateTime dt)
        {
            return new HashSet<DateTime>
            {
                new DateTime(dt.Year,1,1),
                new DateTime(dt.Year,5,1),
                new DateTime(dt.Year,5,8),
                new DateTime(dt.Year,7,14),
                new DateTime(dt.Year,8,15),
                new DateTime(dt.Year,11,1),
                new DateTime(dt.Year,11,11),
                new DateTime(dt.Year,12,25)
            };
        }

        /// <summary>
        /// Calcule la liste des jours fériés mobiles dans une année.
        /// </summary>
        /// <param name="year">L'année pour laquelle on cherche les jours fériés.</param>
        /// <param name="pentecostMondayAsHolliday">True si on souhaite ajouter le lundi de pentecôte  à la liste des jours fériés(par défaut), sinon false</param>
        /// <returns>Retourne un Hashset contenant les instances de DateTime représentant les jours fériers recherchés.</returns>
        private static HashSet<DateTime> ComputeEaster(int year, bool pentecostMondayAsHolliday = true)
        {
            int dom; // Nombre dominical
            int pfm; // Pleine lune de paques
            int solar; // Correction solaire
            int lunar; // Correction lunaire

            int goldenNumber = (year % 19) + 1;
            if (year <= 1752)
            {
                dom = (year + (year / 4) + 5) % 7;
                if (dom < 0) dom += 7;
                pfm = (3 - (11 * goldenNumber) - 7) % 30;
                if (pfm < 0) pfm += 30;
            }
            else
            {
                dom = (year + (year / 4) - (year / 100) + (year / 400)) % 7;
                if (dom < 0) dom += 7;
                solar = (year - 1600) / 100 - (year - 1600) / 400;
                lunar = (((year - 1400) / 100) * 8) / 25;
                pfm = (3 - (11 * goldenNumber) + solar - lunar) % 30;
                if (pfm < 0) pfm += 30;
            }

            // jours après le 21 mars (equinoxe de printemps)
            if ((pfm == 29) || (pfm == 28 && goldenNumber > 11))
                pfm--;
            int tmp = (4 - pfm - dom) % 7;
            if (tmp < 0)
                tmp += 7;

            int easter = pfm + tmp + 1;// Paques en nombre de jour apres le 21 mars
            DateTime easterDay = (easter < 11) ? DateTime.Parse((easter + 21) + "/3/" + year) : DateTime.Parse((easter - 10) + "/4/" + year);
            DateTime ascent = easterDay.AddDays(39);
            DateTime pentecostMonday = easterDay.AddDays(50);
            DateTime easterMonday = easterDay.AddDays(1);

            var computed = new HashSet<DateTime> { easterMonday, ascent };
            if (pentecostMondayAsHolliday)
                computed.Add(pentecostMonday);

            return computed;
        }

        #region Logsys business
        /// <summary>
        /// Obtient le N° de semaine de la date au format Iso8601, avec un décallage dû aux impératifs de préparation.
        /// </summary>
        /// <param name="dt">La date pour laquelle on veut connaître le N° de semaine.</param>
        /// <returns>Un entier représentant le N° de semaine.</returns>
        /// <remarks>Permet de rattacher les jours de la semaine à partir du vendredi 13h.</remarks>
        public static int GetStaggeredWeekNumber(this DateTime dt)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dt);
            DateTime date = dt;

            // Permet de décaler les vendredi (13h), samedi et dimanche à la semaine suivante
            if ((day == DayOfWeek.Friday && date.Hour >= 13) || day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                date = date.AddDays(3);

            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                date = date.AddDays(3);

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }



        public static DateTime GetWorkDay(this DateTime dt)
        {
            DateTime result;

            if (dt.Hour >= 13 && dt.DayOfWeek == DayOfWeek.Friday)
                result = dt.AddDays(3);
            else if (dt.DayOfWeek == DayOfWeek.Saturday)
                result = dt.AddDays(2);
            else if (dt.DayOfWeek == DayOfWeek.Sunday)
                result = dt.AddDays(1);
            else
                result = dt;

            return new DateTime(result.Year, result.Month, result.Day);
        }
        #endregion
    }
}
