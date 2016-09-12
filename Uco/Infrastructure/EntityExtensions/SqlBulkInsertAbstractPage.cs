using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.EntityExtensions
{
	public static int SqlBulkInsertAbstractPage<G, T>(this IList<T> list) where T : class where G : class
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
		string startSql = @"INSERT INTO " + table + @" (";
		string endSql = @";";
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
		sb.Append(valSql + @") VALUES");

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
							else if (p.PropertyType.FullName.Contains("Decimal"))
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
			sb.Append(@") ");
			sbdlm = @",";
			count++;
			if (count > 500)
			{
				//flush porcion
				sb.Append(endSql);
				var sqlPorcion = sb.ToString();
				res += _db.Database.ExecuteSqlCommand(sqlPorcion, new object[] { });
				sb.Clear();
				sb.Append(startSql);
				sb.Append(valSql + @") VALUES");
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
}