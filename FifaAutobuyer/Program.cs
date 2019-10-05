using FifaAutobuyer.Database;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Managers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Threading;
using FifaAutobuyer.Database.Web;
using FifaAutobuyer.Fifa.ActionScheduler;
using FifaAutobuyer.Fifa.EADatabase;
using FifaAutobuyer.Fifa.MailService;
using FifaAutobuyer.WebServer;
using FluentScheduler;

namespace FifaAutobuyer
{
    class Program
    {
        static void Main(string[] args)
        { 
            var connectionString = AppSettingsManager.GetConnectionString();

            AppDomain.CurrentDomain.SetData("DataDirectory", Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            
            Console.Title = "FifaAutobuyer // " + AppSettingsManager.GetInstanceDescription() + " // Path: " + Path.GetFileName(Environment.CurrentDirectory) + " // Port: " + AppSettingsManager.GetWebappPort();
            Console.WriteLine("Connectionstring: " + connectionString);
            Console.WriteLine();
            Console.WriteLine("Description: " + AppSettingsManager.GetInstanceDescription());


            Console.WriteLine("Create Database if not exists...");
            DatabaseScheduler.CreateDatabaseIfNotExists();


            using (var ctx = new FUTCreationDatabase())
            {
                if (!ctx.WebpanelAccounts.Any())
                {
                    ctx.WebpanelAccounts.Add(new WebpanelAccount() { Username = "admin", Password = "fifa123", Role = WebAccessRole.Administrator });
                    ctx.WebpanelAccounts.Add(new WebpanelAccount() { Username = "mod", Password = "mod123", Role = WebAccessRole.Moderator });
                }
                ctx.SaveChanges();
            }


            Console.WriteLine("Updating Settings...");
            ProxyManager.Initialize();

            Console.WriteLine("Updating Players and Consumables...");
            EADatabaseScraper.UpdateAsync().Wait();
            FUTItemManager.LoadItems();

            Console.WriteLine("Initializing BotManager...");
            BotManager.Initialize();
            Console.WriteLine("Initializing ActionScheduler...");
            ActionScheduler.CreateScheduler();
            Console.WriteLine("Resetting PriceChecks...");
            ItemListManager.ResetPriceCheckEverywhere();
            Console.WriteLine("Initializing MailService...");
            MailClientFactory.Initialize();

            Console.WriteLine("Initializing HttpServer...");
            HttpWebServer.Start(AppSettingsManager.GetWebappPort());

            Console.WriteLine("AntiCaptcha: " + AppSettingsManager.GetAntiCaptchaKey());

            Console.WriteLine("HttpServer hosted on port " + AppSettingsManager.GetWebappPort());

            Console.WriteLine("Ready...");

            //var dbScheudulerDeleteOldTrades = new Timer((e) =>
            // {
            //     AuctionManager.RemoveOldAuctions();
            // }, null, 0, (long)TimeSpan.FromHours(1).TotalMilliseconds);


            //var dbScheudulerSaveBotStatistics = new Timer((e) =>
            //{
            //    DatabaseScheduler.SaveBotStatistics();
            //}, null, 0, (long)TimeSpan.FromMinutes(30).TotalMilliseconds);

            //var dbScheudulerDeleteOldLogs = new Timer((e) =>
            //{
            //    DatabaseScheduler.DeleteOldLogs();
            //}, null, 0, (long)TimeSpan.FromMinutes(30).TotalMilliseconds);

            while (true)
            {
                var command = Console.ReadLine();
                if (command == "exit")
                {
                    Environment.Exit(0);
                }

            }
        }
    }
}
