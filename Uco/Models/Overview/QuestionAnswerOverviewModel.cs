using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class QuestionAnswerOverviewModel
    {
        public int ID { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string CreateDate { get; set; }
    }
}