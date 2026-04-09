using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.DEMO.Toolkit.Extensions
{
    public static class BooleanExtensions
    {
        /// <summary>
        /// Représente un booléen à trois états prenant en compte l'état non défini (différent de null).
        /// </summary>
        /// <remarks>Cette énumération est utiliser pour récupérer l'état de bouton radio.</remarks>
        public enum ThreeStateBool { False, Undefined, True };
    }
}
