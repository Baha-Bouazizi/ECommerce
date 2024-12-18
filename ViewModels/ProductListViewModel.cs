using CommerceElectronique.Models;
using System.Collections.Generic;

namespace CommerceElectronique.ViewModels
{
    
    
        public class ProductListViewModel
        {
            public IEnumerable<Product> Products { get; set; }
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
        }
    }


