using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskPlugin;
using System.Threading;

namespace LocalCache.Cache
{
    public class CountryCache : CacheBase
    {
        private static List<string> countryList = new List<string>();

        /// <summary>
        /// 全量更新
        /// </summary>
        /// <returns></returns>
        public override int Build()
        {
            countryList.Add("中国");
            countryList.Add("美国");
            countryList.Add("加拿大");
            countryList.Add("日本");
            countryList.Add("英国");
            countryList.Add("朝鲜");
            countryList.Add("菲律宾");
            countryList.Add("越南");
            Thread.Sleep(30000);
            return countryList.Count;

        }
        /// <summary>
        /// 增量更新
        /// </summary>
        /// <param name="buildPartTime"></param>
        /// <returns></returns>
        public override int BuildPart(DateTime buildPartTime)
        {
            return this.Build();
        }

        public static List<string> CountryList()
        {
            return countryList;
        }
    }

    public class CityCache : CacheBase
    {

        private static List<string> cityList = new List<string>();

        /// <summary>
        /// 全量更新
        /// </summary>
        /// <returns></returns>
        public override int Build()
        {
            cityList.Add("上海");
            cityList.Add("北京");
            cityList.Add("广州");
            cityList.Add("苏州");
            cityList.Add("杭州");
            cityList.Add("嘉兴");
            cityList.Add("绍兴");
            cityList.Add("南京");
            Thread.Sleep(10000);
            return cityList.Count;
        }
        /// <summary>
        /// 增量更新
        /// </summary>
        /// <param name="buildPartTime"></param>
        /// <returns></returns>
        public override int BuildPart(DateTime buildPartTime)
        {
           return this.Build();
        }

        public static List<string> CityList()
        {
            return cityList;
        }
    }
}