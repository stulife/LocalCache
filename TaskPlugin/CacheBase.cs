using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskPlugin
{
    /// <summary>
    /// cache Builder基类
    /// </summary>
    public abstract class CacheBase
    {
        /// <summary>
        /// 全量刷新
        /// </summary>
        /// <returns>缓存信息数</returns>
        public abstract int Build();

        /// <summary>
        /// 增量刷新缓存
        /// </summary>
        /// <param name="buildPartTime">时间戳</param>
        /// <returns>缓存信息数</returns>
        public abstract int BuildPart(DateTime buildPartTime);
    }
}
