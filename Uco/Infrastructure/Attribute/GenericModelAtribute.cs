using System;
using System.Linq.Expressions;

namespace Uco.Infrastructure
{
    //General class attributes
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ModelGeneralAttribute : Attribute
    {
        public string Role { get; set; } //bind to role, empty for general
        public string Controller { get; set; }
        public string Action { get; set; }
         
        public bool Cached = false;
        public bool Show = true; //show
        public bool Edit = false; //is editable
        public bool AjaxEdit = true; // is ajax edit 
        public string EditMode = "inline"; //popup - ajax edit create mod
        public bool Create = true; //is createable
        public bool CreateAjax = false; // is quick createable
        public bool Delete = true; // is deleteable
        public bool DependedShow = true;//is show child grid
        public bool Acl = false;
        public string EditText = null;
        public bool CanBack = true;
        public bool SubmitButton = true;
    }

    //model property attributes
    [AttributeUsage(AttributeTargets.Property)]
    public class SeoUrlAttribute : Attribute
    {
        public string TitleField="Title";
        public string NameField="Name";
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ModelAttribute : Attribute
    {
        public string Role { get; set; }
        public bool Show = true;
        private bool? _showInGrid { get; set; }
        public bool ShowInGrid { get {
            if (_showInGrid.HasValue) { return _showInGrid.Value; }
            return Show;
        }
            set {
                _showInGrid = value;
            }
        }
        private bool? _showInEdit { get; set; }
        public bool ShowInEdit
        {
            get
            {
                if (_showInEdit.HasValue) { return _showInEdit.Value; }
                return Show;
            }
            set
            {
                _showInEdit = value;
            }
        }
        public bool Edit = true;
        public bool AjaxEdit = true;
        public bool Filter = false;
        public bool FilterOnTop = false;
        public bool Sort = false;
        public bool PreOrder = false;
        public bool PreOrderDesc = false;
        public bool IsDropDownName = false;
        public bool ShowInParentGrid = false;//show sub oject field in new column
        public bool ForeignName = false; // for showing in dropdown
        public string ForeignKey = null;
        public string KeyField = null;
    }
}