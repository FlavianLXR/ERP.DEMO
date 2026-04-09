using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.DEMO.Models.TestDb
{
    /// <summary>
    /// Représente un type d'article dans l'application (table TYPESARTICLES).
    /// </summary>
    [Table("TYPESARTICLES")]
    public class Type
    {
        /// <summary>
        /// Obtient ou définit l'identifiant du type d'articles (TART_ID).
        /// </summary>
        [Key, Column("TART_ID"), Display(Name = "Identifiant Type d'article")]
        public int Id { get; set; }

        /// <summary>
        /// Obtient ou définit le libellé du type d'article (TART_LIBELLE).
        /// </summary>
        [/*Required,*/ StringLength(50), Column("TART_LIBELLE"), Display(Name = "Type d'article")]
        public string Label { get; set; }

        /// <summary>
        /// Obtient ou définit si le type d'article est actif (TART_ACTIF).
        /// </summary>
        [Column("TART_ACTIF"), Display(Name = "Actif")]
        public bool IsActive { get; set; }

        #region Navigation Properties
        /// <summary>
        /// Obtient ou définit la liste des articles du type de l'instance courrante.
        /// </summary>
        [Display(Name = "Articles")]
        public virtual ICollection<Product>? Products { get; set; }
        #endregion
    }
}