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
    /// Représente les utilisateurs dans l'application (Table User).
    /// </summary>
    [Table("User")]
    public class User
    {
        /// <summary>
        /// Obtient ou définit l'identifiant (clef primaire) d'un utilisateur (USR_ID).
        /// </summary>
        [Key, Column("USR_ID")/*, Identity(true)*/]
        public int Id { get; set; }

        /// <summary>
        /// Obtient ou définit le nom d'utilisateur d'un utilisateur (USR_USERNAME).
        /// </summary>
        [Required, StringLength(50), Column("USR_USERNAME")]
        public string Username { get; set; }

        /// <summary>
        /// Obtient ou définit le mot de passe d'un utilisateur (USR_PASSWORD).
        /// </summary>
        [Required, Column("USR_PASSWORD")]
        public string Password { get; set; }

        /// <summary>
        /// Obtient ou définit le nom de famille d'un utilisateur (USR_LASTNAME).
        /// </summary>
        [StringLength(50), Column("USR_LASTNAME")]
        public string? LastName { get; set; }

        /// <summary>
        /// Obtient ou définit le prénom d'un utilisateur (USR_FIRSTNAME).
        /// </summary>
        [StringLength(50), Column("USR_FIRSTNAME")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Obtient ou définit l'adresse email d'un utilisateur (USR_EMAIL).
        /// </summary>
        [StringLength(255), Column("USR_EMAIL")]
        public string? Email { get; set; }

        /// <summary>
        /// Obtient ou définit si l'utilisateur est actif (USR_ISACTIVE).
        /// </summary>
        [Required, Column("USR_ISACTIVE")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Obtient ou définit la date de dernière connexion d'un utilisateur (USR_LASTLOGINDATE).
        /// </summary>
        [Column("USR_LASTLOGINDATE")]
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Obtient ou définit la date de création d'un utilisateur (USR_CREATEDAT).
        /// </summary>
        [Column("USR_CREATEDAT")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Obtient ou définit la date de modification d'un utilisateur (USR_MODIFIEDAT).
        /// </summary>
        [Column("USR_MODIFIEDAT")]
        public DateTime? ModificationDate { get; set; }

        /// <summary>
        /// Obtient ou définit les préférences d'un utilisateur (USR_PREFERENCES).
        /// </summary>
        [StringLength(int.MaxValue), Column("USR_PREFERENCES")]
        public string? Preferences { get; set; }

        /// <summary>
        /// Obtient ou définit le rôle d'un utilisateur (USR_ROLE).
        /// </summary>
        [Required, Column("USR_ROLE")]
        public short Role { get; set; }


        #region Navigation properties
        // Ajoutez ici les propriétés de navigation si nécessaire
        #endregion
    }
}