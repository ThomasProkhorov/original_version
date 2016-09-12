using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Infrastructure;
using Uco.Models;
using System.Data.Entity;
using Uco.Infrastructure.Repositories;
using System.IO;
using System.Text;
using System.Globalization;

using Uco.Infrastructure.EntityExtensions;
using Uco.Infrastructure.Livecycle;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;

namespace Uco.Areas.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class ProductShopController : BaseMemberController
    {
        public ActionResult Index()
        {
            return View(new ProductShop());
        }

        #region ImportExport
        private class PShopImportSheme
        {
            /// <summary>
            /// 0
            /// </summary>
            public int SKU = 0;
            /// <summary>
            /// 1
            /// </summary>
            public int Name = 1;
            /// <summary>
            /// 2
            /// </summary>
            public int Price = 2;
            /// <summary>
            /// 3
            /// </summary>
            public int Quantity = 3;
            /// <summary>
            /// 4
            /// </summary>
            public int VAT = 4;
            /// <summary>
            /// 5
            /// </summary>
            public int Options = 5;
            /// <summary>
            /// 6
            /// </summary>
            public int MaxToOrder = 6;
            /// <summary>
            /// 7
            /// </summary>
            public int IncludeInShipPrice = 7;
            ///// <summary>
            ///// 7
            ///// </summary>
            //public int Category = 7;
            /// <summary>
            /// 8
            /// </summary>
            public int QuantityChecking = 8;


        }

        public ActionResult RemoveNotMappedShortSku()
        {
            if (CurrentShop != null)
            {
                var ShopID = CurrentShop.ID;
                var sqlcommand = string.Format("DELETE FROM [ProductSkuMaps] WHERE [ShopID] = {0} AND ([ProductSKU] IS NULL OR [ProductSKU] = N'')", ShopID);
                var sqlsay = _db.Database.ExecuteSqlCommand(sqlcommand);
                return Content(RP.S("Member.ProductSkuMap.Removed") + " SQL: " + sqlcommand + " SQLRETURN: " + sqlsay.ToString());
            }
            return Content("Shop not found");
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase attachment, bool deleteold = false,bool useContains=false)
        {
            if (CurrentShop == null) return Json(new { success = "error", message = RP.S("Admin.ProductShop.Import.Error.NoShop") });
            if (attachment == null)
            {
                // TempData["MessageRed"] = "File Error";
                return Json(new { success = "error", message = RP.S("Admin.ProductShop.Import.Error.FileMissing") });
            }
            _db.Configuration.ValidateOnSaveEnabled = false;
            string FolderPath = Server.MapPath("~/App_Data/Temp/");
            //1
            string FileName = "";
            string FilePath = "";

            FileName = Path.GetFileName(attachment.FileName);
            FilePath = Path.Combine(FolderPath, FileName);
            attachment.SaveAs(FilePath);
            if (!FileName.EndsWith(".xls") && !FileName.EndsWith(".xlsx") && !FileName.EndsWith(".csv"))
            {
                return Json(new { success = "error", message = RP.S("Admin.ProductShop.Import.Error.FileExtensionMustBe.xls.xlsx.csv") });

            }

            var existingFile = new FileInfo(FilePath);
            var all = new List<Dictionary<int, string>>();
            int minSkuLength=5;
            #region read excell
            if (FileName.EndsWith(".xls"))
            {
                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    var wb = new HSSFWorkbook(fs);
                    // get sheet
                    var sh = (HSSFSheet)wb.GetSheetAt(0);
                    var head = sh.GetRow(0);
                    int colCount = 0;
                    if (head != null)
                    {
                        colCount = head.Cells.Count;
                    }
                    int i = 1;
                    while (sh.GetRow(i) != null)
                    {
                        var xlsRow = sh.GetRow(i);
                        // read some data
                        Dictionary<int, string> row = new Dictionary<int, string>();
                        // write row value
                        for (int j = 0; j < colCount; j++)
                        {
                            var cell = xlsRow.GetCell(j);

                            if (cell != null)
                            {

                                // TODO: you can add more cell types capability, e. g. formula
                                switch (cell.CellType)
                                {
                                    case NPOI.SS.UserModel.CellType.Numeric:
                                        var valN = cell.NumericCellValue;
                                        row[j] = valN.ToString();
                                        break;
                                    case NPOI.SS.UserModel.CellType.String:
                                        var val = cell.StringCellValue;
                                        row[j] = val != null ? val : "";
                                        break;
                                    default:
                                        row[j] = "";
                                        break;
                                }
                            }
                            else
                            {
                                row[j] = "";
                            }
                        }
                        all.Add(row);
                        i++;
                    }
                }
            }
            else
            {
                // Open and read the XlSX file.
                using (var package = new ExcelPackage(existingFile))
                {
                    // Get the work book in the file
                    ExcelWorkbook workBook = package.Workbook;
                    if (workBook != null)
                    {
                        if (workBook.Worksheets.Count > 0)
                        {
                            // Get the first worksheet
                            ExcelWorksheet currentWorksheet = workBook.Worksheets.First();

                            var begin = currentWorksheet.Dimension.Start;
                            var end = currentWorksheet.Dimension.End;
                            for (var i = begin.Row + 1; i <= end.Row; i++)
                            {

                                // read some data
                                Dictionary<int, string> row = new Dictionary<int, string>();
                                for (var c = begin.Column; c <= end.Column; c++)
                                {
                                    var val = currentWorksheet.Cells[i, c].Value;
                                    row[c - 1] = val != null ? val.ToString() : "";

                                }
                                all.Add(row);
                            }

                        }

                    }

                }
            }
            #endregion
            //Dictionary<string, string> skuImageMap = new Dictionary<string, string>();
            //Dictionary<int, string> idImageMap = new Dictionary<int, string>();
            var productforInsert = new List<ProductShop>();
            var productForUpdate = new List<ProductShop>();
            var productForDelete = new List<ProductShop>();
            //  var productForDelete = new List<Product>();
            var productsList = _db.Products
                    .OrderBy(x => x.ID)
                    .Select(x => new { x.ID, x.SKU, x.ProductShopOptions, x.CategoryID, x.UnitsPerPackage }) // optimization
                    .ToList().ToDictionary(x => x.SKU);
            int shopID = CurrentShop.ID;
            var shopCategories = _db.ShopCategories.Where(x => x.ShopID == shopID).ToList(); // LS.Get<ShopCategory>().Where(x => x.ShopID == shopID).ToList();
            var categories = LS.Get<Category>();
            var productShopList = _db.ProductShopMap
                .OrderBy(x => x.ID)
                .Where(x => x.ShopID == shopID)
                .Select(x => new { x.ID, x.ProductID, x.DontImportPrice }).ToList();
            var replaceSkuMap = _db.ProductSkuMaps.AsNoTracking().Where(x => x.ShopID == shopID).ToList();
            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            //  var AllCategories = LS.Get<Category>(); //get category cache
            //options container
            //Dictionary<int, 
            List<ProductAttributeOption> updateAttributes = new List<ProductAttributeOption>();
            Dictionary<int, List<ProductAttributeOption>> insertAttributes = new Dictionary<int, List<ProductAttributeOption>>();

            Dictionary<int, int> productShopIDMap = new Dictionary<int, int>();
            List<int> notDeleteListIds = new List<int>();
            List<int> notDeleteListSkuMapIds = new List<int>();

            //Run each excell
            StringBuilder resultMessage = new StringBuilder();
            List<ProcessError> processErrors = new List<ProcessError>();
            bool needAddCategory = true;
            int minCoulCount = 9;
            int curLine = 1;
            StringBuilder lastEmpty = new StringBuilder();
            bool createProductIfSkuNotExist = false;
            Dictionary<string, int> processed = new Dictionary<string, int>();
            var sheme = new PShopImportSheme();
            foreach (var s in all)
            {
                curLine++;
                bool allOk = true;
                if (curLine == 2) { continue; }//skip second row (excell numeration)
                if (s.Count < minCoulCount)
                {
                    processErrors.Add(new ProcessError()
                    {
                        LineNum = curLine
                        ,
                        Message = string.Format(RP.S("Admin.ProductShop.Import.Error.MissingColumns-ParameterLineNum{0}"), curLine)
                    });
                    resultMessage.AppendLine(string.Format(RP.S("Admin.ProductShop.Import.Error.MissingColumns-ParameterLineNum{0}"), curLine));
                    continue;
                }
                if (s[sheme.SKU] == null || s[sheme.SKU].ToString().Trim() == "")
                {
                    allOk = false;
                    processErrors.Add(new ProcessError()
                    {
                        LineNum = curLine
                        ,
                        Message = string.Format(RP.S("Admin.ProductShop.Import.Error.SKUEmpty-LineNume{0}"), curLine)
                    });
                    lastEmpty.AppendLine(string.Format(RP.S("Admin.ProductShop.Import.Error.SKUEmpty-LineNume{0}"), curLine));
                    continue;
                    //    continue; 
                }
                if (lastEmpty.Length > 0)
                {

                    resultMessage.AppendLine(lastEmpty.ToString());
                    lastEmpty.Clear();
                }
                var sku = s[sheme.SKU].ToString().Trim();//must be unique, check and validate
                sku = Regex.Replace(sku, "[^.0-9]", "");
                var originalSku = sku;
                //check map table
                var replaceTo = replaceSkuMap.FirstOrDefault(x => (
                    x.ShortSKU != null
                    && sku != null
                   && x.ShortSKU == sku)
                    );
                if (replaceTo != null && !string.IsNullOrEmpty(replaceTo.ProductSKU))
                {
                    sku = replaceTo.ProductSKU;
                    notDeleteListSkuMapIds.Add(replaceTo.ID);
                }
                else if (useContains && originalSku.Length>=minSkuLength)
                {
                    sku = productsList.Keys.FirstOrDefault(x => x.EndsWith(originalSku));
                    sku = string.IsNullOrEmpty(sku) ? originalSku : sku;
                }

                if(string.IsNullOrEmpty(sku) && string.IsNullOrEmpty(s[sheme.Name]))
                {
                    continue;
                }
                //check if not repeated row
                if ((sku != null && processed.ContainsKey(sku)) 
                  )
                {
                    processErrors.Add(new ProcessError()
                    {
                        LineNum = curLine
                        ,
                        SKU = sku
                        ,
                        Message = string.Format(RP.S("Admin.Product.Import.Error.SKUAlreadyProcessed-LineNume{0}"), curLine) 
                        + " (Row: " + processed [sku].ToString()+ ")"
                    });
                    resultMessage.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.SKUAlreadyProcessed-LineNume{0}"), curLine) + " (Row: " + processed[sku].ToString() + ")");

                    continue;
                }
                var priceStr = s[sheme.Price].ToString().Trim();
                decimal productPrice = 0;
                int stepQuantity = 1000;
                if (!int.TryParse(s[sheme.Quantity].Trim(), out stepQuantity))
                {
                    stepQuantity = 1000;
                }
                try
                {
                    productPrice = Convert.ToDecimal(priceStr.Replace(".", decimalSeparator).Replace(",", decimalSeparator));

                }
                catch
                { }
                if (sku != null)
                {
                    processed[sku] = curLine;
                }
                else
                {
                    processed[s[sheme.Name]] = curLine;
                }
                int temp = 0;
                if (sku == null || !productsList.ContainsKey(sku))
                {
                    //add to map if not exists
                    if (!replaceSkuMap.Any(x => (
                     x.ShortSKU != null
                     && originalSku != null
                    && x.ShortSKU == originalSku)
                     ))
                    {
                        var newMapSkuSlk = new ProductSkuMap()
                        {
                            // ProductSKU = ,
                            ShortSKU = originalSku,
                            ShopID = shopID,
                            Price = productPrice,
                            ImportProductName = s[sheme.Name],
                            Quantity = stepQuantity
                        };
                        _db.ProductSkuMaps.Add(newMapSkuSlk);
                        _db.SaveChanges();
                        replaceSkuMap.Add(newMapSkuSlk);
                    }
                    else
                    {


                        notDeleteListSkuMapIds.Add(replaceSkuMap.FirstOrDefault(x => (
                     x.ShortSKU != null
                     && originalSku != null
                    && x.ShortSKU == originalSku)
                    // || (x.ShortSKU == null && x.ImportProductName != null && s[sheme.Name] != null
                    // && x.ImportProductName == s[sheme.Name])
                     ).ID);
                    }
                    //process insert new product (parent)
                    if (!createProductIfSkuNotExist)
                    {

                        allOk = false;
                        processErrors.Add(new ProcessError()
                        {
                            LineNum = curLine
                            ,
                            SKU = sku,
                            Message = string.Format(RP.S("Admin.ProductShop.Import.Error.SkuNotExists-ParameterLineNum{0}"), curLine)
                        });
                        resultMessage.AppendLine(string.Format(RP.S("Admin.ProductShop.Import.Error.SkuNotExists-ParameterLineNum{0}"), curLine));
                        continue;
                    }
                    else
                    {
                        var productSku = new Product()
                        {
                            SKU = sku,
                            Name = sku,

                        };
                        _db.Products.Add(productSku);
                        _db.SaveChanges();
                        productsList.Add(sku, new
                        {
                            productSku.ID,
                            productSku.SKU
                            ,
                            productSku.ProductShopOptions,
                            productSku.CategoryID,
                            productSku.UnitsPerPackage
                        });
                    }
                    // resultMessage.AppendLine(string.Format(RP.S("Admin.ProductShop.Import.Error.NoProductWithSKU-ParameterSKU"), sku));
                    //  continue; // no product
                }
                replaceTo = replaceSkuMap.FirstOrDefault(x => (
                    x.ShortSKU != null
                    && originalSku != null
                   && x.ShortSKU == originalSku)
                  //  || (x.ShortSKU == null && x.ImportProductName != null && s[sheme.Name] != null
                   // && x.ImportProductName == s[sheme.Name])
                    );

                if (replaceTo == null)
                {
                    var newMapSkuSlk = new ProductSkuMap()
                    {
                        ProductSKU = productsList[sku].SKU,
                        ShortSKU = originalSku,
                        ShopID = shopID,
                        Price = productPrice,
                        ImportProductName = s[sheme.Name],
                        Quantity = stepQuantity,
                        Imported = true
                    };
                    _db.ProductSkuMaps.Add(newMapSkuSlk);
                    _db.SaveChanges();
                    replaceSkuMap.Add(newMapSkuSlk);
                }
                else
                {
                    notDeleteListSkuMapIds.Add(replaceTo.ID);
                }
                var pID = productsList[sku].ID;
                var defaultOptions = productsList[sku].ProductShopOptions;
                var categoryID = productsList[sku].CategoryID;

                // if (priceStr == "")
                // {
                //    priceStr = "0";
                // }
                var product = new ProductShop()
                {
                    ProductID = pID,
                    ShopID = shopID,

                    IncludeVat = true,
                    IncludeInShipPrice = true

                };
                try
                {
                    product.Price = Convert.ToDecimal(priceStr.Replace(".", decimalSeparator).Replace(",", decimalSeparator));
                    if (productsList[sku].UnitsPerPackage.HasValue && productsList[sku].UnitsPerPackage.Value > 0)
                    {
                        //price by one unit
                        product.PriceByUnit = product.Price / productsList[sku].UnitsPerPackage.Value;
                    }
                }
                catch (Exception e)
                {
                    allOk = false;
                    processErrors.Add(new ProcessError()
                    {
                        LineNum = curLine
                        ,
                        SKU = sku,
                        Message = string.Format(RP.S("Admin.ProductShop.Import.Error.BadPrice-ParameterLineNum{0}"), curLine)
                    });
                    resultMessage.AppendLine(string.Format(RP.S("Admin.ProductShop.Import.Error.BadPrice-ParameterLineNum{0}"), curLine));
                    // continue;
                }
                if (s[sheme.VAT].ToString().Trim() == "0" || s[sheme.VAT].ToString().Trim() == "false"
                    || s[sheme.VAT].ToString().Trim() == "False")
                {
                    product.IncludeVat = false;
                }
                if (s[sheme.IncludeInShipPrice].ToString().Trim() == "0"
                    || s[sheme.IncludeInShipPrice].ToString().Trim() == "false"
                    || s[sheme.IncludeInShipPrice].ToString().Trim() == "False")
                {
                    product.IncludeInShipPrice = false;
                }
                product.QuantityType = ProductQuantityType.NotCheck;
                if (s[sheme.QuantityChecking].ToString().Trim() == "1")
                {
                    product.QuantityType = ProductQuantityType.CheckByProduct;
                }
                else if (s[sheme.QuantityChecking].ToString().Trim() == "2")
                {
                    product.QuantityType = ProductQuantityType.CheckByProductOptions;
                }
                //step for quantity change
                int step = 1000;
                if (!string.IsNullOrEmpty(s[sheme.Quantity]))
                {

                    bool valid = int.TryParse(s[sheme.Quantity].ToString().Trim(), out step);
                    if (!valid)
                    {
                        step = 1000;
                    }
                    else
                    {
                        product.QuantityType = ProductQuantityType.CheckByProduct;
                    }

                }
                product.Quantity = step;
                if (!string.IsNullOrEmpty(s[sheme.MaxToOrder].ToString().Trim()))
                {
                    step = 0;
                    bool valid = int.TryParse(s[sheme.MaxToOrder].ToString().Trim(), out step);
                    if (step > 0)
                        product.MaxCartQuantity = step;
                    if (!valid)
                    {
                        allOk = false;
                        processErrors.Add(new ProcessError()
                        {
                            LineNum = curLine
                            ,
                            SKU = sku,
                            Message = string.Format(RP.S("Admin.ProductShop.Import.Error.MaxCartQuantity-ParameterLineNum{0}"), curLine)
                        });
                        resultMessage.AppendLine(string.Format(RP.S("Admin.ProductShop.Import.Error.MaxCartQuantity-ParameterLineNum{0}"), curLine));

                    }
                }
                if (!allOk)
                {
                    continue;
                }
                var psh = productShopList.FirstOrDefault(x => x.ProductID == pID);
                if (psh != null)
                {
                    if (product.Price == 0 || (product.Quantity == 0))
                    //&& product.QuantityType != ProductQuantityType.NotCheck) )
                    {
                        //delete product
                        productForDelete.Add(new ProductShop() { ID = psh.ID });
                        continue;
                    }
                    if (psh.DontImportPrice)
                    {
                        product.Price = 0;//don`t import price
                    }
                    product.ID = psh.ID;
                    productForUpdate.Add(product);
                    productShopIDMap[pID] = psh.ID;
                    if (deleteold)
                    {
                        notDeleteListIds.Add(psh.ID);
                    }
                }
                else
                {
                    if (product.Price == 0 || (product.Quantity == 0))
                    //&& product.QuantityType != ProductQuantityType.NotCheck))
                    {
                        //don`t import if price 0
                        continue;
                    }
                    product.CreateDate = DateTime.Now;
                    productforInsert.Add(product);
                }

                //category check
                if (categoryID > 0)
                {
                    if (!shopCategories.Any(x => x.CategoryID == categoryID))
                    {
                        //create and add
                        var shopCat = new ShopCategory()
                        {
                            CategoryID = categoryID,
                            DisplayOrder = 1000,
                            Published = true,
                            ShopID = shopID
                        };
                        _db.ShopCategories.Add(shopCat);
                        shopCategories.Add(shopCat);
                        //check if parent cat in shop map
                        var cat = categories.FirstOrDefault(x => x.ID == categoryID);
                        if (cat != null && cat.ParentCategoryID > 0)
                        {

                            int parentCategoryID = cat.ParentCategoryID;
                            if (!shopCategories.Any(x => x.CategoryID == parentCategoryID))
                            {
                                //create and add
                                var shopCatParent = new ShopCategory()
                                {
                                    CategoryID = parentCategoryID,
                                    DisplayOrder = 1000,
                                    Published = true,
                                    ShopID = shopID
                                };
                                _db.ShopCategories.Add(shopCatParent);
                                shopCategories.Add(shopCatParent);
                            }

                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        //update visibility
                        var ct = shopCategories.FirstOrDefault(x => x.CategoryID == categoryID);
                        if (ct != null && !ct.Published)
                        {
                            ct.Published = true;
                            //check parent
                            var cat = categories.FirstOrDefault(x => x.ID == categoryID);
                            if (cat != null && cat.ParentCategoryID > 0)
                            {
                                int parentCategoryID = cat.ParentCategoryID;
                                if (!shopCategories.Any(x => x.CategoryID == parentCategoryID))
                                {
                                    //create and add
                                    var shopCatParent = new ShopCategory()
                                    {
                                        CategoryID = parentCategoryID,
                                        DisplayOrder = 1000,
                                        Published = true,
                                        ShopID = shopID
                                    };
                                    _db.ShopCategories.Add(shopCatParent);
                                    shopCategories.Add(shopCatParent);
                                }
                                else
                                {
                                    var prcat = shopCategories.FirstOrDefault(x => x.CategoryID == parentCategoryID);
                                    if (prcat != null && !prcat.Published)
                                    {
                                        prcat.Published = true;
                                    }
                                }

                            }

                            _db.SaveChanges();
                        }
                    }
                }
                else
                {
                    product.NotInCategory = true;
                    if (needAddCategory)//run only one time
                    {
                        var otherCategory = categories.FirstOrDefault(x => x.Name == "מוצרים נוספים" && x.ParentCategoryID == 0);
                        if (otherCategory == null)
                        {
                            otherCategory = new Category()
                            {
                                DisplayOrder = 1000000,
                                Name = "מוצרים נוספים",
                                Published = true,

                            };
                            _db.Categories.Add(otherCategory);
                            _db.SaveChanges();
                            categories.Add(otherCategory);

                        }
                        var catshopmap = shopCategories.FirstOrDefault(x => x.CategoryID == otherCategory.ID);
                        if (catshopmap == null)
                        {
                            var otherShopCategory = new ShopCategory()
                            {
                                CategoryID = otherCategory.ID,
                                DisplayOrder = 1000000,
                                Published = true,
                                ShopID = shopID
                            };
                            _db.ShopCategories.Add(otherShopCategory);
                            _db.SaveChanges();
                        }
                        else
                        {
                            if (!catshopmap.Published)
                            {
                                var catshopMapAttached = _db.ShopCategories.FirstOrDefault(x => x.ID == catshopmap.ID);
                                if (catshopMapAttached != null)
                                {
                                    catshopMapAttached.Published = true;
                                    _db.SaveChanges();
                                }
                            }
                        }
                        needAddCategory = false;
                    }

                }

                //options
                string options = s[sheme.Options].ToString().Trim();
                if (string.IsNullOrEmpty(options))
                {
                    //default options
                    options = defaultOptions;
                }
                if (!string.IsNullOrEmpty(options))
                {
                    string[] values = options.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var listattr = new List<ProductAttributeOption>();
                    foreach (var o in values)
                    {
                        ProductAttributeOption pao = new ProductAttributeOption()
                        {
                            Name = o,

                            ProductShopID = product.ID
                        };
                        listattr.Add(pao);
                    }
                    if (psh != null)
                    {
                        updateAttributes.AddRange(listattr);
                    }
                    else
                    {
                        insertAttributes.Add(pID, listattr);
                    }
                }

            }
            string message = RP.S("Admin.ProductShop.Import.SeeError");
            if (true)// || resultMessage.Length < 3)
            {
                productForUpdate.SqlUpdateById(false);
                int lastID = _db.ProductShopMap.Select(x => x.ID).DefaultIfEmpty().Max();
                productforInsert.SqlInsert();
                productForDelete.SqlDeleteById();
                //run options insert update
                //1) get last product shop ID and get list of products

                if (deleteold)
                {
                    List<ProductShop> deleteThis = new List<ProductShop>();
                    foreach (var pshID in productShopList)
                    {
                        if (!notDeleteListIds.Contains(pshID.ID))
                        {
                            deleteThis.Add(new ProductShop()
                            {
                                ID = pshID.ID
                            });
                        }
                    }
                    deleteThis.SqlDeleteById();

                    

                         List<ProductSkuMap> deleteThisMap = new List<ProductSkuMap>();
                    foreach (var mapID in replaceSkuMap)
                    {
                        if (!notDeleteListSkuMapIds.Contains(mapID.ID))
                        {
                            deleteThisMap.Add(new ProductSkuMap()
                            {
                                ID = mapID.ID
                            });
                        }
                    }
                    deleteThisMap.SqlDeleteById();
                }

                var psmap = _db.ProductShopMap
                    .Where(x => x.ID > lastID && x.ShopID == shopID)
                    .Select(x => new { x.ID, x.ProductID }).ToList();

                foreach (var it in psmap)
                {
                    productShopIDMap[it.ProductID] = it.ID;
                }
                //.ToDictionary(x => x.ProductID, x => x.ID)
                //productShopIDMap.AddRange(

                //    );
                // run options merge
                foreach (var o in insertAttributes)
                {
                    if (productShopIDMap.ContainsKey(o.Key))
                    {
                        int productShopID = productShopIDMap[o.Key];
                        o.Value.ForEach((x) =>
                        {
                            x.ProductShopID = productShopID;

                            updateAttributes.Add(x);
                        });
                    }
                }
                insertAttributes.Clear();
                //runn options insert or update
                var containsFilterList = updateAttributes.Select(x => x.ProductShopID).Distinct().ToList();
                var existsAttributes = _db.ProductAttributeOptions.Where(x => containsFilterList.Contains(x.ProductShopID)).ToList();
                var forInsertOptions = new List<ProductAttributeOption>();
                object _lock = new object();//for lock multithread action
                Parallel.ForEach(updateAttributes, x =>
                {
                    var cur = existsAttributes.FirstOrDefault(y => y.ProductShopID == x.ProductShopID && y.Name == x.Name);
                    if (cur != null)
                    {
                        //update
                    }
                    else
                    {
                        //insert
                        lock (_lock)
                        {
                            forInsertOptions.Add(x);
                        }
                    }
                });
                //and insert options
                forInsertOptions.SqlInsert();
                message = RP.S("Admin.ProductShop.Import.Success");
                //add errors to table
                try
                {
                    foreach (var procError in processErrors)
                    {
                        procError.CreateOn = DateTime.Now;
                        procError.FileServiceName = FileName;
                        procError.IP = LS.GetUser_IP(Request);
                        procError.PageUrl = Request.RawUrl;
                        procError.RefererUrl = Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null;
                        procError.ShopID = shopID;
                        procError.UserID = LS.CurrentUser.ID;
                    }
                    processErrors.SqlInsert();
                }
                catch
                {

                }
            }


            return Json(new { success = "ok", message = message, errors = resultMessage.ToString() });

        }
        #endregion
    }
}
