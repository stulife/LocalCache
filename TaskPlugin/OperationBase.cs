using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace TaskPlugin
{
    /// <summary>
    /// 缓存操作基类
    /// </summary>
    public abstract class OperationBase
    {
        #region 系统设置参数
        /// <summary>
        /// 缓存过期时间，单位秒
        /// </summary>
        private TimeSpan _cacheExpirationTime = new TimeSpan(0, 0, 3600);

        /// <summary>
        /// 缓存过期时间，单位秒
        /// </summary>
        public TimeSpan CacheExpirationTime
        {
            get
            {
                return this._cacheExpirationTime;
            }
        }

        /// <summary>
        /// 缓存全量刷新时间点
        /// </summary>
        private string _cacheAllRefreshTime = "00:00";

        /// <summary>
        /// 缓存全量刷新时间点
        /// 格式为HHmm
        /// </summary>
        public string CacheAllRefreshTime
        {
            get
            {
                return this._cacheAllRefreshTime;
            }
        }

        /// <summary>
        /// 缓存的主键
        /// </summary>
        private string _cacheKey = string.Empty;

        /// <summary>
        /// 缓存的主键
        /// </summary>
        public string CacheKey
        {
            get
            {
                return this._cacheKey;
            }
        }

        /// <summary>
        /// 缓存的中文描述
        /// </summary>
        private string _cacheName = string.Empty;

        /// <summary>
        /// 缓存的中文描述
        /// </summary>
        public string CacheName
        {
            get
            {
                return this._cacheName;
            }
        }

        /// <summary>
        /// 缓存级别
        /// </summary>
        private CacheLevel _cacheLevelSetting = CacheLevel.Level1;

        /// <summary>
        /// 缓存级别
        /// </summary>
        public CacheLevel CacheLevelSetting
        {
            get
            {
                return this._cacheLevelSetting;
            }
        }

        /// <summary>
        /// 配置缓存刷新参数
        /// </summary>
        /// <param name="cacheExpirationTime">缓存过期时间，单位秒</param>
        /// <param name="cacheAllRefreshTime">缓存全量刷新时间点</param>
        /// <param name="cacheKey">缓存的主键</param>
        /// <param name="cacheName">缓存的中文描述</param>
        /// <param name="cacheLevelSetting">缓存级别</param>
        public void Setting(int cacheExpirationTime,
            string cacheAllRefreshTime,
            string cacheKey,
            string cacheName,
            CacheLevel cacheLevelSetting)
        {
            this._cacheAllRefreshTime = cacheAllRefreshTime;
            if (cacheExpirationTime <= OperationResource.CacheExpireTime_Default)
            {
                cacheExpirationTime = OperationResource.CacheExpireTime_Default;
            }

            this._cacheExpirationTime = new TimeSpan(0, 0, cacheExpirationTime);
            this._cacheKey = cacheKey;
            this._cacheName = cacheName;
            this._cacheLevelSetting = cacheLevelSetting;
        }
        #endregion

        #region 运行过程中参数
        /// <summary>
        /// 缓存刷新过程状态
        /// 系统根据配置参数自动计算
        /// </summary>
        public OperationState OperationState { get; set; }

        /// <summary>
        /// 本次缓存刷新类型
        /// 是全量还是增量
        /// 系统根据配置参数自动计算
        /// </summary>
        public OperationType OperationType { get; set; }

        /// <summary>
        /// 缓存刷新的运行信息
        /// 初始化时为空，其他场景下为本次的、或上次刷新的状态信息
        /// </summary>
        public CacheRefreshInfo CacheRefreshInfo { get; set; }

        /// <summary>
        /// 上次全量刷新的时间戳
        /// </summary>
        public DateTime LastRefreshAllTime { get; set; }
        #endregion

        #region 刷新方法
        /// <summary>
        /// 全量刷新
        /// </summary>
        protected abstract int Execute();

        /// <summary>
        /// 增量刷新
        /// </summary>
        protected abstract int ExecutePart(DateTime buildPartTime);

        /// <summary>
        /// 刷新缓存
        /// </summary>
        /// <param name="operationTypeObj">判断是全量刷新，还是增量刷新</param>
        public void Refresh(object operationTypeObj)
        {
            // 定义是否全量刷新还是增量刷新
            OperationType operationType = operationTypeObj == null ? OperationType.Part : (OperationType)operationTypeObj;

            // 记录日志
            StringBuilder sb = new StringBuilder();
            DateTime startTime = DateTime.Now;
            sb.AppendLine(this.OperationType.ToString() + "," + this.CacheKey + ":" + this.CacheName);
            sb.AppendLine("start time:" + startTime.ToString("yyyyMMdd HHmmss.fff"));

            if (this.CacheRefreshInfo == null)
            {
                this.CacheRefreshInfo = new CacheRefreshInfo();
            }

            DateTime buildPartTime = DateTime.MinValue;

            try
            {
                // 获取上次开始刷新的时间
                buildPartTime = this.CacheRefreshInfo.StartRefresh_Time;
                this.OperationState = OperationState.Refreshing;

                #region 设置开始参数
                this.CacheRefreshInfo.OperationType = operationType;
                this.CacheRefreshInfo.StartRefresh_Time = DateTime.Now;
                if (operationType == OperationType.All)
                {
                    // 设置全量刷新的开始时间，为增量刷新做标记
                    this.LastRefreshAllTime = this.CacheRefreshInfo.StartRefresh_Time;
                }

                #endregion

                // 执行全量缓存刷新
                if (operationType == OperationType.All)
                {
                    this.CacheRefreshInfo.RecordCount = this.Execute();
                }
                else
                {
                    sb.AppendLine("上次刷新的时间戳:" + buildPartTime.ToString("yyyy-MM-dd HH:mm:ss"));

                    if (buildPartTime == DateTime.MinValue)
                    {
                        this.CacheRefreshInfo.RecordCount = this.ExecutePart(buildPartTime);
                    }
                    else
                    {
                        this.CacheRefreshInfo.RecordCount = this.ExecutePart(buildPartTime - _cacheExpirationTime);
                    }
                }

                sb.AppendLine("RecordCount:" + this.CacheRefreshInfo.RecordCount);
            }
            catch (Exception ee)
            {
                this.CacheRefreshInfo.StartRefresh_Time = buildPartTime;
                this.CacheRefreshInfo.TipInfo = ee.Message;
                sb.AppendLine(ee.Message);
                //Logger.Error("Refresh_" + operationType + "_Error_" + this.OperationType.ToString() + "," + this.CacheKey + ":" + this.CacheName, ee,ErrorCode.ApplicaitonException);
            }
            finally
            {
                this.OperationState = OperationState.Normal;
                this.CacheRefreshInfo.EndRefresh_Time = DateTime.Now;
                sb.AppendLine(this.CacheRefreshInfo.ToString());
                DateTime endTime = DateTime.Now;
                sb.AppendLine("结束时间;" + endTime.ToString("yyyyMMdd HHmmss.fff"));
                sb.AppendLine("耗费时间:" + (endTime - startTime).TotalMilliseconds);

                if ((endTime - startTime).TotalMilliseconds > 30000)
                {
                   // Logger.Info("Refresh_" + operationType + "_Expire(" + (endTime - startTime).TotalMilliseconds + ")", sb.ToString());
                }
                else
                {
                    //Logger.Info("Refresh_" + operationType + "_Normal(" + (endTime - startTime).TotalMilliseconds + ")", sb.ToString());
                }

                sb = null;

                // 任务执行完毕后,从任务队列中移除
                Watcher.CacheRefreshTaskEnd(this.CacheKey);
            }
        }
        #endregion
    }
}
