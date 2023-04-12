using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Resources;
using Nistec.Data.Entities;

namespace Nistec.Legacy
{
    #region IEntityLang

    /// <summary>
    /// EntityLang interface
    /// </summary>
    public interface IEntityLang
    {
        CultureInfo Culture { get; set; }
        T GetValue<T>(string name);
        T GetValue<T>(CultureInfo culture, string name);
        
        string GetString(string name);
        string GetString(string name, string defaultValue);
        string GetString(CultureInfo culture, string name);
        string GetString(CultureInfo culture, string name, string defaultValue);
    }
    #endregion

    #region DynamicEntityLang
    /// <summary>
    /// DynamicEntityLang
    /// </summary>
    public sealed class DynamicEntityLang : EntityLang
    {
        string currentCulture;
        public DynamicEntityLang(string cultuer,string resource, Type type)
        {
            currentCulture = cultuer;
            base.Init(cultuer, NetResourceManager.GetResourceManager(resource, type));
        }

        internal DynamicEntityLang(IEntity instance, string resource)
        {
            currentCulture = instance.EntityDb.EntityCulture.Name;
            base.Init(currentCulture, NetResourceManager.GetResourceManager(resource, instance.GetType()));
         }

        protected override string CurrentCulture()
        {
            return currentCulture;
        }
        protected override void LangBind()
        {
        }
    }
    #endregion

    #region EntityLang

    /// <summary>
    /// EntityLang
    /// </summary>
    public abstract class EntityLang : NetResourceManager, IEntityLang
    {
        public static CultureInfo DefaultCulture { get { return CultureInfo.CurrentCulture; } }

        protected abstract void LangBind();
        protected abstract string CurrentCulture();

        protected void Init(string resource)
        {
            ResourceManager rm= NetResourceManager.GetResourceManager(resource, this.GetType());
            base.Init(CurrentCulture(), rm);
        }

        protected void Init(ResourceManager rm)
        {
            base.Init(CurrentCulture(), rm);
        }

        public EntityLang()
        {
            LangBind();
        }

        #region static IEntityRM


        static IEntityLang Create<Erm>() where Erm : IEntityLang
        {
            return Activator.CreateInstance<Erm>();
        }

        public static IEntityLang Get<Erm>() where Erm : IEntityLang
        {
            IEntityLang rm = null;
            string name = typeof(Erm).Name;
            if (!Hash.TryGetValue(name, out rm))
            {
                rm = Create<Erm>();
                Hash[name] = rm;
            }
            return rm;
        }

        private static Dictionary<string, IEntityLang> m_hash;

        private static Dictionary<string, IEntityLang> Hash
        {
            get
            {
                if (m_hash == null)
                {
                    m_hash = new Dictionary<string, IEntityLang>();
                }
                return m_hash;
            }
        }
        #endregion



        /*
         * 

        #region Ctor
        private static NetRM loader;
        public readonly static CultureInfo DefualtCulture;

        //private static Hashtable	_Cultures= new Hashtable();
        private static CultureInfo _CultureInfo;


        //static NetRM()
        //{
        //    NetRM.DefualtCulture=new CultureInfo("en",false);
        //    NetRM.loader = null;
        //    NetRM.SetCultures();
        //    NetRM.Culture=CultureInfo.CurrentCulture;
        //}

        public EntityRM(string culter, string configKey)
        {
            _CultureInfo = new CultureInfo(culter, false);
            this.resources = new ResourceManager(Configuration.NetConfig.AppSettings[configKey], Assembly.GetExecutingAssembly());

            //this.resources = new ResourceManager("Nistec.Framework.Resources.SR", Assembly.GetExecutingAssembly());
            //this.resources = new ResourceManager("Nistec.Framework.Resources.SR", base.GetType().Module.Assembly);
        }

        public NetRM(string culter, string resource, Assembly assembly)
        {
            _CultureInfo = new CultureInfo(culter, false);

            this.resources = new ResourceManager(resource, assembly);
            //this.resources = new ResourceManager("Nistec.Framework.Resources.SR", base.GetType().Module.Assembly);
        }

        public NetRM(string culter, ResourceManager resource)
        {
            _CultureInfo = new CultureInfo(culter, false);
            this.resources = resource;
        }

        #endregion

        #region Cultures

        //public static Hashtable Cultures
        //{
        //    get { return _Cultures; }
        //}

        //private static void SetCultures()
        //{
        //    _Cultures.Add("en","English");
        //    _Cultures.Add("he","Hebrew");
        //}

        //public static bool isCultureSopperted(string clt)
        //{
        //    return _Cultures.Contains(clt);
        //}

        private static CultureInfo Culture
        {
            get
            {
                if (_CultureInfo == null)
                {
                    _CultureInfo = NetRM.DefualtCulture;
                }
                return _CultureInfo;
            }
            set
            {
                string cultureName = value.TwoLetterISOLanguageName;//.Name.Substring(0,2);
                _CultureInfo = new CultureInfo(cultureName, false);
                //if(isCultureSopperted(cultureName))
                //{
                //    _CultureInfo= new CultureInfo(cultureName,false);
                //}
                //else
                //{
                //    _CultureInfo= NetRM.DefualtCulture;
                //}
            }
        }

        //		private CultureInfo GetCurrentCulture()
        //		{
        //			//_CultureInfo=CultureInfo.CurrentCulture;
        //			string cultureName=CultureInfo.CurrentCulture.Name.PadLeft(2);
        //			if(isCultureSopperted(cultureName))
        //			{
        //				_CultureInfo= new CultureInfo(cultureName,false);
        //			}
        //			else
        //			{
        //				_CultureInfo= NetRM.DefualtCulture;
        //			}
        //			return _CultureInfo;
        //		}

        #endregion

       

        private static NetRM GetLoader()
        {
            if (NetRM.loader == null)
            {
                lock (typeof(NetRM))
                {
                    if (NetRM.loader == null)
                    {
                        NetRM.loader = new NetRM();
                    }
                }
            }
            return NetRM.loader;
        }

        */
    }
    #endregion
}
