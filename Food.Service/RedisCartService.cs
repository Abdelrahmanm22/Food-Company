using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Food.Domain.Models;
using Food.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Food.Service
{
    public class RedisCartService : IRedisCartService
    {
        private readonly IDatabase _database;
        public RedisCartService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }
        public async Task<SessionCart?> GetCartAsync(string cartKey)
        {
            var data = await _database.StringGetAsync(cartKey);
            return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<SessionCart>(data!);
        }
        public async Task<SessionCart?> UpdateCartAsync(SessionCart cart)
        {
            var created = await _database.StringSetAsync(cart.Id, JsonSerializer.Serialize(cart),TimeSpan.FromDays(30));
            if(!created) return null;
            return await GetCartAsync(cart.Id);
        }
        public async Task<bool> DeleteCartAsync(string cartKey)
        {
            return await _database.KeyDeleteAsync(cartKey);
        }
        public async Task<List<SessionCart>> GetAllCartsForSessionAsync(int sessionId, List<string> userIds)
        {
            var carts = new List<SessionCart>();
            foreach (var userId in userIds)
            {
                var cartKey = $"cart:{sessionId}:{userId}";
                var cart = await GetCartAsync(cartKey);
                if (cart != null)
                {
                    carts.Add(cart);
                }
            }
            return carts;
        }
        public async Task DeleteAllCartsForSessionAsync(int sessionId, List<string> userIds)
        {
            foreach (var userId in userIds)
            {
                var cartKey = $"cart:{sessionId}:{userId}";
                await DeleteCartAsync(cartKey);
            }
        }
    }
}
