<%@ Page Language="C#"  AutoEventWireup="true" CodeBehind="TaskMonitor.aspx.cs" Inherits="LocalCache.TaskMonitor" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>缓存监控</title>
    <style type="text/css">
        *
        {
            margin: 0;
            padding: 0;
        }
        .header
        {
            background: rgb(69, 69, 69);
            height: 100px;
        }
        .header2
        {
            background: rgb(40, 40, 40);
            height: 30px;
        }
        #logo
        {
            position: relative;
            width: 1450px;
            height: 100px;
            color: #cfcfcf;
            font-size: 28px;
            margin-top: auto;
            margin-bottom: auto;
            margin-left: auto;
            margin-right: auto;
        }
        #logo .img
        {
            color: #0cf;
            font-size: 25px;
        }
        #logo .ip
        {
            color: #0cf;
            font-size: 14px;
        }
        #Container
        {
            width: 100%;
            margin-top: 20px;
            margin-bottom: auto;
            margin-left: auto;
            margin-right: auto;
        }
        .cli
        {
            width: 100%;
            border-top: 1px solid #CCCCCC;
            border-left: 1px solid #CCCCCC;
            overflow: hidden;
            margin-top: 10px;
            margin-bottom: 10px;
        }
        
        .cli li
        {
            border-bottom: 1px solid #CCCCCC;
            width: 100%;
            height: 30px;
            line-height: 30px;
            list-style: none;
            display: inline-block;
            background-color: #EFEFEF;
        }
        .cli span
        {
            width: 169px;
            height: 30px;
            background-color: #EFEFEF;
            border-right: 1px solid #CCCCCC;
            display: inline-block;
            text-align: right;
        }
        .cli .bright span
        {
            font-size: 18px;
            font-weight: 500;
            background-color: #006699;
            color: #FFFFFF;
            text-align: left;
        }
        .table
        {
            color: #333333;
            width: 100%;
            border-collapse: collapse;
        }
        .title
        {
            color: White;
            background-color: #5D7B9D;
            font-weight: bold;
        }
        .tr
        {
            background-color: #E5F1F4;
        }
        #GridView1 tr:hover
        {
            background-color: #CCCCCC;
            color: #333333;
        }
        .f
        {
            float: left;
        }
    </style>
    <script src="Scripts/jquery-1.4.1.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            OnloadFun();
        });
        function OnloadFun() {
            GetOperationStates();
            GetThreadState();
            setInterval(GetOperationStates, 1000);
            setInterval(GetThreadState, 1000);
        }
        function GetOperationStates() {
            var url = "Handler.ashx?Type=OperationStates&Number" + Math.random();
            $.ajax({
                url: url,
                type: "GET",
                dataType: 'json',
                success: function (data) {
                    var jsonDate = data;
                    var htmlTemp = new Array();
                    htmlTemp.push("<TABLE id=\"GridView1\" cellspacing=\"0\" cellpadding=\"4\" rules=\"all\" border=\"1\" class=\"table\"><tr class=\"title\"><td>key</td><td>中文描述</td><td>状态</td><td>全量刷新时间</td><td>增量刷新时间</td><td>刷新开始时间</td><td>刷新模式</td><td>刷新消耗时间</td><td>记录条数</td><td>操作</td></tr>");
                    if (jsonDate != null && jsonDate.length > 0) {
                        var i = 0;
                        var tr = "";
                        $.each(jsonDate, function (idx, item) {
                            i++;
                            if (i % 2 == 0) {
                                tr = "<tr class=\"tr\">";
                            }
                            else {
                                tr = "<tr>";
                            }
                            htmlTemp.push(tr + "<td title='" + item.OperationKey + "'>"
                            + item.OperationKey + "</td><td>"
                            + item.OperationKeyCN + "</td><td>"
                            + item.OperationState + "</td><td>"
                            + item.AllExpirationTime + "</td><td>"
                            + item.SlidingExpirationTime + "</td><td>"
                            + item.LatestCompletingTime + "</td><td>"
                            + item.OperationStyle + "</td><td>"
                            + item.ExecutionTime + "</td><td>"
                            + item.ResultCount + "</td><td><a href=\"#\" onclick=\"RefreshCache('"
                            + item.OperationKey + "','0');\">全量刷新</a>&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"#\" onclick=\"RefreshCache('"
                            + item.OperationKey + "','1');\">增量刷新</a></td></tr>");
                        });
                    }
                    htmlTemp.push("</TABLE>");
                    $("#mainUl").html(htmlTemp.join(""));
                }
            });
        }
        function RefreshCache(key, type) {
            var password = $("#passwordTxt").val();
            var url = "Handler.ashx?Type=RefreshCache&p=" + password + "&RefreshType=" + type + "&CacheKey=" + key + "&Number" + Math.random();
            $.ajax({
                url: url,
                type: "GET",
                dataType: 'text',
                success: function (data) {
                    alert(data);
                }
            });
        }
        function GetThreadState() {
            var url = "Handler.ashx?Type=ThreadState&Number" + Math.random();
            $.ajax({
                url: url,
                type: "GET",
                dataType: 'text',
                success: function (data) {
                    $("#threadState").html(data);
                }
            });
        }
        function LoadServiceCache(cacheType) {
            var password = $("#passwordTxt").val();
            var url = "Handler.ashx?Type=LoadServiceCache&p=" + password + "&CacheType=" + cacheType + "&Number" + Math.random();
            $.ajax({
                url: url,
                type: "GET",
                dataType: 'text',
                success: function (data) {
                    alert(data);
                }
            });
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div class="header">
        <h3 id="logo">
            <span class="img">local cache</span>|缓存监控<span class="ip"><%=IpAddress %></span></h3>
    </div>
    <div class="header2">
    </div>
    <div id="Container">
        <h3>
            线程状态:<span id="threadState"></span></h3>
        <h3>
            当前各操作状态</h3>
        <h3>
            <input name="passwordTxt" type="password" id="passwordTxt" style="width: 240px;" />
            <input id="loadSearchCache" type="button" value="预先数据" onclick="LoadServiceCache(0);" />
            <br />
        </h3>
        <div>
            <ul id="mainUl" class="cli">
            </ul>
        </div>
    </div>
    </form>
</body>
</html>
