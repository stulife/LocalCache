using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskPlugin
{
    /// <summary>
    /// 缓存刷新操作类
    /// </summary>
    /// <typeparam name="T">BaseCacheBuilder</typeparam>
    public class Operation<T> : OperationBase
        where T : CacheBase, new()
    {
        #region 刷新方法
        /// <summary>
        /// 全量刷新
        /// </summary>
        /// <returns>刷新影响记录条数</returns>
        protected override int Execute()
        {
            return new T().Build();
        }

        /// <summary>
        /// 增量刷新
        /// </summary>
        /// <returns>刷新影响记录条数</returns>
        protected override int ExecutePart(DateTime buildPartTime)
        {
            return new T().BuildPart(buildPartTime);
        }
        #endregion
    }
}
