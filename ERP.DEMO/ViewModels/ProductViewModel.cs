using System.ComponentModel.DataAnnotations;

namespace ERP.DEMO.ViewModels
{
    public class ProductCreateViewModel
    {
        [Display(Name = "Code")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le libellé est requis.")]
        [StringLength(255, ErrorMessage = "Le libellé ne peut pas dépasser 255 caractères.")]
        [Display(Name = "Libellé")]
        public string Label { get; set; } = string.Empty;

        [Display(Name = "Actif")]
        public bool IsActiv { get; set; } = true;

        [StringLength(8000, ErrorMessage = "La description ne peut pas dépasser 8000 caractères.")]
        public string? Description { get; set; }

        [Range(0, float.MaxValue, ErrorMessage = "La longueur doit être un nombre positif.")]
        [Display(Name = "Longueur (mm)")]
        public float? Length { get; set; }

        [Range(0, float.MaxValue, ErrorMessage = "La largeur doit être un nombre positif.")]
        [Display(Name = "Largeur (mm)")]
        public float? Width { get; set; }

        [Range(0, float.MaxValue, ErrorMessage = "La hauteur doit être un nombre positif.")]
        [Display(Name = "Hauteur (mm)")]
        public float? Height { get; set; }

        [Range(0, float.MaxValue, ErrorMessage = "Le poids doit être un nombre positif.")]
        [Display(Name = "Poids (g)")]
        public float? Weight { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le prix de vente doit être un nombre positif.")]
        [Display(Name = "Prix de vente (€)")]
        public decimal? Price { get; set; }


    }

    public static class ProductViewModelExtension
    {
        public static Models.TestDb.Product ToProduct(this ProductCreateViewModel viewModel)
        {
            return new Models.TestDb.Product
            {
                Id = viewModel.Id,
                Label = viewModel.Label,
                Description = viewModel.Description,
                Length = viewModel.Length,
                Width = viewModel.Width,
                Height = viewModel.Height,
                Weight = viewModel.Weight,
                Price = viewModel.Price,

            };
        }
    }

}