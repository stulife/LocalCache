using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskPlugin
{
    /// <summary>
    /// 缓存刷新类型
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// 全量
        /// </summary>
        All = 0,

        /// <summary>
        /// 增量
        /// </summary>
        Part = 1,
    }

    /// <summary>
    /// 缓存刷新过程状态
    /// </summary>
    public enum OperationState
    {
        /// <summary>
        /// 正常状态
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 插入任务队列
        /// </summary>
        InsertTaskList = 1,

        /// <summary>
        /// 分配线程执行刷新任务
        /// </summary>
        AssignProcessor = 2,

        /// <summary>
        /// 刷新中
        /// </summary>
        Refreshing = 3,
    }

    /// <summary>
    /// 缓存刷新的运行信息
    /// </summary>
    public class CacheRefreshInfo
    {
        /// <summary>
        /// 插入任务队列
        /// </summary>
        public DateTime InsertTaskList_Time = DateTime.MinValue;

        /// <summary>
        /// 分配线程执行刷新任务
        /// </summary>
        public DateTime AssignProcessor_Time = DateTime.MinValue;

        /// <summary>
        /// 开始刷新时间
        /// </summary>
        public DateTime StartRefresh_Time = DateTime.MinValue;

        /// <summary>
        /// 结束刷新的时间
        /// </summary>
        public DateTime EndRefresh_Time = DateTime.MinValue;

        /// <summary>
        /// 刷新类型
        /// </summary>
        public OperationType OperationType { get; set; }

        /// <summary>
        /// 影响的记录条数
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// 附加信息
        /// </summary>
        public string TipInfo { get; set; }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns>字符串</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("插入任务队列时间:" + this.InsertTaskList_Time.ToString("MMdd-HH:mm:ss.fff"));
            sb.AppendLine("分配线程执行刷新任务:" + this.AssignProcessor_Time.ToString("MMdd-HH:mm:ss.fff"));
            sb.AppendLine("开始刷新时间:" + this.StartRefresh_Time.ToString("MMdd-HH:mm:ss.fff"));
            sb.AppendLine("结束刷新的时间:" + this.EndRefresh_Time.ToString("MMdd-HH:mm:ss.fff"));
            sb.AppendLine("刷新类型:" + this.OperationType.ToString());
            sb.AppendLine("RecordCount:" + this.RecordCount.ToString());
            sb.AppendLine("附加信息:" + this.TipInfo);
            return sb.ToString();
        }
    }

    /// <summary>
    /// 缓存级别,从高到低进行刷新
    /// </summary>
    public enum CacheLevel
    {
        /// <summary>
        /// 缓存级别1
        /// </summary>
        Level1 = 1,

        /// <summary>
        /// 缓存级别2
        /// </summary>
        Level2 = 2,

        /// <summary>
        /// 缓存级别3
        /// </summary>
        Level3 = 3,

        /// <summary>
        /// 缓存级别4
        /// </summary>
        Level4 = 4,

        /// <summary>
        /// 缓存级别5
        /// </summary>
        Level5 = 5,
    }
}
