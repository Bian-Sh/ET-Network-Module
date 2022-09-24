using ProtoBuf;
namespace ET
{
    [ResponseType(nameof(G2C_Ping))]
    [Message(Opcode.C2G_Ping)]
    [ProtoContract]
    public partial class C2G_Ping : Object, IRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }
    }
}