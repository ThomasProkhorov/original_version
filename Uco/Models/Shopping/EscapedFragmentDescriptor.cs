using System;
using System.Linq;

namespace Uco.Models.Shopping
{
    public class EscapedFragmentDescriptor
    {
        public enum Targets
        {
            Category,
            Product
        }

        public Targets Target { get; private set; }
        public int ShopID { get; private set; }
        public int CategoryID { get; private set; }

        //public EscapedFragmentDescriptor(Targets target, int shopID, int categoryID)
        //{
        //    Target = target;
        //    ShopID = shopID;
        //    CategoryID = categoryID;
        //}

        public EscapedFragmentDescriptor(string fragmentPath)
        {
            if (string.IsNullOrEmpty(fragmentPath))
            {
                throw new ArgumentNullException("fragmentPath");
            }

            if (fragmentPath.StartsWith("#"))
            {
                fragmentPath = fragmentPath.Substring(1);
            }

            if (fragmentPath.Contains("?"))
            {
                fragmentPath = fragmentPath.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries).First();
            }

            string[] parts = fragmentPath.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                Target = Targets.Product;
                int productShopId = 0;
                int.TryParse(parts[1], out productShopId);
                ShopID = productShopId;
            }
            switch (parts[0].ToLowerInvariant())
            {
                case "category":

                    if (parts.Length > 2)
                    {
                        Target = Targets.Category;
                        ShopID = int.Parse(parts[1]);
                        CategoryID = int.Parse(parts[2]);
                    }
                    break;
                case "product":
                    if (parts.Length > 1)
                    {
                        Target = Targets.Product;
                        ShopID = int.Parse(parts[1]);
                    }
                    break;
               // default:
                   
            }
        }
    }
}