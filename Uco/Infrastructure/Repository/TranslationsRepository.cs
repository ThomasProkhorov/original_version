using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Repositories
{
    public static partial class RP
    {

        #region Get/Clean Repository

        public static void CleanTranslationsRepository()
        {
            foreach (string item in System.Configuration.ConfigurationManager.AppSettings["Languages"].Split(','))
            {
                LS.Cache.Remove(string.Format("Translation_{0}", item));
            }
        }

        public static List<Translation> GetTranslationsReprository()
        {
            return LS.Get<Translation>();
            string LanguageCode = SF.GetLangCodeThreading();//RP.GetCurrentSettings().LanguageCode; //<- bug 
            string Token = "Translation_" + LanguageCode;

            if (LS.Cache[Token] == null)
            {
                List<Translation> l = new List<Translation>();
                l = _db.Translations.Where(r => r.LangCode == LanguageCode).ToList();

                LS.Cache[Token] = l;
                return l;
            }
            else return LS.Cache[Token] as List<Translation>;
        }

        #endregion

        #region Get/Set single

        public static bool Mbool(out string Res, string ModelName, string ModelField, string group = "")
        {
            var SystemName = string.Format("Models{2}.{0}.{1}", ModelName, ModelField, group);
            Translation tc = GetTranslationsReprository().FirstOrDefault(r => r.SystemName == SystemName);
            if (tc != null)
            {
                Res=  tc.Text;
                return true;
            }
            Translation addnew = new Translation();
            addnew.SystemName = SystemName;
            addnew.Text = SystemName;
            _db.Translations.Add(addnew);
            _db.SaveChanges();
            Res = SystemName;
            return false;
        }
        public static bool Madd(string ModelName, string ModelField, string group = "",string Value="")
        {
            var SystemName = string.Format("Models{2}.{0}.{1}", ModelName, ModelField, group);

            Translation tc = GetTranslationsReprository().FirstOrDefault(r => r.SystemName == SystemName);
            if (tc == null)
            {

                Translation addnew = new Translation();
                addnew.SystemName = SystemName;
                if (!string.IsNullOrEmpty(Value))
                {
                    addnew.Text = Value;
                }
                else
                {
                    addnew.Text = ModelField;
                }
                _db.Translations.Add(addnew);
                _db.SaveChanges();
                return true;
            }
            else {
                var tr = _db.Translations.FirstOrDefault(r => r.ID == tc.ID);
                tr.Text = Value;
                _db.SaveChanges();
            }

            return false;
        }
        public static string M(string ModelName,string ModelField,string group="")
        {
            var prefix = string.Format("Models{1}.{0}.", ModelName,  group);
            var SystemName = prefix+ ModelField;
            
            Translation tc = GetTranslationsReprository().FirstOrDefault(r => r.SystemName == SystemName);
            if(tc!= null)
            {
                if(tc.Text == null)
                {
                    return tc.Text;
                }
                return tc.Text;//.Replace(prefix,"");
            }
            Translation addnew = new Translation();
            addnew.SystemName = SystemName;
            addnew.Text = SystemName;
            _db.Translations.Add(addnew);
            _db.SaveChanges();
            return SystemName.Replace(prefix, "");
        }
        public static IHtmlString T(string SystemName)
        {
            Translation tc = GetTranslationsReprository().FirstOrDefault(r => r.SystemName == SystemName);
            if (tc == null)
            {
             //if not found try insert it
                if (!string.IsNullOrEmpty(SystemName) && true) // for future can be setting in web config
                {

                    Translation addnew = new Translation();
                    addnew.SystemName = SystemName;
                    addnew.Text = SystemName;
                    _db.Translations.Add(addnew);
                    _db.SaveChanges();
                }
                //return key
                return new HtmlString(SystemName);
            }
            else return new HtmlString(tc.Text);
        }
        public static string S(string SystemName)
        {
            Translation tc = GetTranslationsReprository().FirstOrDefault(r => r.SystemName == SystemName);
            if (tc == null)
            {
                //if not found try insert it
                if (!string.IsNullOrEmpty(SystemName) && true) // for future can be setting in web config
                {

                    Translation addnew = new Translation();
                    addnew.SystemName = SystemName;
                    addnew.Text = SystemName;
                    _db.Translations.Add(addnew);
                    _db.SaveChanges();
                }
                //return key
                return SystemName;
            }
            else return tc.Text;
        }
        #endregion
    }
}