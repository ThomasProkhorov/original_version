using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class TreeModel
    {
        public TreeModel()
        {
            Childrens = new List<TreeModel>();
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int ParentID { get; set; }
        public List<TreeModel> Childrens { get; set; }
    }
}