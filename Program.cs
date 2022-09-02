using Cosmos.ModelBuilding;
using System.Diagnostics;
using System.Text.Json;

namespace AzureEF
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, Cosmos!");
            AzureContext azureContext = new();
            LocalContext localContext = new LocalContext();


            if (File.Exists("log.html"))
                File.Delete("log.html");

            Stopwatch sw = Stopwatch.StartNew();
            Stopwatch bigSw = Stopwatch.StartNew();

            try
            {

                List<WowAuction> remoteAuctions = azureContext.WowAuctions.ToList();

                LogMaker.Log($"Finding that {remoteAuctions.Count} are already on Azure took {sw.ElapsedMilliseconds} ms.");
                sw.Restart();
                List<WowAuction> localAuctions = localContext.WowAuctions.ToList();
                LogMaker.Log($"Finding that {localAuctions.Count} stored locally took {sw.ElapsedMilliseconds} ms.");
                sw.Restart();
                List<WowAuction> auctionsToUpload = new();
                List<WowAuction> auctionsToUpdate = new();
                foreach (WowAuction auction in localAuctions)
                {
                    WowAuction upload = remoteAuctions.FirstOrDefault(a => a.Id == auction.Id || a.AuctionId == auction.AuctionId);
                    if (upload == null)
                    {
                        auctionsToUpload.Add(auction);
                    }
                    else
                    {
                        auction.LastSeenTime = DateTime.UtcNow;
                        auctionsToUpdate.Add(upload);
                    }
                }
                LogMaker.Log($"Finding we have {auctionsToUpload.Count} auctions to upload and {auctionsToUpdate.Count} to update took {sw.ElapsedMilliseconds} ms.");
                sw.Restart();
                azureContext.UpdateRange(auctionsToUpdate.Take(100));

                LogMaker.Log($"Getting ready to update 100 auctions upload took {sw.ElapsedMilliseconds} ms.");
                sw.Restart();
                await azureContext.SaveChangesAsync();
                LogMaker.Log($"100 auctions updated in {sw.ElapsedMilliseconds} ms.");
                LogMaker.Log($"I love to see this log entry! It took {GetReadableTimeByMs(bigSw.ElapsedMilliseconds)} ms.");
                sw.Stop();


            }
            catch (Exception ex)
            {
                LogMaker.Log("___________________________________");
                LogMaker.Log(ex.Message);
                LogMaker.Log("___________________________________");
                LogMaker.Log(ex.StackTrace);
                LogMaker.Log("___________________________________");
                if (ex.InnerException != null)
                    LogMaker.Log($"{ex.InnerException}");
            }


            Console.WriteLine("Goodbye, Cosmos!");





        }

        static string GetReadableTimeByMs(long ms)
        {
            // Based on answers https://stackoverflow.com/questions/9993883/convert-milliseconds-to-human-readable-time-lapse
            TimeSpan t = TimeSpan.FromMilliseconds(ms);
            if (t.Hours > 0) return $"{t.Hours} hours {t.Minutes} minutes {t.Seconds} seconds";
            else if (t.Minutes > 0) return $"{t.Minutes}minutes {t.Seconds} seconds";
            else if (t.Seconds > 0) return $"{t.Seconds} seconds";
            else return $"{t.Milliseconds} milliseconds";
        }

        static void SeedData()
        {
            string fileName = "mini_kazzak.json";
            List<WowItem> wowItems = new();
            List<WowAuction> wowAuctions = new();
            string json = string.Empty;
            if (File.Exists(fileName))
            {
                json = File.ReadAllText(fileName);
            }

            List<Auction> jsonAuctions = new();
            WowItem trial = new();
            if (json.Length > 0)
            {
                Root root = JsonSerializer.Deserialize<Root>(json);
                jsonAuctions = root.auctions.ToList();
            }

            WowAuction extraAuction = new();
            foreach (Auction auction in jsonAuctions)
            {
                extraAuction.Id = Guid.NewGuid();
                extraAuction.AuctionId = auction.id;
                extraAuction.PartitionKey = "54321";
                extraAuction.LastSeenTime = DateTime.UtcNow;
                extraAuction.Quantity = auction.quantity;
                extraAuction.Buyout = auction.buyout;
                extraAuction.UnitPrice = auction.buyout;
                extraAuction.ItemId = auction.item.id;
                if (auction.time_left.ToLower().Contains("short"))
                    extraAuction.ShortTimeLeftSeen = true;
                wowAuctions.Add(extraAuction);

                trial = wowItems.FirstOrDefault(l => l.ItemId == extraAuction.ItemId);
                if (trial == null)
                {
                    WowItem next = new();
                    next.ItemId = extraAuction.ItemId;
                    next.Id = Guid.NewGuid();
                    next.Name = string.Empty;
                    next.BonusList = string.Empty;
                    wowItems.Add(next);
                }
                extraAuction = new();
            }

            LogMaker.Log($"Found {wowAuctions.Count} auctions and {wowItems.Count} items.");

            LocalContext context = new();
            context.WowAuctions.AddRange(wowAuctions);
            context.WowItems.AddRange(wowItems);
            context.SaveChanges();


        }
    }
}