using System;
using System.IO;
using AATool.Configuration;
using AATool.Utilities;

namespace AATool.Exceptions
{
    [Serializable]
    public class NoWorldException : IOException
    {
        public NoWorldException() : base(GetMessage())
        {

        }

        private static string GetMessage()
        {
            if (Config.Tracking.Source == TrackerSource.ActiveInstance)
            {
                return ActiveInstance.HasNumber
                    ? $"该实例下没有世界 {ActiveInstance.Number}"
                    : "当前活动的实例中没有世界";
            }

            if (Config.Tracking.Source == TrackerSource.SpecificWorld)
            {
                try
                {
                    string name = new DirectoryInfo(Config.Tracking.CustomWorldPath).Name;
                    return $"指定的世界 \"{name}\" 并不存在";
                }
                catch
                {
                    return $"指定的世界无效";
                }
            }
            return "自定义存档文件夹路径下没有世界";
        }
    }
}
