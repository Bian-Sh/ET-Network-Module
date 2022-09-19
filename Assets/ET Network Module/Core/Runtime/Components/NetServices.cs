using System.Collections.Generic;
namespace ET
{
    public static class NetServices
    {
        public const int checkInteral = 2000;
        public const int recvMaxIdleTime = 60000;
        public const int sendMaxIdleTime = 60000;
        public static HashSet<AService> Services = new HashSet<AService>();
        public static void Add(AService kService)
        {
            if (!kService.IsDispose())
            {
                Services.Add(kService);
            }
        }

        public static void Remove(AService kService)
        {
            if (!kService.IsDispose())
            {
                Services.Remove(kService);
            }
        }
    }
}
