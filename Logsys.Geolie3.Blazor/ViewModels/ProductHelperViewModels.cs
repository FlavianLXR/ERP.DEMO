using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.DEMO.ViewModels
{
    public class ProductHelperViewModels
    {
        public class StockViewModel
        {
            public int Id { get; set; }

            public string Label { get; set; }

            public List<RangeViewModel> Ranges { get; set; }
        }

        public class RangeViewModel
        {
            public int Id { get; set; }
            public int StockId { get; set; }

            public string Label { get; set; }

            public bool IsAssigned { get; set; }
        }
    }
}