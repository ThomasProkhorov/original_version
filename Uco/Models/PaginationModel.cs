using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Serialization;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;

namespace Uco.Models
{
    public class Pagination
    {
        public int PageTotal { get; set; }
        public int PageNumber { get; set; }
        public int PageItems { get; set; }
        public string Url { get; set; }

        public Pagination()
        {
            this.PageTotal = 20;
            this.PageNumber = 1;
            this.PageItems = 20;
            this.Url = "/";
        }

        public Pagination(int _PageTotal, int _PageNumber, int _PageItems, string _Url)
        {
            this.PageTotal = _PageTotal;
            this.PageNumber = _PageNumber;
            this.PageItems = _PageItems;
            this.Url = _Url;
        }
    }

}