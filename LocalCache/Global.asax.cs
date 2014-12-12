using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using TaskPlugin;
using LocalCache.Cache;

namespace LocalCache
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            // 开启任务

            List<OperationBase> taskList=new List<OperationBase>();
            //国家信息
            OperationBase operationBase = new Operation<CountryCache>();
            operationBase.Setting(
                OperationResource.CacheExpireTime_30Minutes,
                OperationResource.CacheAllRefreshTime_0005,
                "Country", "01-国家",
                CacheLevel.Level3);
            taskList.Add(operationBase);

            // 城市信息
            operationBase = new Operation<CityCache>();
            operationBase.Setting(
                OperationResource.CacheExpireTime_30Minutes,
                OperationResource.CacheAllRefreshTime_0005,
                "City", "02-城市",
                CacheLevel.Level3);
            taskList.Add(operationBase);

            OperationManager.AddOperationManager(taskList);
            Watcher.Start();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}