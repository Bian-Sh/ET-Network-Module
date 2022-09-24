using ProtoBuf;
namespace ET
{
    [Message(Opcode.G2C_Ping)]
    [ProtoContract]
    public partial class G2C_Ping : Object, IResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

        [ProtoMember(1)]
        public long Time { get; set; }
    }
}