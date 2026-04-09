using System;
using System.Reflection;
//using System.Web.Mvc;

namespace ERP.DEMO.Toolkit.MvcTools
{
    /// <summary>
    /// Permet de positionner plusieurs boutons submit dans un écran.
    /// </summary>
    /// <example>Utiliser l'attribut [MultipleButton(Name = "action", Argument = "Search")] sur la méthode du controller et les attribut name="action:Search" value="Search" sur le bouton</example>
    //[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    //public sealed class MultipleButtonAttribute : ActionNameSelectorAttribute
    //{
    //    public string Name { get; set; }
    //    public string Argument { get; set; }

    //    public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
    //    {
    //        var isValidName = false;
    //        var keyValue = string.Format("{0}:{1}", Name, Argument);
    //        var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

    //        if (value != null)
    //        {
    //            controllerContext.Controller.ControllerContext.RouteData.Values[Name] = Argument;
    //            isValidName = true;
    //        }

    //        return isValidName;
    //    }
    //}
}
