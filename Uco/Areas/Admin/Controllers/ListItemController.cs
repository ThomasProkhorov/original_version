using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Infrastructure;
using Uco.Models;
using System.Data;
using System.Data.Entity;
using Uco.ActiveTrailApi.CustomersServiceProxy;

namespace Uco.Areas.Admin.Controllers
{
    public class ListItemController : BaseAdminController
    {
        #region XmlContact

        public ActionResult XmlContacts(int ID, string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View(ID);
        }

        public ActionResult _XmlContacts_AjaxIndex([DataSourceRequest]DataSourceRequest request, int ParentID)
        {
            List<XmlContact> l = new List<XmlContact>();
            Uco.Models.PageLocation p = _db.PageLocations.FirstOrDefault(r => r.ID == ParentID);
            try
            {
                if (p != null) l = p.GetContactsFromXML<XmlContact>();
            }
            catch { }

            DataSourceResult result = l.OrderBy(r => r.Order).ToDataSourceResult(request);

            return Json(result);

        }


        public ActionResult DeleteXmlContact(int ID, int ParentID, string ReturnUrl)
        {
            List<XmlContact> l = new List<XmlContact>();
            Uco.Models.PageLocation p = _db.PageLocations.FirstOrDefault(r => r.ID == ParentID);
            if (p != null) l = p.GetContactsFromXML<XmlContact>();

            l.RemoveAll(r => r.ID == ID);

            p.SetContactsToXML<XmlContact>(l);
            _db.Entry(p).State = EntityState.Modified;
            _db.SaveChanges();

            return Redirect(Url.Content("/Admin/Main/Edit/" + ParentID + "#TabStrip-6"));
        }

        public ActionResult CreateXmlContact(int ParentID, string ReturnUrl)
        {
            ViewBag.ParentID = ParentID;
            ViewBag.ReturnUrl = ReturnUrl;
            return View("CreateXMLContact", "", new XmlContact());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateXmlContact(XmlContact XMLGrid, int ParentID, string ReturnUrl)
        {

            List<XmlContact> l = new List<XmlContact>();
            Uco.Models.PageLocation p = _db.PageLocations.FirstOrDefault(r => r.ID == ParentID);
            if (p != null) l = p.GetContactsFromXML<XmlContact>();

            if (l.Count != 0)
            {
                XMLGrid.ID = l.Max(r => r.ID) + 1;
                XMLGrid.Order = l.Max(r => r.Order) + 1;
            }
            else
            {
                XMLGrid.ID = 1;
                XMLGrid.Order = 1;
            }
            l.Add(XMLGrid);

            p.SetContactsToXML<XmlContact>(l);
            _db.Entry(p).State = EntityState.Modified;
            _db.SaveChanges();
            //ViewBag.ReturnUrl = Url.Content("/Admin/Main/Edit/" + ParentID);
            return Redirect(Url.Content("/Admin/Main/Edit/" + ParentID));
        }

        public ActionResult EditXmlContact(int ParentID, int ID, string ReturnUrl)
        {
            ViewBag.ParentID = ParentID;
            ViewBag.ReturnUrl = ReturnUrl;

            List<XmlContact> l = new List<XmlContact>();
            Uco.Models.PageLocation p = _db.PageLocations.FirstOrDefault(r => r.ID == ParentID);
            if (p != null) l = p.GetContactsFromXML<XmlContact>();

            XmlContact current_xml = l.Where(r => r.ID == ID).FirstOrDefault();
            return View("EditXMLContact", "", current_xml);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditXmlContact(XmlContact curent_xml, int ParentID, string ReturnUrl)
        {
            List<XmlContact> l = new List<XmlContact>();
            Uco.Models.PageLocation p = _db.PageLocations.FirstOrDefault(r => r.ID == ParentID);
            if (p != null) l = p.GetContactsFromXML<XmlContact>();
            l.RemoveAll(r => r.ID == curent_xml.ID);
            l.Add(curent_xml);
            p.SetContactsToXML<XmlContact>(l);
            _db.Entry(p).State = EntityState.Modified;
            _db.SaveChanges();
            //ViewBag.ReturnUrl = Url.Content("/Admin/Main/Edit/" + ParentID);
            return Redirect(Url.Content("/Admin/Main/Edit/" + ParentID));
        }

        #endregion
    }
}
