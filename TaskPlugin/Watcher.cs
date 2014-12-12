using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace TaskPlugin
{
    /// <summary>
    /// 缓存监控对象
    /// 包括2部分内容，一部分为监控当前系统中的缓存，假如缓存过期，则插入一个任务进行缓存刷新，包括增量和全量的缓存
    /// 另外一部分内容为任务执行调度器，专门处理任务
    /// </summary>
    public class Watcher
    {
        #region 私有属性
        /// <summary>
        /// 刷新缓存任务队列
        /// 只有任务执行完毕后，才从队列移除
        /// key值为OperationManager的dicOperations集合的key
        /// </summary>
        private static ConcurrentDictionary<string, OperationBase> refreshTaskList = new ConcurrentDictionary<string, OperationBase>();

        /// <summary>
        /// 缓存监控器
        /// 监控系统缓存的更新，假如缓存过期，则插入一个任务进行缓存刷新，包括增量和全量的缓存
        /// </summary>
        private static Thread cacheMonitorThread;

        /// <summary>
        /// 缓存刷新任务执行线程
        /// </summary>
        private static Thread cacheRefreshTaskProcessorThread;

        /// <summary>
        /// 刷新缓存Thread集合
        /// 子线程集合，缓存刷新任务执行线程
        /// key值为OperationManager的dicOperations集合的key
        /// </summary>
        private static ConcurrentDictionary<string, Thread> refreshSubThread = new ConcurrentDictionary<string, Thread>();
        #endregion

        #region 启动
        /// <summary>
        /// Watcher启动
        /// 开始初始化系统，并启动缓存监控线程和缓存刷新任务执行线程
        /// </summary>
        public static void Start()
        {
            // 日志记录器
            StringBuilder sb = new StringBuilder();
            DateTime startTime = DateTime.Now;
            try
            {
                sb.AppendLine("Watcher start:" + startTime.ToString("yyyyMMdd HHmmss.fff"));

                // 判断基层缓存刷新单元是否已经配置好，假如没有，抛出异常
                if (OperationManager.GetOperationDic() == null
                    ||
                    OperationManager.GetOperationDic().Count == 0)
                {
                    string errorString = "缓存刷新单元对象集合为空，系统启动失效，请确认是否配置";
                    sb.AppendLine(errorString);
                    throw new Exception(errorString);
                }
                else
                {
                    sb.AppendLine("一共加载" + OperationManager.GetOperationDic().Count + "缓存刷新单元对象");

                    #region 检查各个处理单元，并给各个单元赋值
                    // 定义临时变量
                    OperationBase operationBase;
                    foreach (string operationBaseKey in OperationManager.GetOperationDic().Keys)
                    {
                        operationBase = OperationManager.GetOperationDic()[operationBaseKey];
                        if (operationBase == null)
                        {
                            sb.AppendLine("单元:" + operationBaseKey + "，对应处理单元为null，请检查");
                            continue;
                        }

                        #region 系统启动时全量刷新
                        if (operationBase.CacheRefreshInfo == null)
                        {
                            operationBase.CacheRefreshInfo = new CacheRefreshInfo();
                        }

                        operationBase.OperationType = OperationType.All;
                        operationBase.OperationState = OperationState.InsertTaskList;
                        operationBase.CacheRefreshInfo.InsertTaskList_Time = DateTime.Now;
                        operationBase.CacheRefreshInfo.OperationType = OperationType.All;

                        // 插入队列
                        refreshTaskList[operationBaseKey] = operationBase;
                        #endregion
                    }
                    #endregion
                }

                cacheRefreshTaskProcessorThread = new Thread(new ThreadStart(CacheRefreshTaskProcessor));
                cacheRefreshTaskProcessorThread.Start();
                sb.AppendLine("缓存刷新任务监控线程启动完毕");

                cacheMonitorThread = new Thread(new ThreadStart(CacheMonitor));
                cacheMonitorThread.Start();
                sb.AppendLine("缓存监控线程启动完毕");

                sb.AppendLine("Watcher成功启动");
            }
            catch (Exception ee)
            {
                sb.AppendLine(ee.Message);
              //  Logger.Error("Watcher启动失败", ee, ErrorCode.ApplicaitonException);
                throw ee;
            }
            finally
            {
                DateTime endTime = DateTime.Now;
                sb.AppendLine("Watcher启动完毕时间:" + endTime.ToString("yyyyMMdd HHmmss.fff"));
                sb.AppendLine("消耗时间:" + (endTime - startTime).TotalMilliseconds);
               // Logger.Info("Watcher启动", sb.ToString());
                sb = null;
            }
        }
        #endregion

        #region 缓存刷新任务执行线程
        /// <summary>
        /// 信号
        /// </summary>
        static ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        /// <summary>
        /// 缓存刷新任务执行线程
        /// 从任务队列里面获取任务并执行
        /// </summary>
        public static void CacheRefreshTaskProcessor()
        {
            // 该线程长期运行
            while (true)
            {
                manualResetEvent.Reset();

                try
                {
                   // Logger.Info("CacheRefreshTaskProcessor", DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));

            

                    #region 更新的缓存
                    // 定义一个临时变量
                    IEnumerable<OperationBase> operationBaseList =
                                from n in
                                    refreshTaskList.Values
                                where
                                n.OperationState == OperationState.InsertTaskList
                                select n;

                    if (operationBaseList.Count() > 0)
                    {
                        // 是否有多余的线程
                        bool isHasThread = true;

                        // 定义一个更新顺序
                        OperationType[] operationTypeList = new OperationType[2]{
                OperationType.All,OperationType.Part};
                        for (int operationIndex = 0; operationIndex < operationTypeList.Length; operationIndex++)
                        {
                            #region 执行某类更新
                            for (int index = 5; index >= 1; index--)
                            {
                                operationBaseList =
                                    from n in
                                        refreshTaskList.Values
                                    where
                                    n.OperationState == OperationState.InsertTaskList
                                    && n.OperationType == operationTypeList[operationIndex]
                                    && n.CacheLevelSetting.GetHashCode() == index
                                    select n;

                                foreach (OperationBase operationBase in operationBaseList)
                                {
                                    if (refreshSubThread.Count < OperationResource.CacheRefreshThread_Max)
                                    {
                                        isHasThread = true;

                                        #region 有多余的线程则执行
                                        operationBase.OperationState = OperationState.AssignProcessor;
                                        Thread thread = null;
                                        operationBase.CacheRefreshInfo.AssignProcessor_Time = DateTime.Now;
                                        thread = new Thread(new ParameterizedThreadStart(operationBase.Refresh));
                                        refreshSubThread[operationBase.CacheKey] = thread;
                                        thread.Name = operationBase.CacheKey;
                                        thread.Start(operationTypeList[operationIndex]);
                                        #endregion
                                    }
                                    else
                                    {
                                        isHasThread = false;
                                        break;
                                    }
                                }

                                if (!isHasThread)
                                {
                                    // 假如没有多余的线程，则直接跳出循环
                                    break;
                                }
                            }

                            if (!isHasThread)
                            {
                                // 假如没有多余的线程，则直接跳出循环
                                break;
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                catch (Exception ee)
                {
                   // Logger.Error("CacheRefreshTaskProcessor_Error", ee, ErrorCode.ApplicaitonException);
                }

                manualResetEvent.WaitOne();
            }
        }

        /// <summary>
        /// 计划任务执行结束后,从相关队列里移除对应的对象
        /// </summary>
        /// <param name="cacheKey">缓存key</param>
        public static void CacheRefreshTaskEnd(string cacheKey)
        {
            try
            {
                OperationBase operationBase = null;
                refreshTaskList.TryRemove(cacheKey, out operationBase);
                Thread thread = null;
                refreshSubThread.TryRemove(cacheKey, out thread);
            }
            catch (Exception ee)
            {
               
                //Logger.Error("CacheRefreshTaskProcessor_End_Error" + cacheKey, ee, ErrorCode.ApplicaitonException);
            }

            manualResetEvent.Set();
        }
        #endregion

        #region 缓存监控
        /// <summary>
        /// 缓存过期监控
        /// 采用轮询的方法查看缓存
        /// </summary>
        public static void CacheMonitor()
        {
            // 该线程长期运行
            while (true)
            {
                try
                {
                    #region 遍历缓存刷新单元
                    foreach (string operationBaseKey in OperationManager.GetOperationDic().Keys)
                    {
                        try
                        {
                            // 定义临时变量，存储单个缓存刷新单元
                            OperationBase operationBase = OperationManager.GetOperationDic()[operationBaseKey];

                            #region 判断当前任务队列里面是否有该单元的刷新任务，假如有，且是全量刷新的话，忽略本次该单元的刷新
                            if (refreshTaskList == null)
                            {
                                refreshTaskList = new ConcurrentDictionary<string, OperationBase>();
                            }

                            if (refreshTaskList.ContainsKey(operationBaseKey)
                                &&
                               refreshTaskList[operationBaseKey] != null
                                &&
                               refreshTaskList[operationBaseKey].OperationType == OperationType.All)
                            {
                                continue;
                            }
                            #endregion

                            // 单元是否需要刷新
                            bool isMustRefresh = false;

                            // 设置为局部刷新
                            OperationType operationType = OperationType.Part;

                            #region 判断是否需要全量刷新
                            // 先判断是否在此时刻前全量是否刷新过
                            if (operationBase.LastRefreshAllTime == DateTime.MinValue)
                            {
                                // 表示该单元从来没有全量刷新过，本次全量刷新，一般系统第一次启动时，进行该步骤
                                operationType = OperationType.All;
                                isMustRefresh = true;
                            }
                            else
                            {
                                // 定义临时变量，计算全量刷新的时间
                                // 计算本天全量刷新的时间点
                                DateTime cacheAllRefreshTime =Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " " + operationBase.CacheAllRefreshTime);

                                // 判断本次是否需要全量刷新
                                // 假如上次刷新时间，小于本次应该刷新的时间，且当前时间大于应该刷新的时间，则表明需要全量刷新
                                if (operationBase.LastRefreshAllTime < cacheAllRefreshTime
                                    &&
                                    DateTime.Now > cacheAllRefreshTime)
                                {
                                    operationType = OperationType.All;
                                    isMustRefresh = true;
                                }
                            }
                            #endregion

                            if (!isMustRefresh)
                            {
                                #region 判断是否必要局部刷新
                                // 判断任务队列是否有对应的任务，假如有，则不要局部刷新
                                if (refreshTaskList.ContainsKey(operationBaseKey)
                                &&
                               refreshTaskList[operationBaseKey] != null)
                                {
                                    continue;
                                }
                                else
                                {
                                    // 任务队列没有对应的任务
                                    // 判断上次运行参数
                                    if (
                                        operationBase.CacheRefreshInfo.StartRefresh_Time.AddMinutes(operationBase.CacheExpirationTime.TotalMinutes) < DateTime.Now
                                        )
                                    {
                                        isMustRefresh = true;
                                        operationType = OperationType.Part;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                #endregion
                            }

                            if (!isMustRefresh)
                            {
                                continue;
                            }

                            #region 全量更新缓存时，假如存在增量更新线程，则直接将该线程终止
                            if (operationType == OperationType.All
                                &&
                                refreshSubThread.ContainsKey(operationBaseKey))
                            {
                                // 如果为全量刷新，且线程队列里面有对应的线程时，判断该线程的状态，假如该线程为运行状态时，取消该线程
                                Thread thread = refreshSubThread[operationBaseKey];
                                if (thread != null
                                    &&
                                   thread.ThreadState != ThreadState.Stopped
                                    &&
                                    thread.ThreadState != ThreadState.StopRequested
                                    &&
                                    thread.ThreadState != ThreadState.Aborted
                                    &&
                                    thread.ThreadState != ThreadState.AbortRequested)
                                {
                                    try
                                    {
                                        // 终止线程
                                        thread.Abort();
                                    }
                                    catch (Exception ee)
                                    {
                                        //Logger.Error("CacheMonitor_ThreadAbort:" + operationBaseKey, ee, ErrorCode.ApplicaitonException);
                                    }
                                }
                            }
                            #endregion

                            #region 设置状态
                            operationBase.OperationType = operationType;
                            operationBase.OperationState = OperationState.InsertTaskList;

                            // 初始化运行状态
                            if (operationBase.CacheRefreshInfo == null)
                            {
                                operationBase.CacheRefreshInfo = new CacheRefreshInfo();
                            }

                            operationBase.CacheRefreshInfo.InsertTaskList_Time = DateTime.Now;
                            operationBase.CacheRefreshInfo.OperationType = operationType;
                            #endregion

                            // 插入队列
                            refreshTaskList[operationBaseKey] = operationBase;
                            manualResetEvent.Set();
                        }
                        catch (Exception ee)
                        {
                           // Logger.Error("CacheMonitor_Error_Key:" + operationBaseKey, ee, ErrorCode.ApplicaitonException);
                        }
                    }
                    #endregion
                }
                catch (Exception ee)
                {
                    //Logger.Error("CacheMonitor_Error", ee, ErrorCode.ApplicaitonException);
                }

                #region 监控线程睡眠
                Thread.Sleep(new TimeSpan(0, 0, OperationResource.CacheMonitorPollingMiddleTime));
                #endregion
            }
        }
        #endregion

        #region 获取任务状态
        /// <summary>
        /// 服务数据加载线程
        /// </summary>
        private static Thread loadServiceThread = null;

        /// <summary>
        /// 服务数据加载obj
        /// </summary>
        private static object loadServiceObj = new object();

        /// <summary>
        /// 预先加载服务数据
        /// </summary>
        /// <param name="cacheType">服务数据类型</param>
        /// <returns>加载状态</returns>
        public static string LoadServiceCache(string cacheType)
        {
            if (loadServiceThread != null
                        && 
                        (
                        loadServiceThread.ThreadState == ThreadState.Running 
                        || 
                        loadServiceThread.ThreadState == ThreadState.WaitSleepJoin))
            {
                return "服务数据加载线程正在运行";
            }

            lock (loadServiceObj)
            {
                switch (cacheType)
                {
                 
                    // 预先加载数据
                    case "0":
                        return "没有实现";

                    default:
                        return "错误的缓存类型";
                }
            }
        }

        /// <summary>
        /// 获取任务状态
        /// </summary>
        /// <returns>任务状态字符串</returns>
        public static string GetState()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("缓存状态监控:" + (cacheMonitorThread == null ? "Null" : cacheMonitorThread.ThreadState.ToString()));
            sb.AppendLine("缓存刷新任务监控:" + (cacheRefreshTaskProcessorThread == null ? "Null" : cacheRefreshTaskProcessorThread.ThreadState.ToString()));
            sb.AppendLine("服务数据加载线程:" + (loadServiceThread == null ? "Null" : loadServiceThread.ThreadState.ToString()));

            sb.AppendLine("缓存刷新任务:");
            foreach (string threadKey in refreshSubThread.Keys)
            {
                Thread thread = refreshSubThread[threadKey];
                sb.AppendLine(threadKey + "-" + thread == null ? "Null" : thread.ThreadState.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// 根据key刷新缓存
        /// </summary>
        /// <param name="cacheKey">key</param>
        /// <param name="refreshType">0-全量刷新，1-增量刷新</param>
        /// <returns>刷新缓存</returns>
        public static string RefreshCache(string cacheKey,string refreshType)
        {
            foreach (string operationBaseKey in OperationManager.GetOperationDic().Keys)
            {
                if (operationBaseKey.Equals(cacheKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (refreshTaskList.ContainsKey(operationBaseKey))
                    {
                        return "该缓存在等待刷新中，不能指定操作";
                    }

                    OperationBase operationBase = OperationManager.GetOperationDic()[operationBaseKey];
                    if (operationBase.CacheRefreshInfo == null)
                    {
                        operationBase.CacheRefreshInfo = new CacheRefreshInfo();
                    }

                    if (refreshType.Equals("0"))
                    {
                        operationBase.OperationType = OperationType.All;
                    }
                    else
                    {
                        operationBase.OperationType = OperationType.Part;
                    }

                    operationBase.OperationState = OperationState.InsertTaskList;
                    operationBase.CacheRefreshInfo.InsertTaskList_Time = DateTime.Now;
                    operationBase.CacheRefreshInfo.OperationType = OperationType.All;

                    // 插入队列
                    refreshTaskList[operationBaseKey] = operationBase;
                    manualResetEvent.Set();

                    return "全量刷新操作已经加入任务队列";
                }
            }

            return "指定的缓存不存在";
        }
        #endregion
    }
}
