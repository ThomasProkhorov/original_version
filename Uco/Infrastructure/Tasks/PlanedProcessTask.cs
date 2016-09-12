
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using Uco.Infrastructure.EntityExtensions;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;

namespace Uco.Infrastructure.Tasks
{
    public class PlanedProcessTask : ITask
    {

        public string Title { get { return "PlanedProcessTask"; } }
        public int StartSeconds { get { return 10; } }
        public int IntervalSecondsFrom { get { return 60; } }
        public int IntervalSecondsTo { get { return 61; } }
        public void Execute()
        {
            using (Db _db = new Db())
            {
                var plans = _db.PlanedTasks.Where(x => !x.Started && x.Active).ToList();
                foreach (var action in plans)
                {
                    action.Started = true;
                    action.Message = RP.S("Admin.PlanedTask.Started");
                    action.LastStart = DateTime.UtcNow;
                    action.LastEnd = null;
                    _db.SaveChanges();
                    StartNewInThread(action.SystemName);
                }

            }
        }
        public void StartNewInThread(string SystemName)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                string message = RP.S("Admin.PlanedTask.Success");
                int? progress = 100;
                try
                {
                    Run(SystemName);
                }
                catch (Exception e)
                {
                    message = RP.S("Admin.PlanedTask.Error");
                    var exception = e;
                    while (exception != null)
                    {
                        message += " " + exception.Message + Environment.NewLine + exception.StackTrace + " ";
                        exception = exception.InnerException;
                    }
                    progress = null;
                }
                using (Db _db = new Db())
                {
                    var currentTask = _db.PlanedTasks.FirstOrDefault(x => x.SystemName == SystemName);
                    if (currentTask != null)
                    {
                        currentTask.Active = false;
                        currentTask.Started = false;

                        currentTask.Message = message;
                        if (progress.HasValue)
                        {
                            currentTask.PercentProgress = progress.Value;
                            currentTask.LastEnd = DateTime.UtcNow;

                        }
                        _db.SaveChanges();
                    }
                }
            });
        }
        public void Run(string SystemName)
        {
            MethodInfo method = typeof(PlanedProcessTask).GetMethod(SystemName, BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(null, new object[] { SystemName });
            }
            else
            {
                throw new Exception(RP.S("Admin.PlanedTask.MethodNotFound"));
            }
        }

        #region Import Prodct Admin

        #region util import
        private class PIsheme
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
            public int SubCategory = 2;
            /// <summary>
            /// 3
            /// </summary>
            public int SoldByWeight = 3;
            /// <summary>
            /// 4
            /// </summary>
            public int Capacity = 4;
            /// <summary>
            /// 5
            /// </summary>
            public int MeasureUnit = 5;
            /// <summary>
            /// 6
            /// </summary>
            public int UnitPerPackage = 6;
            /// <summary>
            /// 7
            /// </summary>
            public int Manufacturer = 7;
            /// <summary>
            /// 8
            /// </summary>
            public int Tax = 8;
            /// <summary>
            /// 9
            /// </summary>
            public int MadeCountry = 9;

            /// <summary>
            /// 10
            /// </summary>
            public int IsKosher = 10;
            /// <summary>
            /// 11
            /// </summary>
            public int KosherType = 11;


            /// <summary>
            /// 12
            /// </summary>
            public int Components = 12;
            /// <summary>
            /// 13
            /// </summary>
            public int ShortDescription = 13;
            /// <summary>
            /// 14
            /// </summary>
            public int FullDescription = 14;
            /// <summary>
            /// 15
            /// </summary>
            public int ProductOptions = 15;
            /// <summary>
            /// 16 (0, 1, 2, 3)
            /// </summary>
            public int FlagOperation = 16;
            ///// <summary>
            ///// 7
            ///// </summary>
            //public int Category = 7;



            ///// <summary>
            ///// 11
            ///// </summary>
            //public int Price = 11;
            ///// <summary>
            ///// 12
            ///// </summary>
            //     public int SoldByWeight = 12;

            public int UnitOfMeasure = 17;
            public int QuantityUnitOfMeasure = 18;



        }
        #endregion
        public static void ProductAdminImport(string SystemName)
        {
            using (Db _db = new Db())
            {
                var currentTask = _db.PlanedTasks.FirstOrDefault(x => x.SystemName == SystemName);
                if (currentTask != null)
                {
                    currentTask.Message = RP.S("Admin.PlanedTask.InProgress");
                    currentTask.PercentProgress = 10;
                    _db.SaveChanges();

                    if (string.IsNullOrEmpty(currentTask.ProcessData))
                    {
                        return;
                    }
                    #region Import

                    string FolderPath = HostingEnvironment.MapPath("~/App_Data/Import/");
                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }
                    //1
                    string FileName = "";
                    string FilePath = "";

                    FileName = Path.GetFileName(currentTask.ProcessData);
                    FilePath = Path.Combine(FolderPath, FileName);
                    if (!File.Exists(FilePath))
                    {
                        throw new Exception(RP.S("Admin.Product.Import.FileNotFound"));

                    }
                    var existingFile = new FileInfo(FilePath);
                    var all = new List<Dictionary<int, string>>();
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
                    //Dictionary<string, string> skuImageMap = new Dictionary<string, string>();
                    //Dictionary<int, string> idImageMap = new Dictionary<int, string>();
                    var productforInsert = new List<Product>();
                    var productForUpdate = new List<Product>();
                    var productForUpdateVariable = new List<ProductVariable<Product>>();
                    var productForDelete = new List<Product>();
                    var productsList = _db.Products
                            .OrderBy(x => x.ID)
                            .Select(x => new
                            {
                                x.ID,
                                x.SKU,
                                x.IgnoreOnImport
                                ,
                                x.Name,
                                x.IsKosher,
                                x.KosherType,
                                x.Capacity,
                                x.MeasureUnit,

                                x.Components,
                                x.ProductMeasureID,
                                x.UnitsPerPackage,
                                x.CategoryID,
                                x.ProductManufacturerID,
                                x.NoTax,
                                // x.RecomendedPrice
                                // ,
                                x.MadeCoutry,
                                x.SoldByWeight,
                                x.ShortDescription,
                                x.FullDescription
                                ,
                                x.ProductShopOptions,
                                x.ContentUnitMeasureID,
                                x.ContentUnitPriceMultiplicator
                            }) // optimization
                            .ToList().ToDictionary(x => x.SKU);
                    // var replaceSkuMap = _db.ProductSkuMaps.AsNoTracking().ToList();
                    var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    var AllCategories = LS.Get<Category>(); //get category cache
                    var AllManufacturers = LS.Get<Manufacturer>();
                    PIsheme sheme = new PIsheme();
                    int lineNum = 1;
                    StringBuilder errors = new StringBuilder();
                    StringBuilder lastEmpty = new StringBuilder();
                    List<ProcessError> processErrors = new List<ProcessError>();
                    bool needCreateCategoryIfNotExist = false;
                    Dictionary<string, bool> processed = new Dictionary<string, bool>();

                    var allMeasures = LS.Get<Measure>();
                    foreach (var s in all)
                    {
                        lineNum++;
                        if (lineNum == 2) { continue; }//skip second row (excell numeration)
                        if (s.Count < 17)
                        {
                            processErrors.Add(new ProcessError() { LineNum = lineNum, Message = string.Format(RP.S("Admin.Product.Import.Error.WrongColumnCount-LineNume{0}"), lineNum) });
                            errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.WrongColumnCount-LineNume{0}"), lineNum));
                            continue;
                        }
                        string actionFlag = s[sheme.FlagOperation];
                        if (string.IsNullOrEmpty(actionFlag))
                        {
                            actionFlag = "1";
                        }
                        bool isInsert = false;
                        bool isDelete = actionFlag == "0";
                        bool isInsertUpdateIfNotDest = actionFlag == "1"; // insert or add if before empty or null or default;
                        bool isUpdateIfSource = actionFlag == "2"; // update only if not null empty or default
                        bool isFullUpdate = actionFlag == "3"; // update only if not null empty or default
                        if (!isDelete)
                        {
                            isInsert = true;
                        }
                        long test = 0;
                        bool validRow = true;
                        bool SKUorNameEmpty = false;
                        if (s[sheme.SKU] == null)
                        {
                            SKUorNameEmpty = true;
                        }
                        else if (s[sheme.SKU].ToString().Trim() == "")
                        {
                            SKUorNameEmpty = true;
                        }
                        else if (s[sheme.Name] == null)
                        {
                            SKUorNameEmpty = true;
                        }
                        else if (s[sheme.Name] == "")
                        {
                            SKUorNameEmpty = true;
                        }
                        else if (!long.TryParse(s[sheme.SKU].ToString(), out test))
                        {
                            //  SKUorNameEmpty = true;
                        }

                        if (SKUorNameEmpty)
                        {
                            processErrors.Add(new ProcessError()
                            {
                                LineNum = lineNum
                                ,
                                Message = string.Format(RP.S("Admin.Product.Import.Error.SKUorNameEmpty-LineNume{0}"), lineNum)
                            });

                            lastEmpty.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.SKUorNameEmpty-LineNume{0}"), lineNum));
                            continue;
                        }
                        //check SKU code
                        var sku = s[sheme.SKU].ToString().Trim();
                        sku = Regex.Replace(sku, "[^.0-9]", "");
                        var fieldsToUpdate = new List<string>();

                        //Ignore numbers in s[sheme.MeasureUnit]
                        s[sheme.MeasureUnit] = Regex.Replace(s[sheme.MeasureUnit], "[0-9]", "");
                        //check map table
                        //  var replaceTo = replaceSkuMap.FirstOrDefault(x => x.ShortSKU == sku);
                        //if (replaceTo != null)
                        // {
                        //     sku = replaceTo.ProductSKU;
                        // }
                        //check if not repeated row
                        if (processed.ContainsKey(sku))
                        {
                            validRow = false;
                            processErrors.Add(new ProcessError()
                            {
                                LineNum = lineNum
                                ,
                                SKU = sku
                                ,
                                Message = string.Format(RP.S("Admin.Product.Import.Error.SKUAlreadyProcessed-LineNume{0}"), lineNum)
                            });
                            errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.SKUAlreadyProcessed-LineNume{0}"), lineNum));

                            continue;
                        }
                        processed[sku] = true;

                        if (sku.Length == 13 || sku.Length == 12)
                        {
                            // 036000291454x

                            int codelength = 12;
                            int startFrom1 = 1;
                            int startFrom2 = 0;
                            if (sku.Length == 12)
                            {
                                // 03600029145x
                                codelength = 11;
                                startFrom1 = 0;
                                startFrom2 = 1;
                            }
                            int odd = 0;
                            int even = 0;
                            int check = 0;
                            int.TryParse(sku[codelength].ToString(), out check);

                            int tmp = 0;
                            for (int i = startFrom1; i < codelength; i = i + 2)
                            {
                                int.TryParse(sku[i].ToString(), out tmp);
                                odd += tmp;
                            }
                            odd = odd * 3;
                            for (int i = startFrom2; i < codelength; i = i + 2)
                            {
                                int.TryParse(sku[i].ToString(), out tmp);
                                even += tmp;
                            }
                            int sum = odd + even;
                            int module = sum % 10;
                            int mustBe = (10 - module) % 10;
                            if (mustBe != check)
                            {
                                //validRow = false;
                                processErrors.Add(new ProcessError()
                                {
                                    LineNum = lineNum
                                    ,
                                    SKU = sku
                                    ,
                                    Message = string.Format(RP.S("Admin.Product.Import.Error.SKUnotValid-LineNume{0}"), lineNum)
                                });
                                errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.SKUnotValid-LineNume{0}"), lineNum));

                            }
                        }


                        if (lastEmpty.Length > 0)
                        {

                            errors.AppendLine(lastEmpty.ToString());
                            lastEmpty.Clear();
                        }
                        //var priceStr = s[sheme.Price].ToString().Trim();
                        //if (priceStr == "")
                        //{
                        //    priceStr = "0";
                        //}
                        //else
                        //{
                        //    fieldsToUpdate.Add("RecomendedPrice");
                        //}
                        if (!string.IsNullOrEmpty(s[sheme.Name].ToString()))
                        {
                            fieldsToUpdate.Add("Name");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.IsKosher].ToString()))
                        {
                            fieldsToUpdate.Add("IsKosher");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.KosherType].ToString()))
                        {
                            fieldsToUpdate.Add("KosherType");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.MadeCountry]))
                        {
                            fieldsToUpdate.Add("MadeCoutry");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.Capacity].ToString()))
                        {
                            fieldsToUpdate.Add("Capacity");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.MeasureUnit].ToString()))
                        {
                            fieldsToUpdate.Add("MeasureUnit");
                            fieldsToUpdate.Add("ProductMeasureID");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.Components].ToString()))
                        {
                            fieldsToUpdate.Add("Components");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.Tax].ToString()))
                        {
                            fieldsToUpdate.Add("NoTax");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.SoldByWeight].ToString()))
                        {
                            fieldsToUpdate.Add("SoldByWeight");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.ShortDescription].ToString()))
                        {
                            fieldsToUpdate.Add("ShortDescription");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.FullDescription].ToString()))
                        {
                            fieldsToUpdate.Add("FullDescription");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.ProductOptions].ToString()))
                        {
                            fieldsToUpdate.Add("ProductShopOptions");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.UnitPerPackage].ToString()))
                        {
                            fieldsToUpdate.Add("UnitPerPackage");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.Manufacturer].ToString()))
                        {
                            fieldsToUpdate.Add("ProductManufacturerID");
                            fieldsToUpdate.Add("DisplayOrder");
                        }
                        if (!string.IsNullOrEmpty(s[sheme.SubCategory].ToString()))
                        {
                            fieldsToUpdate.Add("CategoryID");
                        }

                        if (!string.IsNullOrEmpty(s[sheme.UnitOfMeasure].ToString()) 
                            && !string.IsNullOrEmpty(s[sheme.QuantityUnitOfMeasure].ToString()))
                        {
                            fieldsToUpdate.Add("ContentUnitPriceMultiplicator");
                            fieldsToUpdate.Add("ContentUnitMeasureID");
                        }




                        var product = new Product()
                        {
                            SKU = sku,
                            Name = s[sheme.Name].ToString().Trim(),
                            IsKosher = s[sheme.IsKosher].ToString().Trim() == "1"
                            || s[sheme.IsKosher].ToString().Trim() == "true"
                            || s[sheme.IsKosher].ToString().Trim() == "True"
                            || !string.IsNullOrEmpty(s[sheme.KosherType].ToString().Trim()),
                            KosherType = s[sheme.KosherType].ToString().Trim(),

                            Capacity = s[sheme.Capacity].ToString().Trim(),
                            MeasureUnit = s[sheme.MeasureUnit].ToString().Trim() == "" ? null : s[sheme.MeasureUnit].ToString().Trim(),
                            // ProductMeasureID = allMeasures
                            Components = s[sheme.Components].ToString().Trim(),
                            // Manufacturer = s[sheme.Manufacturer].ToString().Trim(),
                            NoTax = s[sheme.Tax].ToString().Trim() == "1" || s[sheme.Tax].ToString().Trim() == "true" || s[sheme.Tax].ToString().Trim() == "True",
                            SoldByWeight = s[sheme.SoldByWeight].ToString().Trim() == "1" || s[sheme.SoldByWeight].ToString().Trim() == "true" || s[sheme.SoldByWeight].ToString().Trim() == "True",
                            ShortDescription = s[sheme.ShortDescription].ToString().Trim(),
                            FullDescription = s[sheme.FullDescription].ToString().Trim(),
                            ProductShopOptions = s[sheme.ProductOptions].ToString().Trim(),
                            MadeCoutry = s[sheme.MadeCountry].Trim()
                        };

                        PrepareContentUnitMeasure(s, sheme,product);
                        //Searching for measure matching
                        var firstMatchedMeasure = allMeasures.FirstOrDefault(x => x.VariantList.Contains(s[sheme.MeasureUnit]));
                        if (firstMatchedMeasure != null)
                        {
                            product.ProductMeasureID = firstMatchedMeasure.ID;
                        }
                        //if (!string.IsNullOrEmpty(priceStr))
                        //{
                        //    decimal price = 0;
                        //    bool valid = decimal.TryParse(priceStr.Replace(".", decimalSeparator).Replace(",", decimalSeparator)
                        //         , out price);

                        //    product.RecomendedPrice = price;
                        //    if (!valid)
                        //    {
                        //        validRow = false;
                        //        processErrors.Add(new ProcessError()
                        //        {
                        //            LineNum = lineNum
                        //            ,
                        //            SKU = sku,
                        //            Message = string.Format(RP.S("Admin.Product.Import.Error.PriceNotValid-LineNume{0}"), lineNum)
                        //        });
                        //        errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.PriceNotValid-LineNume{0}"), lineNum));
                        //    }
                        //}

                        //step for quantity change
                        if (!string.IsNullOrEmpty(s[sheme.UnitPerPackage]))
                        {
                            decimal step = 0;
                            bool valid = decimal.TryParse(s[sheme.UnitPerPackage].ToString().Trim(), out step);
                            if (step > 0)
                                product.MeasureUnitStep = step;
                            product.UnitsPerPackage = step;

                            if (!valid)
                            {
                                validRow = false;
                                processErrors.Add(new ProcessError()
                                {
                                    LineNum = lineNum
                                    ,
                                    SKU = sku,
                                    Message = string.Format(RP.S("Admin.Product.Import.Error.UnitPerPackageNotValidNumber-LineNume{0}"), lineNum)
                                });
                                errors.AppendLine(string.Format(RP.S("Admin.Product.Import.Error.UnitPerPackageNotValidNumber-LineNume{0}"), lineNum));
                            }

                        }
                        var manuf = s[sheme.Manufacturer].ToString().Trim();
                        if (!string.IsNullOrEmpty(manuf))
                        {
                            var man = AllManufacturers.FirstOrDefault(x => x.Name != null && x.Name.ToLower() == manuf.ToLower());
                            if (man == null)
                            {
                                man = new Manufacturer()
                                {
                                    DisplayOrder = 0,
                                    Published = true,
                                    Name = manuf
                                };
                                _db.Manufacturers.Add(man);
                                _db.SaveChanges();
                                AllManufacturers.Add(man);
                            }
                            if (man != null)
                            {
                                product.ProductManufacturerID = man.ID;
                                product.DisplayOrder = man.DisplayOrder;
                            }
                        }
                        //category
                        //var parentcat = AllCategories.FirstOrDefault(x => x.Name == s[sheme.Category].ToString() && x.ParentCategoryID == 0);
                        //if (parentcat == null)
                        //{
                        //    parentcat = new Category()
                        //    {
                        //        DisplayOrder = 500,
                        //        Name = s[sheme.Category],
                        //        ParentCategoryID=0,
                        //        Published=true,
                        //    };
                        //    _db.Categories.Add(parentcat);
                        //    _db.SaveChanges();
                        //    AllCategories.Add(parentcat);
                        //}
                        if (validRow && !string.IsNullOrEmpty(s[sheme.SubCategory].ToString().Trim()))
                        {
                            var cat = AllCategories.FirstOrDefault(x => x.Name == s[sheme.SubCategory].ToString().Trim());
                            if (cat == null
                                && needCreateCategoryIfNotExist)
                            {
                                cat = new Category()
                                {
                                    DisplayOrder = 500,
                                    Name = s[sheme.SubCategory].ToString().Trim(),
                                    ParentCategoryID = 0,
                                    Published = true,
                                };
                                _db.Categories.Add(cat);
                                _db.SaveChanges();
                                AllCategories.Add(cat);
                            }
                            if (cat != null)
                            {
                                product.CategoryID = cat.ID;
                            }
                        }
                        if (!isDelete)
                        {
                            fieldsToUpdate.Add("Deleted");
                        }
                        if (isDelete || validRow)
                        {
                            if (productsList.ContainsKey(sku))
                            {
                                var oldProduct = productsList[sku];
                                product.ID = oldProduct.ID;


                                if (isInsertUpdateIfNotDest)
                                {
                                    #region Flag 1 (Update if Destination null empty or default)
                                    fieldsToUpdate.Clear();
                                    //if (oldProduct.RecomendedPrice == 0)
                                    //{
                                    //    fieldsToUpdate.Add("RecomendedPrice");
                                    //}
                                    if (string.IsNullOrEmpty(oldProduct.Name))
                                    {
                                        fieldsToUpdate.Add("Name");
                                    }
                                    if (!oldProduct.IsKosher)
                                    {
                                        fieldsToUpdate.Add("IsKosher");
                                    }
                                    if (string.IsNullOrEmpty(oldProduct.KosherType))
                                    {
                                        fieldsToUpdate.Add("KosherType");
                                    }
                                    if (!string.IsNullOrEmpty(oldProduct.MadeCoutry))
                                    {
                                        fieldsToUpdate.Add("MadeCoutry");
                                    }
                                    if (string.IsNullOrEmpty(oldProduct.Capacity))
                                    {
                                        fieldsToUpdate.Add("Capacity");
                                    }
                                    if (string.IsNullOrEmpty(oldProduct.MeasureUnit))
                                    {
                                        fieldsToUpdate.Add("MeasureUnit");

                                    }
                                    if (oldProduct.ProductMeasureID == 0)
                                    {
                                        fieldsToUpdate.Add("ProductMeasureID");
                                    }
                                    if (string.IsNullOrEmpty(oldProduct.Components))
                                    {
                                        fieldsToUpdate.Add("Components");
                                    }
                                    if (!oldProduct.NoTax)
                                    {
                                        fieldsToUpdate.Add("NoTax");
                                    }
                                    if (!oldProduct.SoldByWeight)
                                    {
                                        fieldsToUpdate.Add("SoldByWeight");
                                    }
                                    if (string.IsNullOrEmpty(oldProduct.ShortDescription))
                                    {
                                        fieldsToUpdate.Add("ShortDescription");
                                    }
                                    if (string.IsNullOrEmpty(oldProduct.FullDescription))
                                    {
                                        fieldsToUpdate.Add("FullDescription");
                                    }
                                    if (string.IsNullOrEmpty(oldProduct.ProductShopOptions))
                                    {
                                        fieldsToUpdate.Add("ProductShopOptions");
                                    }
                                    if (!oldProduct.UnitsPerPackage.HasValue)
                                    {
                                        fieldsToUpdate.Add("UnitPerPackage");
                                    }
                                    if (oldProduct.ProductManufacturerID == 0)
                                    {
                                        fieldsToUpdate.Add("ProductManufacturerID");
                                        fieldsToUpdate.Add("DisplayOrder");
                                    }
                                    if (oldProduct.CategoryID == 0)
                                    {
                                        fieldsToUpdate.Add("CategoryID");
                                    }

                                    if (oldProduct.ContentUnitMeasureID == 0)
                                    {
                                        fieldsToUpdate.Add("ContentUnitMeasureID");
                                        
                                    }

                                    if (oldProduct.ContentUnitPriceMultiplicator == 0)
                                    {
                                        fieldsToUpdate.Add("ContentUnitPriceMultiplicator");
                                    }




                                    productForUpdateVariable.Add(new ProductVariable<Product>()
                                    {
                                        Fields = fieldsToUpdate,
                                        Entity = product
                                    });
                                    #endregion
                                }
                                else if (isUpdateIfSource)
                                {
                                    #region Flag 2 isUpdateIfSource
                                    productForUpdateVariable.Add(new ProductVariable<Product>()
                                    {
                                        Fields = fieldsToUpdate,
                                        Entity = product
                                    });
                                    #endregion
                                }
                                else if (isFullUpdate)
                                {
                                    //flag 3
                                    productForUpdate.Add(product);
                                }
                                else if (isDelete)
                                {
                                    //Flag 0
                                    productForDelete.Add(product);
                                }


                                #region oldImport
                                //if (!productsList[sku].IgnoreOnImport)
                                //{
                                //    var pID = productsList[sku].ID;
                                //    product.ID = pID;
                                //    //update
                                //    if (isUpdateIfSource)
                                //        productForUpdate.Add(product);

                                //    //delete
                                //    if (isDelete)
                                //        productForDelete.Add(product);
                                //}
                                #endregion
                            }
                            else
                            {
                                //insert
                                if (isInsert)
                                {
                                    if (!productforInsert.Any(x => x.SKU == product.SKU))
                                        productforInsert.Add(product);
                                }
                            }
                        }
                    }
                    string message = RP.S("Admin.Product.Import.SeeError");
                    if (true)// || errors.Length < 2) //no errors
                    {
                        currentTask.Message = RP.S("Admin.PlanedTask.InProgress.SqlBulkOperation");
                        currentTask.PercentProgress = 80;
                        _db.SaveChanges();
                        var insertRes = productforInsert.SqlInsert(returnLog: true);
                        var updateRes = productForUpdate.SqlUpdateById(false, returnLog: true);
                        var updateVarRes = productForUpdateVariable.SqlUpdateById(true);
                        var deleteRes = productForDelete.SqlMarkAsDeletedById(true);
                        message = RP.S("Admin.Product.Import.Success");
                        //add errors to table
                        try
                        {
                            foreach (var procError in processErrors)
                            {
                                procError.CreateOn = DateTime.Now;
                                procError.FileServiceName = FileName;
                                //  procError.IP = LS.GetUser_IP(Request);
                                //  procError.PageUrl = Request.RawUrl;
                                //  procError.RefererUrl = Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : null;
                                procError.ShopID = 0;
                                procError.UserID = LS.CurrentUser.ID;
                            }
                            processErrors.SqlInsert();
                        }
                        catch
                        {

                        }

                        // ActivityLogFiles
                        if (false)
                        {
                            var filename = Guid.NewGuid().ToString().ToLower()
                                .Replace("-", "")
                                + Path.GetExtension(FilePath);
                            var filePath = HostingEnvironment.MapPath("~/Content/ActivityLogFiles/") + filename;
                            System.IO.File.Copy(FilePath, filePath, true);
                            ActivityLog activity = new ActivityLog()
                            {
                                ActivityType = ActivityType.Bulk,
                                CreateOn = DateTime.Now,
                                DirectSQL = insertRes.SqlLog.ToString() + @"
                    
--UPDATE SQL
" + updateRes.SqlLog.ToString()

              + @"
                    
--DELETE SQL
" + deleteRes.SqlLog.ToString()
              ,
                                EntityType = EntityType.Product,
                                FullText = "",
                                // RequestUrl = Request.RawUrl,
                                ShortDescription = "",
                                // UploadedFileName = attachment.FileName,
                                CopiedFileName = filename,
                                UserID = LS.CurrentUser.ID
                            };
                        }
                    }
                    #endregion


                }



            }
        }
        private static Regex decimalsOnly = new Regex(@"[^-?\d+\.\,]");
        private static void PrepareContentUnitMeasure(Dictionary<int, string> s, PIsheme sheme, Product product)
        {
            var unitOfMeasure=s[sheme.UnitOfMeasure];
            var quantityUnitOfMeasure=s[sheme.QuantityUnitOfMeasure];

            if (string.IsNullOrEmpty(unitOfMeasure) || string.IsNullOrEmpty(quantityUnitOfMeasure))
                return;

            var contentUnitMeasureMap=RP.GetContentUnitMeasureMaps().FirstOrDefault(x => x.Synonymous.Contains("|" + unitOfMeasure + "|"));
            if (contentUnitMeasureMap == null) return;
            
            var contentUnitMeasure=RP.GetContentUnitMeasureById(contentUnitMeasureMap.ContentUnitMeasureID);
            if (contentUnitMeasure==null) return;

            decimal capacity=0;
            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            quantityUnitOfMeasure=decimalsOnly.Replace(quantityUnitOfMeasure, "");
            if (!Decimal.TryParse(quantityUnitOfMeasure.Replace(".", decimalSeparator).Replace(",", decimalSeparator), out capacity)) return;
            if (capacity == 0) return;
            product.ContentUnitMeasureID = contentUnitMeasure.ID;
            product.ContentUnitPriceMultiplicator = contentUnitMeasureMap.Multiplicator / capacity;
        }


        #endregion
    }
}