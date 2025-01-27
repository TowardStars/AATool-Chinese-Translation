using System;
using System.IO;
using AATool.Configuration;
using AATool.Utilities;

namespace AATool.Exceptions
{
    [Serializable]
    public class InvalidPathException : ArgumentException
    {
        public InvalidPathException() : base($"自定义存档路径中存在非法字符")
        {

        }
    }
}
