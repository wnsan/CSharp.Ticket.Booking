using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using StackExchange.Redis;

namespace CSharp.Ticket.Booking.DataAccess.InMemoryDatabase
{
    public class TicketPoolRepository : ITicketPoolRepository
    {
        //private readonly IDistributedCache _redisCache;
        private static ConnectionMultiplexer _redis;
        private static IDatabase db;
        private static SearchCommands ft;
        private static JsonCommands json;
        private static List<string> l;
        public TicketPoolRepository()
        {
            if (null == _redis)
            {
                _redis = ConnectionMultiplexer.Connect("localhost:6379,ConnectTimeout=10000", c =>
                {
                    c.AbortOnConnectFail = false;
                    c.ConnectTimeout = 30000;
                    c.ResponseTimeout = 30000;
                });
                db = _redis.GetDatabase();

                ft = db.FT();
                json = db.JSON();
                l = new List<string>();
            }
            //_redisCache = redisCache;
        }

        public int GetTicketCount()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Ticket>> GetTickets(ILogger logger, int numberOfTicket, Guid bookingId)
        {
            bool setLock = false;
            do
            {
                try
                {
                    setLock = await db.StringSetAsync("lock", "lock_value", TimeSpan.FromSeconds(30), When.NotExists);
                }
                catch (Exception ex)
                {
                    setLock = false;
                }
            } while (!setLock);
            var res = (await ft.SearchAsync("idx:tickets", new Query("@Status:\"available\""))).Documents.Select(x => x["json"]);

            var result = res.Select(c => JsonConvert.DeserializeObject<Ticket>(c)).Take(numberOfTicket).ToList();
            foreach (var ticket in result)
            {
                ticket.BookingId = bookingId;
                ticket.ExpireDate = DateTime.UtcNow;
                ticket.Status = "booking";
                var t = new
                {
                    Id = ticket.Id.ToString(),
                    Status = "booking",
                    BookingId = ticket.BookingId?.ToString(),
                    EventId = ticket.EventId.ToString(),
                    ExpireDate = ticket.ExpireDate?.Ticks
                };
                await json.SetAsync("ticket:" + ticket.Id.ToString(), "$", t);
            }
            string lua_script = @"  
                    if (redis.call('GET', KEYS[1]) == ARGV[1]) then  
                        redis.call('DEL', KEYS[1])  
                        return true  
                    else  
                        return false  
                    end  
                    ";
            await db.ScriptEvaluateAsync(lua_script, new RedisKey[] { "lock" }, new RedisValue[] { "lock_value" });
            return result;
        }

        public async Task PrepareDataAsync(List<Ticket> tickets)
        {
            var db = _redis.GetDatabase();
            var ft = db.FT();
            var json = db.JSON();

            var schema = new Schema()
                .AddTextField(new FieldName("$.Id", "Id"))
                .AddTextField(new FieldName("$.Status", "Status"))
                .AddTextField(new FieldName("$.BookingId", "BookingId"))
                .AddTextField(new FieldName("$.EventId", "EventId"))
                .AddNumericField(new FieldName("$.ExpireDate", "ExpireDate"));
            ft.Create(
                "idx:tickets",
                new FTCreateParams().On(IndexDataType.JSON).Prefix("ticket:"),
                schema);
            foreach (var ticket in tickets)
            {
                var t = new
                {
                    Id = ticket.Id.ToString(),
                    Status = ticket.Status,
                    BookingId = ticket.BookingId?.ToString(),
                    EventId = ticket.EventId.ToString(),
                    ExpireDate = ticket.ExpireDate?.Ticks
                };
                await json.SetAsync("ticket:" + ticket.Id.ToString(), "$", t);
            }
        }
    }
}