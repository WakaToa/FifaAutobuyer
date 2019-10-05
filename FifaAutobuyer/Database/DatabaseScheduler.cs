using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Managers;
using FifaAutobuyer.Fifa.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Database.Web;
using FluentScheduler;

namespace FifaAutobuyer.Database
{
    public class DatabaseScheduler
    {
        public static void SaveBotStatistics()
        {
            try
            {
                var totalCoins = 0;
                var accounts = FUTAccountsDatabase.GetFUTAccounts();
                var coins = FUTLogsDatabase.GetFUTCoins();

                foreach (var acc in accounts)
                {
                    var coinsFromAcc = coins.Where(x => x.EMail.ToLower() == acc.EMail.ToLower()).FirstOrDefault();
                    if (coinsFromAcc != null)
                    {
                        totalCoins += coinsFromAcc.Coins;
                    }
                }

                var allTPItems = BotManager.GetTradepileItems();
                var tpValue = (int)(allTPItems.Sum(x => x.buyNowPrice) * 0.95);

                var totalBuys = FUTLogsDatabase.GetFUTBuysCount();
                var totalSells = FUTLogsDatabase.GetFUTSellsCount();

                var log = new FUTBotStatistics();
                log.Buys = totalBuys;
                log.Sells = totalSells;
                log.TotalCoins = totalCoins;
                log.TotalTradepileItems = allTPItems.Count;
                log.TotalTradepileValue = tpValue;

                FUTLogsDatabase.InsertFUTBotStatistics(log);
            }
            catch
            {

            }
        }

        public static void DeleteOldLogs()
        {
            #region FUTBotLogs
            try
            {
                var twoDays = 172800000;
                var now = Helper.CreateTimestamp();
                using (var context = new FUTLogsDatabase())
                {
                    var oldLogs = context.FUTBotLogs.OrderByDescending(x => x.ID).Where(x => (now - x.Timestamp) >= twoDays).ToList();
                    if (oldLogs.Any())
                    {
                        context.FUTBotLogs.RemoveRange(oldLogs);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
            #endregion

            #region FUTBotStatistics
            try
            {
                var twoDays = 172800000;
                var now = Helper.CreateTimestamp();
                using (var context = new FUTLogsDatabase())
                {
                    var oldLogs = context.FUTBotStatistics.OrderByDescending(x => x.ID).Where(x => (now - x.Timestamp) >= twoDays).ToList();
                    if (oldLogs.Any())
                    {
                        context.FUTBotStatistics.RemoveRange(oldLogs);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
            #endregion

            #region FUTBuys
            try
            {
                var twoDays = 345600000;
                var now = Helper.CreateTimestamp();
                using (var context = new FUTLogsDatabase())
                {
                    var oldLogs = context.FUTBuys.OrderByDescending(x => x.ID).Where(x => (now - x.Timestamp) >= twoDays).ToList();
                    if (oldLogs.Any())
                    {
                        context.FUTBuys.RemoveRange(oldLogs);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
            #endregion

            #region FUTSells
            try
            {
                var twoDays = 172800000;
                var now = Helper.CreateTimestamp();
                using (var context = new FUTLogsDatabase())
                {
                    var oldLogs = context.FUTSells.OrderByDescending(x => x.ID).Where(x => (now - x.Timestamp) >= twoDays).ToList();
                    if (oldLogs.Any())
                    {
                        context.FUTSells.RemoveRange(oldLogs);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
            #endregion

            #region FUTExceptionLogs
            try
            {
                var twoDays = 172800000;
                var now = Helper.CreateTimestamp();
                using (var context = new FUTLogsDatabase())
                {
                    var oldLogs = context.FUTExceptionLogs.OrderByDescending(x => x.ID).Where(x => (now - x.Timestamp) >= twoDays).ToList();
                    if (oldLogs.Any())
                    {
                        context.FUTExceptionLogs.RemoveRange(oldLogs);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
            #endregion
        }

        public static void UpdateMaxConnections()
        {
            using (var context = new FUTLogsDatabase())
            {
                //10000
                var connectionString = AppSettingsManager.GetConnectionString();

                SqlConnection conn = null;
                SqlDataReader rdr = null;
                var maxConnection = -1;
                try
                {
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    string stm = "show variables like \"max_connections\";";
                    SqlCommand cmdGet = new SqlCommand(stm, conn);
                    rdr = cmdGet.ExecuteReader();

                  

                    while (rdr.Read())
                    {
                        maxConnection = rdr.GetInt32(1);
                    }
                    if(maxConnection >= 0 && maxConnection < 10000)
                    {
                        if (rdr != null)
                        {
                            rdr.Close();
                        }

                        SqlCommand cmdUpdate = new SqlCommand();
                        cmdUpdate.Connection = conn;
                        cmdUpdate.CommandText = "set global max_connections = @Connections;";
                        cmdUpdate.Prepare();

                        cmdUpdate.Parameters.AddWithValue("@Connections", 10000);
                        cmdUpdate.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Error Updating Max Connections: {0}", ex.ToString());

                }
                finally
                {
                    if (rdr != null)
                    {
                        rdr.Close();
                    }

                    if (conn != null)
                    {
                        conn.Close();
                    }

                }
            }
        }

        public static void CreateDatabaseIfNotExists()
        {
            using (var ctx = new FUTCreationDatabase())
            {
                ctx.Database.CreateIfNotExists();
                FUTSettings.Instance.SaveChanges();
                ctx.SaveChanges();
            }
        }
    }
}
