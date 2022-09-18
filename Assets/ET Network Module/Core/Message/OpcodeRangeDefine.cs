namespace ET
{
    public static class OpcodeRangeDefine
    {
        // 10001 - 30000 是pb，中间分成两个部分，外网pb跟内网pb
        public const ushort PbMinOpcode = 10001;
        public const ushort OuterMinOpcode = 10001;
        public const ushort OuterMaxOpcode = 20000;

        // 20001-30000 内网pb
        public const ushort InnerMinOpcode = 20001;
        public const ushort PbMaxOpcode = 30000;
        public const ushort MaxOpcode = 60000;
    }
}