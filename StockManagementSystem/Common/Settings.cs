﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Connection
/// </summary>
public static class Settings
{
    public static string SkyTradeConn { get; set; }
    public static string StockMarketConn { get; set; }

    public static void Initialize()
    {
        string machineName = System.Environment.MachineName;

        if (machineName == "DIDIERDROGBA")
        {
            SkyTradeConn = System.Configuration.ConfigurationManager.ConnectionStrings["AndrewSkyTrade"].ToString();
            StockMarketConn = System.Configuration.ConfigurationManager.ConnectionStrings["AndrewStockMarket"].ToString();
        }
        else if (machineName == "MAYBE-PC" || machineName == "CISSY-PC")
        {
            SkyTradeConn = System.Configuration.ConfigurationManager.ConnectionStrings["XiSkyTrade"].ToString();
            StockMarketConn = System.Configuration.ConfigurationManager.ConnectionStrings["XiStockMarket"].ToString();
        }
        else
        {
            // Note that these connections will only work while on campus (or remotted in)
            SkyTradeConn = System.Configuration.ConfigurationManager.ConnectionStrings["SkyTrade"].ToString();
            StockMarketConn = System.Configuration.ConfigurationManager.ConnectionStrings["StockMarket"].ToString();
        }
    }
}