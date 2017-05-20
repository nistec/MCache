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
    public class AdventureWorksResources : EntityLocalizer//EntityLang
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
            base.Init("DataEntityDemo.Resources.AdventureWorks");
            //or
            //base.Init(Nistec.Sys.NetResourceManager.GetResourceManager("DataEntityDemo.Resources.AdventureWorks", this.GetType()));
            //or
            //base.Init(Nistec.Sys.NetResourceManager.GetResourceManager( "DataEntityDemo.Resources.AdventureWorks",this.GetType()));
        }
        #endregion
    }
    #endregion

    #region DbContext

    [DbContext( "AdventureWorks")]
    [Serializable]
    public class AdventureWorks : DbContext
    {
        #region static

        public const bool EnableCache = true;
        private static readonly AdventureWorks _instance = new AdventureWorks();

        public static AdventureWorks Instance
        {
            get { return _instance; }
        }

        public static string Cnn
        {
            get { return NetConfig.ConnectionString("AdventureWorks"); }
        }

        public AdventureWorks()
        {
        }

        #endregion

        #region override

        protected override void EntityBind()
        {
            //base.SetConnection("AdventureWorks", Cnn, DBProvider.SqlServer);
            //base.Items.SetEntity("Contact", "Person.Contact", EntitySourceType.Table, new EntityKeys("ContactID"));
            //base.SetEntity<ActiveContact>();
        }

        public override ILocalizer Localization
        {
            get
            {
                return base.GetLocalizer<AdventureWorksResources>();
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

        public EntityDbContext Contact { get { return this.DbEntities.Get("Contact", "Person.Contact", EntitySourceType.Table, EntityKeys.Get("ContactID"), EnableCache); } }

        #endregion

    }
    #endregion
}
