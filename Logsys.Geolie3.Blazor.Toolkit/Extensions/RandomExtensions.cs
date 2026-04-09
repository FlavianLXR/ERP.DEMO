using System;
using System.Text;

namespace ERP.DEMO.Toolkit.Extensions
{
    public static class RandomExtentions
    {
        public enum CaseOption
        {
            /// <summary>
            /// Pas de lettres (digitRequired doit être true)
            /// </summary>
            None,
            /// <summary>
            /// Majuscules et minuscules
            /// </summary>
            Both,
            /// <summary>
            /// Minuscules
            /// </summary>
            Lower,
            /// <summary>
            /// Majuscules
            /// </summary>
            Upper
        }

        /// <summary>
        /// Génère une chaîne de caractères.
        /// </summary>
        /// <param name="length">Le nombre de caractères.</param>
        /// <param name="caseOption">Spécifie si la chaîne contient des majuscules, des minuscules, les deux ou aucune lettre.</param>
        /// <param name="digitRequired">Spécifie si la chaîne contient des chiffres.</param>
        /// <param name="digitRequired">Spécifie si la chaîne contient des caractères spéciaux.</param>
        /// <returns>La séquence générée, sous forme de chaîne.</returns>
        public static string NextString(this Random rnd, int length, CaseOption caseOption = CaseOption.Both, bool digitRequired = true, bool nonLetterOrDigit = false)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException("length", length, "La chaîne générée doit avoir au moins un caractère");
            if (caseOption == CaseOption.None && !digitRequired && !nonLetterOrDigit)
                throw new ArgumentException("La méthode ne peut pas générer une chaine vide.");

            string availableChars = string.Empty;

            switch (caseOption)
            {
                case CaseOption.Both:
                    availableChars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
                    break;
                case CaseOption.Lower:
                    availableChars = "abcdefghijklmnopqrstuvwxyz";
                    break;
                case CaseOption.Upper:
                    availableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    break;
                default:
                    break;
            }
            if (digitRequired) availableChars += "0123456789";
            if (nonLetterOrDigit) availableChars += "~&#{[,!?§]}";

            var randomizer = new Random();
            var generatedString = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                int idx = randomizer.Next(0, availableChars.Length - 1);
                generatedString.Append(availableChars.Substring(idx, 1));
            }
            return generatedString.ToString();
        }
    }
}
