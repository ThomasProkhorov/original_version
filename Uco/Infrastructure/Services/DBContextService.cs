using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Services
{
    public  class DBContextService
    {
        public DBContextService(Db _db = null)
        {
            _CurDb = _db;
            if (_CurDb == null)
            {
               
                    _CurDb = new Db();
                
            }
           
        }
        private  Db _CurDb = null;
       
         public Db EntityContext
        {
            get
            {
                return _CurDb;
            }
        }
    }
}