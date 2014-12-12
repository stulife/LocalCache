using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;

namespace LocalCache
{
    public partial class TaskMonitor : System.Web.UI.Page
    {

        public string IpAddress
        {
            get
            {
                return (string)ViewState["ipAddress"];
            }
            set
            {
                ViewState["ipAddress"] = value;
            }
        }
        /// <summary>
        /// 页面加载
        /// </summary>
        /// <param name="sender">请求对象</param>
        /// <param name="e">事件</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //IpAddress = Request.ServerVariables["LOCAL_ADDR"];
                string hostname = Dns.GetHostName();//得到本机名  
                IPHostEntry localhost = Dns.GetHostEntry(hostname);
                List<string> ipList = new List<string>();
                foreach (var a in localhost.AddressList)
                {
                    ipList.Add(a.ToString());
                }
                IpAddress = string.Join(" | ", ipList);
            }
            catch
            {
            }
        }
    }
}