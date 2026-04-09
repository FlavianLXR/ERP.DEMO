using ERP.DEMO.Models.TestDb;
using ERP.DEMO.Toolkit.Extensions;
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
    /// Représente les commandes dans l'application (Table Order).
    /// </summary>
    [Table("Order")]
    public class Order
    {
        /// <summary>
        /// Obtient ou définit l'identifiant (clef primaire) d'une commande (OR_ID).
        /// </summary>
        [Key, Column("OR_ID")]
        public int Id { get; set; }

        /// <summary>
        /// Obtient ou définit le label d'une commande (OR_LABEL).
        /// </summary>
        [Required, StringLength(150), Column("OR_LABEL")]
        public string Label { get; set; }

        /// <summary>
        /// Obtient ou définit l'identifiant de l'utilisateur associé à la commande (OR_USERID).
        /// </summary>
        [Required, Column("OR_USERID")]
        public int UserId { get; set; }

        /// <summary>
        /// Obtient ou définit le code du status de la commande (OR_STATUS).
        /// </summary>
        [Column("OR_STATUS", TypeName = "int"), Display(Name = "Statut")]
        public OrderStatusList? Status { get; set; }

        /// <summary>
        /// Obtient ou définit si une commande a une priorité (OR_ISPRIORITY).
        /// </summary>
        [Column("OR_ISPRIORITY")]
        public bool IsPriority { get; set; } = false;

        /// <summary>
        /// Obtient ou définit les notes de livraison d'une commande (OR_DELIVERYNOTES).
        /// </summary>
        [StringLength(int.MaxValue), Column("OR_DELIVERYNOTES")]
        public string? DeliveryNotes { get; set; }

        /// <summary>
        /// Obtient ou définit la date de livraison planifiée d'une commande (OR_PLANNEDDELIVERYDATE).
        /// </summary>
        [Column("OR_PLANNEDDELIVERYDATE")]
        public DateTime? PlannedDeliveryDate { get; set; }

        /// <summary>
        /// Obtient ou définit la date de création d'une commande (OR_CREATEDAT).
        /// </summary>
        [Column("OR_CREATEDAT")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Obtient ou définit l'utilisateur qui a créé une commande (OR_CREATEDBY).
        /// </summary>
        [Required, Column("OR_CREATEDBY")]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Obtient ou définit la date de modification d'une commande (OR_MODIFIEDDAT).
        /// </summary>
        [Column("OR_MODIFIEDDAT")]
        public DateTime ModificationDate { get; set; }

        /// <summary>
        /// Obtient ou définit l'utilisateur qui a modifié une commande (OR_MODIFIEDBY).
        /// </summary>
        [Column("OR_MODIFIEDBY")]
        public int? ModifiedBy { get; set; }

        #region Navigation properties
        // Ajoutez ici les propriétés de navigation si nécessaire
        public virtual User? User { get; set; }
        public virtual ICollection<OrderLine> OrderLines { get; set; }
        #endregion

        public enum OrderStatusList
        {
            [Description("Attente de validation"), Order(1)]
            AwaitValidation = -3,
            [Description("En création"), Order(2)]
            Creating = -1,
            [Description("En préparation"), Order(3)]
            Preparation = 0,
            [Description("En instance"), Order(4)]
            Pending = 3,
            [Description("En livraison"), Order(5)]
            Deliver = 1,
            [Description("Livré conforme"), Order(6)]
            Delivered = 4,
            [Description("Livré non conforme"), Order(7)]
            UncompliantDelivery = 5,
            [Description("Ouverture d'enquête"), Order(8)]
            InvestigationOpened = 60,
            [Description("Incident en cours"), Order(9)]
            Incident = 2,
            [Description("Perdu"), Order(10)]
            Lost = 6,
            [Description("Retour"), Order(11)]
            Returned = 7,
            [Description("En anomalie"), Order(12)]
            Anomaly = -2,
            [Description("Annulé"), Order(13)]
            Cancelled = -4,
            [Description("Divers"), Order(14)]
            Undefined = -99
        }
    }
}