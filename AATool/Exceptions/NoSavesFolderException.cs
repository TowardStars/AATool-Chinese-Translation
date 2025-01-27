using System;
using System.IO;
using AATool.Configuration;
using AATool.Utilities;

namespace AATool.Exceptions
{
    [Serializable]
    public class NoSavesFolderException : IOException
    {
        public readonly string MissingPath;

        public NoSavesFolderException(string missingPath) : base(GetMessage())
        {
            this.MissingPath = missingPath;
        }

        private static string GetMessage()
        {
            if (Config.Tracking.Source == TrackerSource.ActiveInstance)
            {
                return ActiveInstance.HasNumber
                    ? $"实例 {ActiveInstance.Number} 存档文件夹并不存在"
                    : "活动实例缺少存档文件夹";
            }

            return "自定义世界路径并不存在";
        }
    }
}
