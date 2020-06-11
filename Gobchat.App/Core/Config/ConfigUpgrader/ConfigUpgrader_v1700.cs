using Newtonsoft.Json.Linq;

namespace Gobchat.Core.Config
{
    internal sealed class ConfigUpgrader_v1700 : IJsonTransformer
    {
        public JObject Transform(JObject json)
        {
            JObject dst = (JObject)json.DeepClone();
            dst["version"] = 1700;

            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-1.ffgroup", 0);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-2.ffgroup", 1);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-3.ffgroup", 2);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-4.ffgroup", 3);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-5.ffgroup", 4);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-6.ffgroup", 5);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-7.ffgroup", 6);

            return dst;
        }
    }
}