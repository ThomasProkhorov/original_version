/*
	string TextToSearch = "";
	var s = FtsInterceptor.Fts(TextToSearch);
	List<AbstractPage> List = AbstractPages.Where(r =>
		r.Title.Contains(s)
		|| r.ShortDescription.Contains(s)
		|| r.Text.Contains(s)
		).ToList();     
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Infrastructure.Interception;

using System.Text.RegularExpressions;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;
using System.Data.Common;
using System.Data;

namespace Uco.Infrastructure.EntityExtensions
{
    public class FullTextSearch : IDbCommandInterceptor
    {
        private const string FullTextPrefix = "-FTSPREFIX-";
        public static string Fts(string search)
        {
          //  return search;
            return string.Format("{0}{1}", FullTextPrefix, search);
        }
        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }
        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            RewriteFullTextQuery(command);
        }
        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }
        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            RewriteFullTextQuery(command);
        }
        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }
        public static void RewriteFullTextQuery(DbCommand cmd)
        {
            string text = cmd.CommandText;
            for (int i = 0; i < cmd.Parameters.Count; i++)
            {
               // break;
                DbParameter parameter = cmd.Parameters[i];
                if (parameter.DbType.In(DbType.String, DbType.AnsiString, DbType.StringFixedLength, DbType.AnsiStringFixedLength))
                {
                    if (parameter.Value == DBNull.Value)
                        continue;
                    var value = (string)parameter.Value;
                    if (value.IndexOf(FullTextPrefix) >= 0)
                    {
                        parameter.Size = 4096;
                        parameter.DbType = DbType.AnsiStringFixedLength;
                        value = value.Replace(FullTextPrefix, ""); // remove prefix we added n linq query
                        value = value.Replace(",", "");
                        value = "\""+value.Substring(1, value.Length - 2).Replace(" ","\" AND \"")+"\""; // remove %% escaping by linq translator from string.Contains to sql LIKE
                        parameter.Value = value;
                        cmd.CommandText = Regex.Replace(text,
                        string.Format(
                        @"\[(\w*)\].\[(\w*)\]\s*LIKE\s*@{0}\s?(?:ESCAPE N?'~')", parameter.ParameterName),
                        string.Format(@"contains([$1].[$2], @{0})", parameter.ParameterName));
                        if (text == cmd.CommandText)
                            throw new Exception("FTS was not replaced on: " + text);
                        text = cmd.CommandText;
                    }
                }
            }
        }
    }
    static class LanguageExtensions
    {
        public static bool In<T>(this T source, params T[] list)
        {
            return (list as IList<T>).Contains(source);
        }
    }
}