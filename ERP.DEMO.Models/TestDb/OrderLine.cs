using ERP.DEMO.Models.TestDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ERP.DEMO.Toolkit.Extensions.BooleanExtensions;

namespace ERP.DEMO.Models.TestDb
{
    /// <summary>
    /// Représente les lignes de commande dans l'application (Table OrderLine).
    /// </summary>
    [Table("OrderLine")]
    public class OrderLine
    {
        /// <summary>
        /// Obtient ou définit l'identifiant (clef primaire) d'une ligne de commande (OL_ID).
        /// </summary>
        [Key, Column("OL_ID")]
        public int Id { get; set; }

        /// <summary>
        /// Obtient ou définit l'identifiant de la commande associée à la ligne de commande (OL_ORDERID).
        /// </summary>
        [ForeignKey("Order"), Column("OL_ORDERID")]
        public int OrderId { get; set; }

        /// <summary>
        /// Obtient ou définit l'identifiant du produit associé à la ligne de commande (OL_PRODUCTID).
        /// </summary>
        [ForeignKey("Product"), Column("OL_PRODUCTID")]
        public int ProductId { get; set; }

        /// <summary>
        /// Obtient ou définit la quantité d'une ligne de commande (OL_QUANTITY).
        /// </summary>
        [Required, Column("OL_QUANTITY")]
        public int Quantity { get; set; }

        #region Navigation properties
        // Ajoutez ici les propriétés de navigation si nécessaire
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        #endregion
    }
}