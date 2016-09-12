using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.EntityExtensions
{
    public static class EntityGeneric
    {

        public static string _log = "";


        public static int SqlInsertForType<G, T>(this IList<T> list, int porcion = 500)
            where T : class
            where G : class
        {
            var _db = LS.CurrentEntityContext;

            var type = typeof(T);
            string Disctiminator = type.Name;
            if (type.BaseType.Name != "object")
            {
                type = type.BaseType;
            }

            string className = typeof(G).Name;
            var container =
               (_db as IObjectContextAdapter).ObjectContext.MetadataWorkspace.GetEntityContainer((_db as IObjectContextAdapter).ObjectContext.DefaultContainerName, DataSpace.CSpace);
            string setName = (from meta in container.BaseEntitySets
                              where meta.ElementType.Name == className
                              select meta.Name).First();

            var CreateScrname = (_db as IObjectContextAdapter).ObjectContext.CreateObjectSet<G>();
            var sqlCreate = CreateScrname.ToTraceString();

            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sqlCreate);

            string table = match.Groups["table"].Value;
            var fields = CreateScrname.EntitySet.ElementType.Properties;
            StringBuilder sb = new StringBuilder();
            string startSql = @"INSERT INTO " + table + @"

(";
            string endSql = @";
";
            string valSql = "";
            sb.Append(startSql);
            string sbdlm = "";
            var pType = typeof(T);
            var pInfo = pType.GetProperties();
            var plist = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo pi in pInfo)
            {
                plist.Add(pi.Name, pi);
            }

            string subSubDlm = "";

            subSubDlm = "";

            foreach (var p in plist.Values)
            {
                var attr = p.GetCustomAttribute(typeof(NotMappedAttribute));
                if (attr != null)
                {
                    continue;
                }


                if (p.Name == "ID") { continue; }
                valSql += subSubDlm + " [" + p.Name + "] ";
                subSubDlm = ", ";
            }
            valSql += subSubDlm + "[Discriminator] ";
            sb.Append(valSql + @")
VALUES");

            sbdlm = "";
            int count = 0;
            var res = 0;
            foreach (var item in list)
            {
                sb.Append(sbdlm + "(");
                subSubDlm = "";
                foreach (var p in plist.Values)
                {
                    var attr = p.GetCustomAttribute(typeof(NotMappedAttribute));
                    if (attr != null)
                    {
                        continue;
                    }
                    if (p.Name == "ID") { continue; }
                    //addFormat = "";
                    if (plist.ContainsKey(p.Name))
                    {
                        var propinfo = plist[p.Name];
                        var val = propinfo.GetValue(item);
                        if (val == null)
                        {
                            sb.Append(subSubDlm + "NULL");
                        }
                        else
                            if (p.PropertyType.Name == "DateTime")
                            {
                                sb.Append(subSubDlm + "'" + (((DateTime)val).Year > 1920 ? ((DateTime)val).Year.ToString() : "2014") + ((DateTime)val).Month.ToString("00") + ((DateTime)val).Day.ToString("00") + "'");
                            }
                            else
                                if (p.PropertyType.Name == "String")
                                {
                                    sb.Append(subSubDlm + "N'" + ((string)val).Replace("'", @"''") + "'");

                                }
                                else if (p.PropertyType.Name == "Boolean")
                                {
                                    sb.Append(subSubDlm + (((bool)val) ? "1" : "0"));

                                }
                                else if (p.PropertyType.Name == "Guid")
                                {

                                    sb.Append(subSubDlm + "CONVERT(uniqueidentifier, '" + ((Guid)val).ToString() + "')");
                                }
                                else if (p.PropertyType.BaseType.Name == "Enum")
                                {
                                    sb.Append(subSubDlm + (int)val);
                                }
                                else if (p.PropertyType.FullName.Contains("Double"))
                                {
                                    sb.Append(subSubDlm + val.ToString().Replace(',', '.'));
                                }
                                else
                                {
                                    sb.Append(subSubDlm + val);
                                }
                        //if (p.Nullable)
                        //{
                        //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
                        //}
                        //else
                        //{
                        //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
                        //}


                        subSubDlm = ", ";
                    }

                }
                sb.Append(subSubDlm + "N'" + Disctiminator + "'");
                sb.Append(@")
");
                sbdlm = @",";
                count++;
                if (count > porcion)
                {
                    //flush porcion
                    sb.Append(endSql);
                    var sqlPorcion = sb.ToString();
                    try
                    {
                        res += _db.Database.ExecuteSqlCommand(sqlPorcion, new object[] { });

                    }
                    catch (Exception e)
                    {
                        _log += "\n " + sqlPorcion + "\n " + e.Message;
                        if (e.InnerException != null)
                        {
                            _log += e.InnerException.Message;
                        }
                    }
                    sb.Clear();
                    sb.Append(startSql);
                    sb.Append(valSql + @")
VALUES");
                    sbdlm = "";
                    count = 0;

                }
            }
            sb.Append(endSql);
            //INSERT INTO Production.UnitMeasure VALUES (N'FT2', N'Square Feet ', '20080923'), (N'Y', N'Yards', '20080923'), (N'Y3', N'Cubic Yards', '20080923');
            string sql = sb.ToString();

            if (count > 0)
            {
                res += _db.Database.ExecuteSqlCommand(sql, new object[] { });
            }
            return res;
        }
        public static BulkResult SqlInsert<T>(this IList<T> list, bool returnLog = false) where T : class
        {
            var _db = LS.CurrentEntityContext;
            var bulkResult = new BulkResult() { DBreturn = 0 };
            var type = typeof(T);
            if (type.BaseType.Name != "object")
            {
                type = type.BaseType;
            }
            var CreateScrname = (_db as IObjectContextAdapter).ObjectContext.CreateObjectSet<T>();
            var sqlCreate = CreateScrname.ToTraceString();

            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sqlCreate);

            string table = match.Groups["table"].Value;
            var fields = CreateScrname.EntitySet.ElementType.Properties;
            StringBuilder sb = new StringBuilder();
            string startSql = @"INSERT INTO " + table + @"

(";
            string endSql = @";
";
            string valSql = "";
            sb.Append(startSql);
            string sbdlm = "";
            var pType = typeof(T);
            var pInfo = pType.GetProperties();
            var plist = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo pi in pInfo)
            {
                plist.Add(pi.Name, pi);
            }

            string subSubDlm = "";

            subSubDlm = "";

            foreach (var p in fields)
            {
                //addFormat = "";
                //if (p.TypeUsage.EdmType.Name == "DateTime")
                //{
                //    addFormat = ", 120";
                //}

                //if (p.Nullable)
                //{
                //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
                //}
                //else
                //{
                //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
                //}
                if (p.Name == "ID") { continue; }
                valSql += subSubDlm + p.Name;
                subSubDlm = ", ";
            }
            sb.Append(valSql + @")
VALUES");
            sbdlm = "";
            int count = 0;
            var res = 0;
            if (returnLog)
            {
                bulkResult.SqlLog = new StringBuilder();
            }
            foreach (var item in list)
            {
                sb.Append(sbdlm + "(");
                subSubDlm = "";
                foreach (var p in fields)
                {
                    if (p.Name == "ID") { continue; }
                    //addFormat = "";
                    if (plist.ContainsKey(p.Name))
                    {
                        var propinfo = plist[p.Name];
                        var val = propinfo.GetValue(item);
                        if (val == null)
                        {
                            sb.Append(subSubDlm + "NULL");
                        }
                        else
                            if (p.TypeUsage.EdmType.Name == "DateTime")
                            {
                                sb.Append(subSubDlm + "'" + (((DateTime)val).Year > 1920 ? ((DateTime)val).Year.ToString() : "2014") + ((DateTime)val).Month.ToString("00") + ((DateTime)val).Day.ToString("00") + "'");
                            }
                            else
                                if (p.TypeUsage.EdmType.Name == "String")
                                {
                                    sb.Append(subSubDlm + "N'" + ((string)val).Replace("'", @"''") + "'");

                                }
                                else if (p.TypeUsage.EdmType.Name == "Boolean")
                                {
                                    sb.Append(subSubDlm + (((bool)val) ? "1" : "0"));

                                }
                                else if (p.TypeUsage.EdmType.Name == "Guid")
                                {

                                    sb.Append(subSubDlm + "CONVERT(uniqueidentifier, '" + ((Guid)val).ToString() + "')");
                                }
                                else if (p.IsEnumType)
                                {
                                    sb.Append(subSubDlm + ((int)val).ToString());
                                }
                                else
                                {
                                    sb.Append(subSubDlm + val);
                                }
                        //if (p.Nullable)
                        //{
                        //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
                        //}
                        //else
                        //{
                        //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
                        //}


                        subSubDlm = ", ";
                    }
                }
                sb.Append(@")
");
                sbdlm = @",";
                count++;
                if (count > 500)
                {
                    //flush porcion
                    sb.Append(endSql);
                    var sqlPorcion = sb.ToString();
                    bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sqlPorcion, new object[] { });
                    if (returnLog)
                    {
                        bulkResult.SqlLog.Append(@"--porcion 
" + sqlPorcion + @"
");
                    }
                    sb.Clear();
                    sb.Append(startSql);
                    sb.Append(valSql + @")
VALUES");
                    sbdlm = "";
                    count = 0;

                }
            }
            sb.Append(endSql);
            //INSERT INTO Production.UnitMeasure VALUES (N'FT2', N'Square Feet ', '20080923'), (N'Y', N'Yards', '20080923'), (N'Y3', N'Cubic Yards', '20080923');
            string sql = sb.ToString();

            if (count > 0)
            {
                bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sql, new object[] { });
                if (returnLog)
                {
                    bulkResult.SqlLog.Append(@"--porcion 
" + sql + @"
");
                }
            }
            return bulkResult;
        }

        public static BulkResult SqlDeleteById<T>(this IEnumerable<T> list, bool returnLog = false) where T : class
        {
            var _db = LS.CurrentEntityContext;
            BulkResult bulkResult = new BulkResult();
            var type = typeof(T);
            if (type.BaseType.Name != "object")
            {
                type = type.BaseType;
            }
            var CreateScrname = (_db as IObjectContextAdapter).ObjectContext.CreateObjectSet<T>();
            var sqlCreate = CreateScrname.ToTraceString();

            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sqlCreate);

            string table = match.Groups["table"].Value;
            var fields = CreateScrname.EntitySet.ElementType.Properties;
            StringBuilder sb = new StringBuilder();
            string startSql = "";
            string endSql = @"
";
            string valSql = "";
            sb.Append(startSql);
            string sbdlm = "";
            var pType = typeof(T);
            var pInfo = pType.GetProperties();
            var plist = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo pi in pInfo)
            {
                plist.Add(pi.Name, pi);
            }

            string subSubDlm = "";

            subSubDlm = "";

            //            foreach (var p in fields)
            //            {
            //                //addFormat = "";
            //                //if (p.TypeUsage.EdmType.Name == "DateTime")
            //                //{
            //                //    addFormat = ", 120";
            //                //}

            //                //if (p.Nullable)
            //                //{
            //                //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
            //                //}
            //                //else
            //                //{
            //                //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
            //                //}
            //                if (p.Name == "ID") { continue; }
            //                valSql += subSubDlm + p.Name;
            //                subSubDlm = ", ";
            //            }
            //            sb.Append(valSql + @")
            //VALUES");
            sbdlm = @"DELETE FROM " + table + @" 

";
            int count = 0;
            var res = 0;
            foreach (var item in list)
            {
                sb.Append(sbdlm + "");
                subSubDlm = "";

                sb.Append(@" WHERE [ID] = " + plist["ID"].GetValue(item) + @" ;
");
                // sbdlm = @",";
                count++;
                if (count > 500)
                {
                    //flush porcion
                    sb.Append(endSql);
                    var sqlPorcion = sb.ToString();
                    bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sqlPorcion, parameters: new object[] { });
                    if (returnLog)
                    {
                        bulkResult.SqlLog.Append(@"--porcion delete 
" + sqlPorcion + @"
");
                    }
                    sb.Clear();
                    sb.Append(startSql);

                    // sbdlm = "";
                    count = 0;

                }
            }
            sb.Append(endSql);
            //INSERT INTO Production.UnitMeasure VALUES (N'FT2', N'Square Feet ', '20080923'), (N'Y', N'Yards', '20080923'), (N'Y3', N'Cubic Yards', '20080923');
            string sql = sb.ToString();

            if (count > 0)
            {
                bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sql, parameters: new object[] { });
                if (returnLog)
                {
                    bulkResult.SqlLog.Append(@"--porcion delete 
" + sql + @"
");
                }
            }
            return bulkResult;
        }

        //
        public static int SqlMarkAsNonDeletedById<T>(this IEnumerable<T> list) where T : class
        {
            var _db = LS.CurrentEntityContext;

            var type = typeof(T);
            if (type.BaseType.Name != "object")
            {
                type = type.BaseType;
            }
            var CreateScrname = (_db as IObjectContextAdapter).ObjectContext.CreateObjectSet<T>();
            var sqlCreate = CreateScrname.ToTraceString();

            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sqlCreate);

            string table = match.Groups["table"].Value;
            var fields = CreateScrname.EntitySet.ElementType.Properties;
            StringBuilder sb = new StringBuilder();
            string startSql = "";
            string endSql = @"
";
            string valSql = "";
            sb.Append(startSql);
            string sbdlm = "";
            var pType = typeof(T);
            var pInfo = pType.GetProperties();
            var plist = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo pi in pInfo)
            {
                plist.Add(pi.Name, pi);
            }

            string subSubDlm = "";

            subSubDlm = "";

            //            foreach (var p in fields)
            //            {
            //                //addFormat = "";
            //                //if (p.TypeUsage.EdmType.Name == "DateTime")
            //                //{
            //                //    addFormat = ", 120";
            //                //}

            //                //if (p.Nullable)
            //                //{
            //                //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
            //                //}
            //                //else
            //                //{
            //                //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
            //                //}
            //                if (p.Name == "ID") { continue; }
            //                valSql += subSubDlm + p.Name;
            //                subSubDlm = ", ";
            //            }
            //            sb.Append(valSql + @")
            //VALUES");
            sbdlm = @"UPDATE " + table + @" SET

";
            int count = 0;
            var res = 0;
            foreach (var item in list)
            {
                sb.Append(sbdlm + "");
                subSubDlm = "";

                sb.Append(@" [Deleted]=0 WHERE [ID] = " + plist["ID"].GetValue(item) + @" ;
");
                // sbdlm = @",";
                count++;
                if (count > 500)
                {
                    //flush porcion
                    sb.Append(endSql);
                    var sqlPorcion = sb.ToString();
                    res += _db.Database.ExecuteSqlCommand(sqlPorcion, parameters: new object[] { });
                    sb.Clear();
                    sb.Append(startSql);

                    //  sbdlm = "";
                    count = 0;

                }
            }
            sb.Append(endSql);
            //INSERT INTO Production.UnitMeasure VALUES (N'FT2', N'Square Feet ', '20080923'), (N'Y', N'Yards', '20080923'), (N'Y3', N'Cubic Yards', '20080923');
            string sql = sb.ToString();

            if (count > 0)
            {
                res += _db.Database.ExecuteSqlCommand(sql, parameters: new object[] { });
            }
            return res;
        }
        public static BulkResult SqlMarkAsDeletedById<T>(this IEnumerable<T> list, bool returnLog = false) where T : class
        {
            var _db = LS.CurrentEntityContext;
            BulkResult bulkResult = new BulkResult();
            var type = typeof(T);
            if (type.BaseType.Name != "object")
            {
                type = type.BaseType;
            }
            var CreateScrname = (_db as IObjectContextAdapter).ObjectContext.CreateObjectSet<T>();
            var sqlCreate = CreateScrname.ToTraceString();

            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sqlCreate);

            string table = match.Groups["table"].Value;
            var fields = CreateScrname.EntitySet.ElementType.Properties;
            StringBuilder sb = new StringBuilder();
            string startSql = "";
            string endSql = @"
";
            string valSql = "";
            sb.Append(startSql);
            string sbdlm = "";
            var pType = typeof(T);
            var pInfo = pType.GetProperties();
            var plist = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo pi in pInfo)
            {
                plist.Add(pi.Name, pi);
            }

            string subSubDlm = "";

            subSubDlm = "";

            //            foreach (var p in fields)
            //            {
            //                //addFormat = "";
            //                //if (p.TypeUsage.EdmType.Name == "DateTime")
            //                //{
            //                //    addFormat = ", 120";
            //                //}

            //                //if (p.Nullable)
            //                //{
            //                //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
            //                //}
            //                //else
            //                //{
            //                //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
            //                //}
            //                if (p.Name == "ID") { continue; }
            //                valSql += subSubDlm + p.Name;
            //                subSubDlm = ", ";
            //            }
            //            sb.Append(valSql + @")
            //VALUES");
            sbdlm = @"UPDATE " + table + @" SET

";
            int count = 0;
            var res = 0;
            foreach (var item in list)
            {
                sb.Append(sbdlm + "");
                subSubDlm = "";

                sb.Append(@" [Deleted]=1 WHERE [ID] = " + plist["ID"].GetValue(item) + @" ;
");
                // sbdlm = @",";
                count++;
                if (count > 500)
                {
                    //flush porcion
                    sb.Append(endSql);
                    var sqlPorcion = sb.ToString();
                    bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sqlPorcion, parameters: new object[] { });
                    if (returnLog)
                    {
                        bulkResult.SqlLog.Append(@"--porcion delete 
" + sqlPorcion + @"
");
                    }
                    sb.Clear();
                    sb.Append(startSql);

                    //  sbdlm = "";
                    count = 0;

                }
            }
            sb.Append(endSql);
            //INSERT INTO Production.UnitMeasure VALUES (N'FT2', N'Square Feet ', '20080923'), (N'Y', N'Yards', '20080923'), (N'Y3', N'Cubic Yards', '20080923');
            string sql = sb.ToString();

            if (count > 0)
            {
                bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sql, parameters: new object[] { });
                if (returnLog)
                {
                    bulkResult.SqlLog.Append(@"--porcion delete 
" + sql + @"
");
                }
            }
            return bulkResult;
        }
        public static BulkResult SqlUpdateById<T>(this IEnumerable<T> list, bool updateNullableOrDefaulted = true, bool returnLog = false) where T : class
        {
            var _db = LS.CurrentEntityContext;
            BulkResult bulkResult = new BulkResult();
            var type = typeof(T);
            if (type.BaseType.Name != "object")
            {
                type = type.BaseType;
            }
            var CreateScrname = (_db as IObjectContextAdapter).ObjectContext.CreateObjectSet<T>();
            var sqlCreate = CreateScrname.ToTraceString();

            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sqlCreate);

            string table = match.Groups["table"].Value;
            var fields = CreateScrname.EntitySet.ElementType.Properties;
            StringBuilder sb = new StringBuilder();
            string startSql = "";
            string endSql = @"
";
            string valSql = "";
            sb.Append(startSql);
            string sbdlm = "";
            var pType = typeof(T);
            var pInfo = pType.GetProperties();
            var plist = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo pi in pInfo)
            {
                plist.Add(pi.Name, pi);
            }

            string subSubDlm = "";

            subSubDlm = "";

            //            foreach (var p in fields)
            //            {
            //                //addFormat = "";
            //                //if (p.TypeUsage.EdmType.Name == "DateTime")
            //                //{
            //                //    addFormat = ", 120";
            //                //}

            //                //if (p.Nullable)
            //                //{
            //                //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
            //                //}
            //                //else
            //                //{
            //                //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
            //                //}
            //                if (p.Name == "ID") { continue; }
            //                valSql += subSubDlm + p.Name;
            //                subSubDlm = ", ";
            //            }
            //            sb.Append(valSql + @")
            //VALUES");
            sbdlm = @"UPDATE " + table + @" SET

";
            var defaultDateTime = new DateTime(0);
            int count = 0;
            var res = 0;
            if (returnLog)
            {
                bulkResult.SqlLog = new StringBuilder();
            }
            foreach (var item in list)
            {
                sb.Append(sbdlm + "");
                subSubDlm = "";
                foreach (var p in fields)
                {
                    if (p.Name == "ID") { continue; }
                    //addFormat = "";
                    if (plist.ContainsKey(p.Name))
                    {
                        var propinfo = plist[p.Name];
                        var val = propinfo.GetValue(item);
                        if (val == null)
                        {
                            if (updateNullableOrDefaulted)
                            {
                                sb.Append(subSubDlm + "[" + p.Name + "]=" + "NULL");
                            }
                            else
                            {
                                sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                            }
                        }
                        else
                            if (p.TypeUsage.EdmType.Name == "DateTime")
                            {
                                if (updateNullableOrDefaulted || (DateTime)val > defaultDateTime)
                                {
                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + "'" + ((DateTime)val).Year.ToString() + ((DateTime)val).Month.ToString("00") + ((DateTime)val).Day.ToString("00") + "'");
                                }
                                else
                                {
                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                }
                            }
                            else
                                if (p.TypeUsage.EdmType.Name == "String")
                                {
                                    if (updateNullableOrDefaulted || !string.IsNullOrEmpty((string)val))
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "N'" + ((string)val).Replace("'", @"''") + "'");
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else if (p.TypeUsage.EdmType.Name == "Boolean")
                                {
                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + (((bool)val) ? "1" : "0"));

                                }
                                else if (p.TypeUsage.EdmType.Name == "Guid")
                                {

                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + "CONVERT(uniqueidentifier, '" + ((Guid)val).ToString() + "')");
                                }
                                else if (p.TypeUsage.EdmType.Name == "Int32")
                                {
                                    if (updateNullableOrDefaulted || (int)val > 0)
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + val);
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else if (p.IsEnumType)
                                {
                                    if (updateNullableOrDefaulted || (int)val > 0)
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + (int)val);
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else if (p.TypeUsage.EdmType.Name == "Double")
                                {
                                    if (updateNullableOrDefaulted || (decimal)val > 0)
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + val);
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else if (p.TypeUsage.EdmType.Name == "Decimal")
                                {
                                    if (updateNullableOrDefaulted || (decimal)val > 0)
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + val);
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else
                                {
                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + val);
                                }
                        //if (p.Nullable)
                        //{
                        //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
                        //}
                        //else
                        //{
                        //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
                        //}


                        subSubDlm = ", ";
                    }
                }
                sb.Append(@" WHERE [ID] = " + plist["ID"].GetValue(item) + @" ;
");
                // sbdlm = @",";
                count++;
                if (count > 500)
                {
                    //flush porcion
                    sb.Append(endSql);
                    var sqlPorcion = sb.ToString();
                    bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sqlPorcion, parameters: new object[] { });
                    if (returnLog)
                    {
                        bulkResult.SqlLog.Append(@"--porcion 
" + sqlPorcion + @"
");
                    }
                    sb.Clear();
                    sb.Append(startSql);

                    //sbdlm = "";
                    count = 0;

                }
            }
            sb.Append(endSql);
            //INSERT INTO Production.UnitMeasure VALUES (N'FT2', N'Square Feet ', '20080923'), (N'Y', N'Yards', '20080923'), (N'Y3', N'Cubic Yards', '20080923');
            string sql = sb.ToString();

            if (count > 0)
            {
                bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sql, parameters: new object[] { });
                if (returnLog)
                {
                    bulkResult.SqlLog.Append(@"--porcion 
" + sql + @"
");
                }
            }
            return bulkResult;
        }
        //
        public static BulkResult SqlUpdateById<T>(this IEnumerable<ProductVariable<T>> list, bool updateNullableOrDefaulted = true, bool returnLog = false) where T : class
        {
            var _db = LS.CurrentEntityContext;
            BulkResult bulkResult = new BulkResult();
            var type = typeof(T);
            if (type.BaseType.Name != "object")
            {
                type = type.BaseType;
            }
            var CreateScrname = (_db as IObjectContextAdapter).ObjectContext.CreateObjectSet<T>();
            var sqlCreate = CreateScrname.ToTraceString();

            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sqlCreate);

            string table = match.Groups["table"].Value;
            var fields = CreateScrname.EntitySet.ElementType.Properties;
            StringBuilder sb = new StringBuilder();
            string startSql = "";
            string endSql = @"
