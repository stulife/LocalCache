using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TaskPlugin
{
    /// <summary>
    /// 操作资源管理类
    /// </summary>
    public class OperationManager
    {
        /// <summary>
        /// 所有的缓存刷新单元队列
        /// </summary>
        private static readonly Dictionary<string, OperationBase> dicOperations = new Dictionary<string, OperationBase>();

        /// <summary>
        /// 静态构造函数
        /// </summary>
        public  static void AddOperationManager(List<OperationBase> taskList)
        {

            if (taskList != null && taskList.Count > 0)
            {
                foreach (var item in taskList)
                {
                    dicOperations.Add(item.CacheKey, item);
                }
            }
        }

        /// <summary>
        /// 获取缓存刷新单元队列
        /// </summary>
        /// <returns>缓存刷新单元队列</returns>
        public static Dictionary<string, OperationBase> GetOperationDic()
        {
            return dicOperations;
        }

        /// <summary>
        /// 获取操作状态
        /// </summary>
        /// <returns>操作状态json格式</returns>
        public static string GetOperationStatesJson()
        {
            StringBuilder jsonResult = new StringBuilder();
            jsonResult.Append("[");
            int temp = 0;
            string operationState = string.Empty;
            string operationStyle = string.Empty;
            foreach (string k in dicOperations.Keys)
            {
                temp++;
                jsonResult.Append("{");
                jsonResult.Append("\"OperationKey\":\"" + k + "\"");
                jsonResult.Append(",\"OperationKeyCN\":\"" + dicOperations[k].CacheName + "\"");
                switch (dicOperations[k].OperationState)
                {
                    case OperationState.Normal:
                        operationState = "正常状态";
                        break;

                    case OperationState.InsertTaskList:
                        operationState = "插入任务队列";
                        break;

                    case OperationState.AssignProcessor:
                        operationState = "分配线程执行刷新任务";
                        break;

                    case OperationState.Refreshing:
                        operationState = "刷新中";
                        break;

                    default:
                        operationState = string.Empty;
                        break;
                }

                jsonResult.Append(",\"OperationState\":\"" + operationState + "\"");
                switch (dicOperations[k].CacheRefreshInfo.OperationType)
                {
                    case OperationType.All:
                        operationStyle = "全量刷新";
                        break;

                    case OperationType.Part:
                        operationStyle = "增量刷新";
                        break;

                    default:
                        operationStyle = string.Empty;
                        break;
                }

                jsonResult.Append(",\"OperationStyle\":\"" + operationStyle + "\"");
                jsonResult.Append(",\"LatestCompletingTime\":\"" + dicOperations[k].CacheRefreshInfo.StartRefresh_Time.ToString() + "\"");
                jsonResult.Append(",\"AllExpirationTime\":\"" + dicOperations[k].CacheAllRefreshTime + "\"");
                jsonResult.Append(",\"SlidingExpirationTime\":\"" + dicOperations[k].CacheExpirationTime.TotalMinutes.ToString() + "\"");
                jsonResult.Append(",\"ExecutionTime\":\"" +
                    (dicOperations[k].CacheRefreshInfo.EndRefresh_Time -
                     dicOperations[k].CacheRefreshInfo.StartRefresh_Time).TotalMilliseconds
                     .ToString() + "\"");
                jsonResult.Append(",\"ResultCount\":\"" + dicOperations[k].CacheRefreshInfo.RecordCount.ToString() + "\"");
                jsonResult.Append("}");
                if (temp < dicOperations.Count)
                {
                    jsonResult.Append(",");
                }
            }

            jsonResult.Append("]");
            return jsonResult.ToString();
        }
    }
}
