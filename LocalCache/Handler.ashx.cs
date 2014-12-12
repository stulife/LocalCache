using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskPlugin;

namespace LocalCache
{
    /// <summary>
    /// Summary description for Handler
    /// </summary>
    public class Handler : IHttpHandler
    {
        /// <summary>
        /// 缓存线程运行状态服务
        /// </summary>
        /// <param name="context">http请求</param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string type = context.Request.QueryString["Type"];
            string resultStr = string.Empty;

            if (!string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case "OperationStates":
                        resultStr = OperationManager.GetOperationStatesJson();
                        break;

                    case "ThreadState":
                        resultStr = Watcher.GetState();
                        break;

                    case "LoadServiceCache":
                        if (this.PasswordIsTrue(context, ref resultStr))
                        {
                            string cacheType = context.Request.QueryString["CacheType"];
                            resultStr = Watcher.LoadServiceCache(cacheType);
                        }
                        break;

                    case "RefreshCache":
                        if (this.PasswordIsTrue(context, ref resultStr))
                        {
                            string cacheKey = context.Request.QueryString["CacheKey"];
                            string RefreshType = context.Request.QueryString["RefreshType"];
                            resultStr = Watcher.RefreshCache(cacheKey, RefreshType);

                            string userHostAddress = string.Empty;
                            if (context.Request != null)
                            {
                                userHostAddress = context.Request.UserHostAddress;
                            }

                            // Logger.Info("ManualRefreshCache", DateTime.Now.ToString("yyyyMMdd HH:mm:ss")
                            //     + "," + cacheKey + "," + RefreshType + "," + resultStr + "," + userHostAddress);
                        }
                        break;
                    default: break;
                }
            }

            context.Response.Write(resultStr);
        }

        /// <summary>
        /// 密码验证
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="errorMsg">错误消息</param>
        /// <returns>是否正确</returns>
        private bool PasswordIsTrue(HttpContext context, ref string errorMsg)
        {
            if (context.Request["p"] == null)
            {
                errorMsg = "没有输入密码，不能刷新缓存";
                return false;
            }
            else
            {
                string operKey = context.Request.QueryString["p"];
                if (!operKey.Equals("admin"))
                {
                    errorMsg = "密码输入错误，不能刷新缓存";
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// IHttpHandler接口
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}