";
            string valSql = "";
            sb.Append(startSql);
            string sbdlm = "";
            var pType = typeof(T);
            var pInfo = pType.GetProperties();
            var plist = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo pi in pInfo)
            {
                plist.Add(pi.Name, pi);
            }

            string subSubDlm = "";

            subSubDlm = "";

            //            foreach (var p in fields)
            //            {
            //                //addFormat = "";
            //                //if (p.TypeUsage.EdmType.Name == "DateTime")
            //                //{
            //                //    addFormat = ", 120";
            //                //}

            //                //if (p.Nullable)
            //                //{
            //                //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
            //                //}
            //                //else
            //                //{
            //                //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
            //                //}
            //                if (p.Name == "ID") { continue; }
            //                valSql += subSubDlm + p.Name;
            //                subSubDlm = ", ";
            //            }
            //            sb.Append(valSql + @")
            //VALUES");
            sbdlm = @"UPDATE " + table + @" SET

";
            var defaultDateTime = new DateTime(0);
            int count = 0;
            var res = 0;
            if (returnLog)
            {
                bulkResult.SqlLog = new StringBuilder();
            }
            foreach (var item in list)
            {
                sb.Append(sbdlm + "");
                subSubDlm = "";
                foreach (var p in fields)
                {
                    if (p.Name == "ID") { continue; }
                    //addFormat = "";
                    if (item.Fields.Contains(p.Name) && plist.ContainsKey(p.Name))
                    {
                        var propinfo = plist[p.Name];
                        var val = propinfo.GetValue(item.Entity);
                        if (val == null)
                        {
                            if (updateNullableOrDefaulted)
                            {
                                sb.Append(subSubDlm + "[" + p.Name + "]=" + "NULL");
                            }
                            else
                            {
                                sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                            }
                        }
                        else
                            if (p.TypeUsage.EdmType.Name == "DateTime")
                            {
                                if (updateNullableOrDefaulted || (DateTime)val > defaultDateTime)
                                {
                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + "'" + ((DateTime)val).Year.ToString() + ((DateTime)val).Month.ToString("00") + ((DateTime)val).Day.ToString("00") + "'");
                                }
                                else
                                {
                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                }
                            }
                            else
                                if (p.TypeUsage.EdmType.Name == "String")
                                {
                                    if (updateNullableOrDefaulted || !string.IsNullOrEmpty((string)val))
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "N'" + ((string)val).Replace("'", @"''") + "'");
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else if (p.TypeUsage.EdmType.Name == "Boolean")
                                {
                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + (((bool)val) ? "1" : "0"));

                                }
                                else if (p.TypeUsage.EdmType.Name == "Guid")
                                {

                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + "CONVERT(uniqueidentifier, '" + ((Guid)val).ToString() + "')");
                                }
                                else if (p.TypeUsage.EdmType.Name == "Int32")
                                {
                                    if (updateNullableOrDefaulted || (int)val > 0)
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + val);
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else if (p.IsEnumType)
                                {
                                    if (updateNullableOrDefaulted || (int)val > 0)
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + (int)val);
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else if (p.TypeUsage.EdmType.Name == "Double")
                                {
                                    if (updateNullableOrDefaulted || (decimal)val > 0)
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]="
                                            + val.ToString().Replace(",", ".")
                                           );
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else if (p.TypeUsage.EdmType.Name == "Decimal")
                                {
                                    if (updateNullableOrDefaulted || (decimal)val > 0)
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]="
                                            + val.ToString().Replace(",", ".")
                                           );
                                    }
                                    else
                                    {
                                        sb.Append(subSubDlm + "[" + p.Name + "]=" + "[" + p.Name + "]");
                                    }
                                }
                                else
                                {
                                    sb.Append(subSubDlm + "[" + p.Name + "]=" + val);
                                }
                        //if (p.Nullable)
                        //{
                        //    sb.Append(sbdlm + "+ISNULL(CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + "),N'')");
                        //}
                        //else
                        //{
                        //    sb.Append(sbdlm + "+CONVERT(nvarchar(MAX), [" + p.Name + "]" + addFormat + ")");
                        //}


                        subSubDlm = ", ";
                    }
                }
                sb.Append(@" WHERE [ID] = " + plist["ID"].GetValue(item.Entity) + @" ;
");
                // sbdlm = @",";
                count++;
                if (count > 500)
                {
                    //flush porcion
                    sb.Append(endSql);
                    var sqlPorcion = sb.ToString();
                    bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sqlPorcion, parameters: new object[] { });
                    if (returnLog)
                    {
                        bulkResult.SqlLog.Append(@"--porcion 
" + sqlPorcion + @"
");
                    }
                    sb.Clear();
                    sb.Append(startSql);

                    //sbdlm = "";
                    count = 0;

                }
            }
            sb.Append(endSql);
            //INSERT INTO Production.UnitMeasure VALUES (N'FT2', N'Square Feet ', '20080923'), (N'Y', N'Yards', '20080923'), (N'Y3', N'Cubic Yards', '20080923');
            string sql = sb.ToString();

            if (count > 0)
            {
                bulkResult.DBreturn += _db.Database.ExecuteSqlCommand(sql, parameters: new object[] { });
                if (returnLog)
                {
                    bulkResult.SqlLog.Append(@"--porcion 
" + sql + @"
");
                }
            }
            return bulkResult;
        }

    }
    public class BulkResult
    {
        public BulkResult()
        {
            SqlLog = new StringBuilder();
        }
        public int DBreturn { get; set; }
        public StringBuilder SqlLog { get; set; }
        public string CustomData { get; set; }

    }
}