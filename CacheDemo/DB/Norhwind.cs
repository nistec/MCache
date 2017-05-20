using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Data.Entities;
using Nistec.Data;
using Nistec.Generic;

namespace Nistec.Caching.Demo.DB
{
   
    #region ResourceLang

    public class NorhwindResources : EntityLocalizer
    {
 
        protected override string CurrentCulture()
        {
            return "he-IL";
        }

        protected override void BindLocalizer()
        {
            //init by config key
            //base.Init(CurrentCulture(), "DataEntityDemo.Resources.Norhwind");
            //or
            base.Init("MCacheDemo.Resources.Norhwind");
            //or
            //base.Init(Nistec.Sys.NetResourceManager.GetResourceManager("DataEntityDemo.Resources.Norhwind", this.GetType()));
            //or
            //base.Init(Nistec.Sys.NetResourceManager.GetResourceManager( "DataEntityDemo.Resources.Norhwind",this.GetType()));
        }
    }
    #endregion

    #region DbContext

    public class Norhwind : DbContext
    {
        public static string Cnn
        {
            get { return NetConfig.ConnectionString("Norhwind"); }
        }

        protected override void EntityBind()
        {
            base.SetConnection("Norhwind", Cnn, DBProvider.OleDb);
        }

        public override ILocalizer Localization
        {
            get
            {
                return base.GetLocalizer<NorhwindResources>();
            }
        }
    }
    #endregion

}
