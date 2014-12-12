LocalCache
==========

多线程动态加载更新本地缓存
    实现 CacheBase 基类

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
