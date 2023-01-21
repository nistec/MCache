using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Nistec.Data;
using Nistec.Data.Factory;
using Nistec.Data.Entities;
using Nistec.Data.Entities.Cache;
using Nistec.Generic;

namespace Nistec.Caching.Demo.DB
{
   
    #region ResourceLang

    [Serializable]
    public class NetcellResources : EntityLocalizer//EntityLang
    {
        public static CultureInfo GetCulture()
        {
            return new CultureInfo( "he-IL");
        }
        #region override

        protected override string CurrentCulture()
        {
            return GetCulture().Name;
        }

        protected override void BindLocalizer()
        {
        
            //init by config key
            //base.Init(CurrentCulture(), "DataEntityDemo.Resources.AdventureWorks");
            //or
            base.Init("DataEntityDemo.Resources.Netcell");
            //or
            //base.Init(Nistec.Sys.NetResourceManager.GetResourceManager("DataEntityDemo.Resources.AdventureWorks", this.GetType()));
            //or
            //base.Init(Nistec.Sys.NetResourceManager.GetResourceManager( "DataEntityDemo.Resources.AdventureWorks",this.GetType()));
        }
        #endregion
    }
    #endregion

    #region DbContext

    [DbContext("Netcell_Docs")]
    [Serializable]
    public class Netcell_Docs : DbContext
    {
        #region static

        public const bool EnableCache = true;
        private static readonly Netcell_Docs _instance = new Netcell_Docs();

        public static Netcell_Docs Instance
        {
            get { return _instance; }
        }

        public static string Cnn
        {
            get { return NetConfig.ConnectionString("Netcell_Docs"); }
        }

        public Netcell_Docs()
        {
        }

        #endregion

        #region override

        protected override void EntityBind()
        {
            //base.SetConnection("AdventureWorks", Cnn, DBProvider.SqlServer);
            //base.Items.SetEntity("Contact", "Accounts", EntitySourceType.Table, new EntityKeys("AccountId"));
            //base.SetEntity<ActiveContact>();
        }

        public override ILocalizer Localization
        {
            get
            {
                return base.GetLocalizer<NetcellResources>();
            }
        }
         #endregion

        #region Entities

        static EntityDbCache cache;

        public EntityDbCache Cache
        {
            get
            {
                if (cache == null)
                {
                    cache = new EntityDbCache(this);
                }
                return cache;
            }
        }

        //public EntityDbContext Contact { get { return this.DbEntities.Get("Contact", "Accounts", EntitySourceType.Table, EntityKeys.Get("AccountId"), EnableCache); } }

        #endregion

    }
    #endregion

    
    [DbContext("Netcell_Stg")]
    [Serializable]
    public class Netcell_Stg : DbContext
    {
        #region static

        public const bool EnableCache = true;
        private static readonly Netcell_Stg _instance = new Netcell_Stg();

        public static Netcell_Stg Instance
        {
            get { return _instance; }
        }

        public static string Cnn
        {
            get { return NetConfig.ConnectionString("Netcell_Stg"); }
        }

        public Netcell_Stg()
        {
        }

        #endregion

        #region override

        protected override void EntityBind()
        {
            //base.SetConnection("AdventureWorks", Cnn, DBProvider.SqlServer);
            //base.Items.SetEntity("Contact", "Accounts", EntitySourceType.Table, new EntityKeys("AccountId"));
            //base.SetEntity<ActiveContact>();
        }

        public override ILocalizer Localization
        {
            get
            {
                return base.GetLocalizer<NetcellResources>();
            }
        }
        #endregion

        #region Entities

        static EntityDbCache cache;

        public EntityDbCache Cache
        {
            get
            {
                if (cache == null)
                {
                    cache = new EntityDbCache(this);
                }
                return cache;
            }
        }

        //public EntityDbContext Contact { get { return this.DbEntities.Get("Contact", "Accounts", EntitySourceType.Table, EntityKeys.Get("AccountId"), EnableCache); } }

        #endregion

    }
}
