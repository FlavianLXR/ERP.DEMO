using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ERP.DEMO.Toolkit.Extensions.BooleanExtensions;
using ERP.DEMO.Models.TestDb;

namespace ERP.DEMO.Models.TestDb
{
    /// <summary>
    /// Représente les produits dans l'application (Table Product).
    /// </summary>
    [Table("Product")]
    public class Product
    {
        /// <summary>
        /// Obtient ou définit l'identifiant (clef primaire) d'un produit (ART_ID).
        /// </summary>
        [Key, Column("ART_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Obtient ou définit le label d'un produit (ART_LABEL).
        /// </summary>
        [Required, StringLength(200), Column("ART_LABEL")]
        public string Label { get; set; }

        /// <summary>
        /// Obtient ou définit la description d'un produit (ART_DESCRIPTION).
        /// </summary>
        [StringLength(int.MaxValue), Column("ART_DESCRIPTION")]
        public string? Description { get; set; }

        /// <summary>
        /// Obtient ou définit le prix d'un produit (ART_PRICE).
        /// </summary>
        [Required, Column("ART_PRICE")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Obtient ou définit la quantité d'un produit (ART_QUANTITY).
        /// </summary>
        [Column("ART_QUANTITY")]
        public int? Quantity { get; set; } = 0;

        /// <summary>
        /// Obtient ou définit si le produit est actif (ART_ISACTIVE).
        /// </summary>
        [Column("ART_ISACTIVE")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Obtient ou définit la longueur d'un produit (ART_LENGTH).
        /// </summary>
        [Column("ART_LENGTH")]
        public double? Length { get; set; }

        /// <summary>
        /// Obtient ou définit le poids d'un produit (ART_WEIGHT).
        /// </summary>
        [Column("ART_WEIGHT")]
        public double? Weight { get; set; }

        /// <summary>
        /// Obtient ou définit la hauteur d'un produit (ART_HEIGHT).
        /// </summary>
        [Column("ART_HEIGHT")]
        public double? Height { get; set; }

        /// <summary>
        /// Obtient ou définit la largeur d'un produit (ART_WIDTH).
        /// </summary>
        [Column("ART_WIDTH")]
        public double? Width { get; set; }

        /// <summary>
        /// Obtient ou définit la date de création d'un produit (ART_CREATEDAT).
        /// </summary>
        [Column("ART_CREATEDAT")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Obtient ou définit l'utilisateur qui a créé un produit (ART_CREATEDBY).
        /// </summary>
        [Required, Column("ART_CREATEDBY")]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Obtient ou définit la date de modification d'un produit (ART_MODIFIEDAT).
        /// </summary>
        [Column("ART_MODIFIEDAT")]
        public DateTime ModificationDate { get; set; }

        /// <summary>
        /// Obtient ou définit l'utilisateur qui a modifié un produit (ART_MODIFIEDBY).
        /// </summary>
        [Column("ART_MODIFIEDBY")]
        public int? ModifiedBy { get; set; }

        #region Navigation properties
        // Ajoutez ici les propriétés de navigation si nécessaire

        /// <summary>
        /// Obtient ou définit les articles inclus dans une commande.
        /// </summary>
        [Display(Name = "Lignes de commande")]
        public virtual ICollection<TestDb.OrderLine>? OrderLines { get; set; }
        #endregion
    }
}