using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskPlugin
{
    /// <summary>
    /// 公共资源
    /// 配置缓存中的一系列参数
    /// </summary>
    public class OperationResource
    {
        /// <summary>
        /// 缓存监控线程轮训缓存过期间隔时间，单位秒
        /// </summary>
        public const int CacheMonitorPollingMiddleTime = 30;

        /// <summary>
        /// 每个缓存存储刷新的历史信息个数
        /// </summary>
        public const int HistoryRefreshInfoCount = 20;

        /// <summary>
        /// 缓存刷新执行器线程最小数
        /// </summary>
        public const int CacheRefreshThread_Min = 2;

        /// <summary>
        /// 缓存刷新执行器线程最大数
        /// </summary>
        public const int CacheRefreshThread_Max = 6;

        #region 各个缓存全量刷新时间
        /// <summary>
        /// 默认缓存全量刷新时间(绝对时间，每天的凌晨00:00分)
        /// </summary>
        public const string CacheAllRefreshTime_Default = "00:00";

        /// <summary>
        /// 5级缓存全量刷新时间
        /// </summary>
        public const string CacheAllRefreshTime_0005 = "00:05";

        /// <summary>
        /// 4级缓存全量刷新时间
        /// </summary>
        public const string CacheAllRefreshTime_0010 = "00:10";

        /// <summary>
        /// 3级缓存全量刷新时间
        /// </summary>
        public const string CacheAllRefreshTime_0015 = "00:15";

        /// <summary>
        /// 2级缓存全量刷新时间
        /// </summary>
        public const string CacheAllRefreshTime_0020 = "00:20";

        /// <summary>
        /// 1级缓存全量刷新时间
        /// </summary>
        public const string CacheAllRefreshTime_0025 = "00:25";
        #endregion

        #region 各个缓存增量刷新时间,即超时时间，单位秒
        /// <summary>
        /// 默认的缓存刷新时间-15分钟
        /// 假如单元没有设置时间，则设置为默认值
        /// </summary>
        public const int CacheExpireTime_Default = 60 * 15;

        /// <summary>
        /// 缓存过期时间-15分钟
        /// </summary>
        public const int CacheExpireTime_15Minutes = 60 * 15;

        /// <summary>
        /// 缓存过期时间-30分钟
        /// </summary>
        public const int CacheExpireTime_30Minutes = 60 * 30;

        /// <summary>
        /// 缓存过期时间-1个小时
        /// </summary>
        public const int CacheExpireTime_1Hours = 60 * 60 * 1;

        /// <summary>
        /// 缓存过期时间-2个小时
        /// </summary>
        public const int CacheExpireTime_2Hours = 60 * 60 * 2;

        /// <summary>
        /// 缓存过期时间-4个小时
        /// </summary>
        public const int CacheExpireTime_4Hours = 60 * 60 * 4;

        /// <summary>
        /// 缓存过期时间-8个小时
        /// </summary>
        public const int CacheExpireTime_8Hours = 60 * 60 * 8;

        /// <summary>
        /// 缓存过期时间-9个小时
        /// </summary>
        public const int CacheExpireTime_9Hours = 60 * 60 * 9;

        /// <summary>
        /// 缓存过期时间-10个小时
        /// </summary>
        public const int CacheExpireTime_10Hours = 60 * 60 * 10;
        #endregion
    }
}
