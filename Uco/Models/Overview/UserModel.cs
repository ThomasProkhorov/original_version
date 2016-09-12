using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Uco.Infrastructure;

namespace Uco.Models.Overview
{
    public class UserModel
    {
        [Required(ErrorMessage = "First name required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name required")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessageResourceType = typeof(Uco.Models.Resources.SystemModels), ErrorMessageResourceName = "EmailRequired")]
        [UcoEmail()]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone required")]
        public string Phone { get; set; }

        public string CompanyName { get; set; }

        public bool NewsLetter { get; set; }
    }
    public class UserPasswordModel
    {

    }
}