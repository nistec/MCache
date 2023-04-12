using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Globalization;
using System.Reflection;
using System.Collections;
using Nistec.Generic;

namespace Nistec.Legacy
{
    public class NetResourceManager
    {

        
		#region Ctor
		private ResourceManager resources;
		//private static NetRM loader;
		public static	CultureInfo DefualtCulture;

		//private static Hashtable	_Cultures= new Hashtable();
		private	static CultureInfo	_CultureInfo;


        //static NetRM()
        //{
        //    NetRM.DefualtCulture=new CultureInfo("en",false);
        //    NetRM.loader = null;
        //    NetRM.SetCultures();
        //    NetRM.Culture=CultureInfo.CurrentCulture;
        //}

        public static ResourceManager GetResourceManager(string resource,Type type)
        {
            return new ResourceManager(resource, type.Assembly);
        }

        public static ResourceManager GetResourceManager(string resource)
        {
            return new ResourceManager(resource, Assembly.GetExecutingAssembly());
        }

        protected void Init(string culture, string configKey)
        {
            _CultureInfo = new CultureInfo(culture, false);
            this.resources = new ResourceManager(NetConfig.AppSettings[configKey], Assembly.GetExecutingAssembly());
        }

        protected void Init(string culture, ResourceManager resource)
        {
            _CultureInfo = new CultureInfo(culture, false);
            this.resources = resource;
        }


        protected void Init(string culture, string resource, Assembly assembly)
        {
            _CultureInfo = new CultureInfo(culture, false);

            this.resources = new ResourceManager(resource, assembly);
            //this.resources = new ResourceManager("Nistec.Framework.Resources.SR", base.GetType().Module.Assembly);
        }

        public NetResourceManager()
        {
            _CultureInfo = CultureInfo.CurrentCulture;
        }

        public NetResourceManager(string culture, string configKey)
        {

            _CultureInfo = new CultureInfo(culture, false);
            this.resources = new ResourceManager(NetConfig.AppSettings[configKey], Assembly.GetExecutingAssembly());

            //this.resources = new ResourceManager("Nistec.Framework.Resources.SR", Assembly.GetExecutingAssembly());
            //this.resources = new ResourceManager("Nistec.Framework.Resources.SR", base.GetType().Module.Assembly);
        }

        public NetResourceManager(string culture, string resource, Assembly assembly)
        {
            _CultureInfo = new CultureInfo(culture, false);
			
            this.resources = new ResourceManager(resource, assembly);
            //this.resources = new ResourceManager("Nistec.Framework.Resources.SR", base.GetType().Module.Assembly);
        }

        public NetResourceManager(string culture, ResourceManager resource)
        {
            _CultureInfo = new CultureInfo(culture, false);
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


        public ResourceManager RM
        {
            get { return resources; }
        }

		public  CultureInfo Culture
		{
			get
			{
				if(_CultureInfo==null)
				{
                    _CultureInfo = NetResourceManager.DefualtCulture;
				}
				return _CultureInfo;
			} 
			set
			{
				//string cultureName= value.TwoLetterISOLanguageName;//.Name.Substring(0,2);
                //_CultureInfo = new CultureInfo(cultureName, false);
                _CultureInfo = value;

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
 
		#region Methods


        //private NetRM GetLoader()
        //{
        //    if (NetRM.loader == null)
        //    {
        //        lock (typeof(NetRM))
        //        {
        //            if (NetRM.loader == null)
        //            {
        //                NetRM.loader = new NetRM();
        //            }
        //        }
        //    }
        //    return NetRM.loader;
        //}


		public object GetObject(string name)
		{
            return GetObject(null, name);
		}

		public object GetObject(CultureInfo culture, string name)
		{
	        if (RM == null)
			{
				return null;
			}
            return RM.GetObject(name, culture);
		}


        public T GetValue<T>(string name)
        {
            return GenericTypes.Convert<T>(GetString(name));
        }

        public T GetValue<T>(CultureInfo culture, string name)
        {
            return GenericTypes.Convert<T>(GetString(culture, name));
        }

        public string GetString(string name, string defaultValue)
		{
            return GetString(null, name, defaultValue);
		}

        public string GetString(string name)
        {
            return GetString(null, name, name);
        }

        public string GetString(CultureInfo culture, string name)
        {
            return GetString(culture, name, name);
        }

        public string GetString(CultureInfo culture, string name, string defaultValue)
        {
           try
            {
                if (RM == null)
                {
                    return defaultValue;
                }
                if (culture == null)
                {
                    culture = Culture;// CultureInfo.CurrentCulture;
                }
                //else if(!isCultureSopperted(culture.Name))
                //{
                //    culture=Culture;
                //}
                return GenericTypes.NZorEmpty(resources.GetString(name, culture), defaultValue);
            }
            catch
            {
                try
                {
                    return GenericTypes.NZorEmpty(resources.GetString(name, NetResourceManager.DefualtCulture), defaultValue);
                }
                catch
                {
                    return defaultValue;
                }
            }
        }


		public string GetStringFormat(string name, params object[] args)
		{
			return GetStringFormat(null, name, args);
		}

		public string GetStringFormat(CultureInfo culture,string name, string args)
		{
			return GetStringFormat(null, name, new object[]{args});
		}

 
		public string GetStringFormat(CultureInfo culture, string name, params object[] args)
		{
	        if (RM == null)
			{
				return null;
			}
			string text1 = name;
			if(culture==null)
			{
				culture=Culture;// CultureInfo.CurrentCulture;
			}
            //else if(!isCultureSopperted(culture.Name))
            //{
            //    culture=NetRM.Culture;
            //}

			try
			{
                text1 = GenericTypes.NZorEmpty(resources.GetString(name, culture), name);
			}
			catch
			{
				try
				{
                    text1 = GenericTypes.NZorEmpty(resources.GetString(name, Culture), name);
				}
				catch
				{
					text1 = name;
				}
			}

			if ((args != null) && (args.Length > 0))
			{
				return string.Format(text1, args);
			}
			return text1;
		}

//		private static CultureInfo GetDefaultCulter()
//		{
//			// Creates and initializes the CultureInfo which uses the international sort.
//			return new CultureInfo( 0x0009, false );
//		}

		#endregion
   }
}
