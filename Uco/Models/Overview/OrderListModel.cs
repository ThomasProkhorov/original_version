using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class OrderListModel
    {
        public int NewCount { get; set; }
        public int PayedCount { get; set; }
        public int NewAndPyedCount { get; set; }
        public int AcceptedCount { get; set; }
        public int SentCount { get; set; }
        public int DelivereCount { get; set; }
        public int CanceledCount { get; set; }
        public int RejectedCount { get; set; }
    }
}