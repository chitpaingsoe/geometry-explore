using StackExchange.Redis;

namespace LoadTest
{
    public class RedisClientHelper
    {
        private IDatabase _cache;

        public RedisClientHelper(IDatabase cache)
        {
            _cache = cache;
        }

      

        public async Task<string> Get(string key)
        {
            key = "lta:" + key;
            LuaScript script = LuaScript.Prepare(@"return redis.call('GET', @key)");
            var res = await _cache
               .ScriptEvaluateAsync(script, new
               {
                   key = new RedisKey(key),
               });
            return res?.ToString();
        }

        public async Task<string> Delete(string key)
        {
            key = "lta:" + key;
            LuaScript script = LuaScript.Prepare(@"return redis.call('DEL', @key)");
            var res = await _cache
               .ScriptEvaluateAsync(script, new
               {
                   key = new RedisKey(key),
               });
            return res?.ToString();
        }

        public async Task<string> Update(string key)
        {
            key = "lta:" + key;
            LuaScript script = LuaScript.Prepare(@"
                local old = redis.call('GET', @key)
                return redis.call('SET', @key, old - 1)

            ");
            var res = await _cache
               .ScriptEvaluateAsync(script, new
               {
                   key = new RedisKey(key)
               });
            return res?.ToString();
        }
        public async Task<string> Add(string key, int value)
        {
            key = "lta:" + key;
            LuaScript script = LuaScript.Prepare(@"
                return redis.call('SET', @key, tonumber(@value))

            ");
            var res = await _cache
               .ScriptEvaluateAsync(script, new
               {
                   key = new RedisKey(key),
                   value = new RedisValue(value.ToString())
               });
            return res?.ToString();
        }
    }
}
