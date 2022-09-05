using ET;
using ProtoBuf;
using System.Collections.Generic;
namespace ET
{
    [ResponseType(nameof(M2C_TestResponse))]
    [Message(OuterOpcode.C2M_TestRequest)]
    [ProtoContract]
    public partial class C2M_TestRequest : Object, IActorLocationRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(1)]
        public string request { get; set; }

    }

    [Message(OuterOpcode.M2C_TestResponse)]
    [ProtoContract]
    public partial class M2C_TestResponse : Object, IActorLocationResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

        [ProtoMember(1)]
        public string response { get; set; }

    }

    [ResponseType(nameof(Actor_TransferResponse))]
    [Message(OuterOpcode.Actor_TransferRequest)]
    [ProtoContract]
    public partial class Actor_TransferRequest : Object, IActorLocationRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(1)]
        public int MapIndex { get; set; }

    }

    [Message(OuterOpcode.Actor_TransferResponse)]
    [ProtoContract]
    public partial class Actor_TransferResponse : Object, IActorLocationResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

    }

    [ResponseType(nameof(G2C_EnterMap))]
    [Message(OuterOpcode.C2G_EnterMap)]
    [ProtoContract]
    public partial class C2G_EnterMap : Object, IRequest
    {
        [ProtoMember(1)]
        public int RpcId { get; set; }

        [ProtoMember(2)]
        public string UniqueIdentifier { get; set; }

    }

    [Message(OuterOpcode.G2C_EnterMap)]
    [ProtoContract]
    public partial class G2C_EnterMap : Object, IResponse
    {
        [ProtoMember(1)]
        public int RpcId { get; set; }

        [ProtoMember(2)]
        public int Error { get; set; }

        [ProtoMember(3)]
        public string Message { get; set; }

        // 自己unitId
        [ProtoMember(4)]
        public long MyId { get; set; }

    }

    [Message(OuterOpcode.MoveInfo)]
    [ProtoContract]
    public partial class MoveInfo : Object
    {
        [ProtoMember(1)]
        public List<float> X = new List<float>();

        [ProtoMember(2)]
        public List<float> Y = new List<float>();

        [ProtoMember(3)]
        public List<float> Z = new List<float>();

        [ProtoMember(4)]
        public float A { get; set; }

        [ProtoMember(5)]
        public float B { get; set; }

        [ProtoMember(6)]
        public float C { get; set; }

        [ProtoMember(7)]
        public float W { get; set; }

        [ProtoMember(8)]
        public int TurnSpeed { get; set; }

    }

    [Message(OuterOpcode.UnitInfo)]
    [ProtoContract]
    public partial class UnitInfo : Object
    {
        [ProtoMember(1)]
        public long UnitId { get; set; }

        [ProtoMember(2)]
        public int ConfigId { get; set; }

        [ProtoMember(3)]
        public int Type { get; set; }

        [ProtoMember(4)]
        public float X { get; set; }

        [ProtoMember(5)]
        public float Y { get; set; }

        [ProtoMember(6)]
        public float Z { get; set; }

        [ProtoMember(7)]
        public float ForwardX { get; set; }

        [ProtoMember(8)]
        public float ForwardY { get; set; }

        [ProtoMember(9)]
        public float ForwardZ { get; set; }

        [ProtoMember(10)]
        public List<int> Ks = new List<int>();

        [ProtoMember(11)]
        public List<long> Vs = new List<long>();

        [ProtoMember(12)]
        public MoveInfo MoveInfo { get; set; }

    }

    [Message(OuterOpcode.M2C_CreateUnits)]
    [ProtoContract]
    public partial class M2C_CreateUnits : Object, IActorMessage
    {
        [ProtoMember(2)]
        public List<UnitInfo> Units = new List<UnitInfo>();

    }

    [Message(OuterOpcode.M2C_CreateMyUnit)]
    [ProtoContract]
    public partial class M2C_CreateMyUnit : Object, IActorMessage
    {
        [ProtoMember(1)]
        public UnitInfo Unit { get; set; }

    }

    [Message(OuterOpcode.M2C_StartSceneChange)]
    [ProtoContract]
    public partial class M2C_StartSceneChange : Object, IActorMessage
    {
        [ProtoMember(1)]
        public long SceneInstanceId { get; set; }

        [ProtoMember(2)]
        public string SceneName { get; set; }

    }

    [Message(OuterOpcode.M2C_RemoveUnits)]
    [ProtoContract]
    public partial class M2C_RemoveUnits : Object, IActorMessage
    {
        [ProtoMember(2)]
        public List<long> Units = new List<long>();

    }

    [Message(OuterOpcode.C2M_PathfindingResult)]
    [ProtoContract]
    public partial class C2M_PathfindingResult : Object, IActorLocationMessage
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(1)]
        public float X { get; set; }

        [ProtoMember(2)]
        public float Y { get; set; }

        [ProtoMember(3)]
        public float Z { get; set; }

    }

    [Message(OuterOpcode.C2M_Stop)]
    [ProtoContract]
    public partial class C2M_Stop : Object, IActorLocationMessage
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

    }

    [Message(OuterOpcode.M2C_PathfindingResult)]
    [ProtoContract]
    public partial class M2C_PathfindingResult : Object, IActorMessage
    {
        [ProtoMember(1)]
        public long Id { get; set; }

        [ProtoMember(2)]
        public float X { get; set; }

        [ProtoMember(3)]
        public float Y { get; set; }

        [ProtoMember(4)]
        public float Z { get; set; }

        [ProtoMember(5)]
        public List<float> Xs = new List<float>();

        [ProtoMember(6)]
        public List<float> Ys = new List<float>();

        [ProtoMember(7)]
        public List<float> Zs = new List<float>();

    }

    [Message(OuterOpcode.M2C_Stop)]
    [ProtoContract]
    public partial class M2C_Stop : Object, IActorMessage
    {
        [ProtoMember(1)]
        public int Error { get; set; }

        [ProtoMember(2)]
        public long Id { get; set; }

        [ProtoMember(3)]
        public float X { get; set; }

        [ProtoMember(4)]
        public float Y { get; set; }

        [ProtoMember(5)]
        public float Z { get; set; }

        [ProtoMember(6)]
        public float A { get; set; }

        [ProtoMember(7)]
        public float B { get; set; }

        [ProtoMember(8)]
        public float C { get; set; }

        [ProtoMember(9)]
        public float W { get; set; }

    }

    [ResponseType(nameof(G2C_Ping))]
    [Message(OuterOpcode.C2G_Ping)]
    [ProtoContract]
    public partial class C2G_Ping : Object, IRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

    }

    [Message(OuterOpcode.G2C_Ping)]
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

    [Message(OuterOpcode.G2C_Test)]
    [ProtoContract]
    public partial class G2C_Test : Object, IMessage
    {
    }

    [ResponseType(nameof(M2C_Reload))]
    [Message(OuterOpcode.C2M_Reload)]
    [ProtoContract]
    public partial class C2M_Reload : Object, IRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(1)]
        public string Account { get; set; }

        [ProtoMember(2)]
        public string Password { get; set; }

    }

    [Message(OuterOpcode.M2C_Reload)]
    [ProtoContract]
    public partial class M2C_Reload : Object, IResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

    }

    [ResponseType(nameof(R2C_Login))]
    [Message(OuterOpcode.C2R_Login)]
    [ProtoContract]
    public partial class C2R_Login : Object, IRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(1)]
        public int Role { get; set; }

    }

    [Message(OuterOpcode.R2C_Login)]
    [ProtoContract]
    public partial class R2C_Login : Object, IResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

        [ProtoMember(1)]
        public string Address { get; set; }

        [ProtoMember(2)]
        public long Key { get; set; }

        [ProtoMember(3)]
        public long GateId { get; set; }

    }

    [ResponseType(nameof(G2C_LoginGate))]
    [Message(OuterOpcode.C2G_LoginGate)]
    [ProtoContract]
    public partial class C2G_LoginGate : Object, IRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(1)]
        public long Key { get; set; }

        [ProtoMember(2)]
        public long GateId { get; set; }

    }

    [Message(OuterOpcode.G2C_LoginGate)]
    [ProtoContract]
    public partial class G2C_LoginGate : Object, IResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

        [ProtoMember(1)]
        public long PlayerId { get; set; }

    }

    [Message(OuterOpcode.G2C_TestHotfixMessage)]
    [ProtoContract]
    public partial class G2C_TestHotfixMessage : Object, IMessage
    {
        [ProtoMember(1)]
        public string Info { get; set; }

    }

    [ResponseType(nameof(M2C_TestRobotCase))]
    [Message(OuterOpcode.C2M_TestRobotCase)]
    [ProtoContract]
    public partial class C2M_TestRobotCase : Object, IActorLocationRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(1)]
        public int N { get; set; }

    }

    [Message(OuterOpcode.M2C_TestRobotCase)]
    [ProtoContract]
    public partial class M2C_TestRobotCase : Object, IActorLocationResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

        [ProtoMember(1)]
        public int N { get; set; }

    }

    [ResponseType(nameof(M2C_TransferMap))]
    [Message(OuterOpcode.C2M_TransferMap)]
    [ProtoContract]
    public partial class C2M_TransferMap : Object, IActorLocationRequest
    {
        [ProtoMember(1)]
        public int RpcId { get; set; }

    }

    [Message(OuterOpcode.M2C_TransferMap)]
    [ProtoContract]
    public partial class M2C_TransferMap : Object, IActorLocationResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

    }

    [Message(OuterOpcode.CaveInfoProto)]
    [ProtoContract]
    public partial class CaveInfoProto : Object
    {
        [ProtoMember(1)]
        public int Index { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public bool Usage { get; set; }

        [ProtoMember(4)]
        public float Size_x { get; set; }

        [ProtoMember(5)]
        public float Size_y { get; set; }

        [ProtoMember(6)]
        public float CreateTime { get; set; }

        [ProtoMember(7)]
        public int Width { get; set; }

        [ProtoMember(8)]
        public int Height { get; set; }

        [ProtoMember(9)]
        public Transfrom Trans { get; set; }

    }

    [ResponseType(nameof(M2C_UpLoadCaveInfo))]
    [Message(OuterOpcode.C2M_UpLoadCaveInfo)]
    [ProtoContract]
    public partial class C2M_UpLoadCaveInfo : Object, IActorLocationRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public List<CaveInfoProto> CProto = new List<CaveInfoProto>();

    }

    [Message(OuterOpcode.M2C_UpLoadCaveInfo)]
    [ProtoContract]
    public partial class M2C_UpLoadCaveInfo : Object, IActorLocationResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

    }

    [Message(OuterOpcode.M2C_UpdateCaveInfo)]
    [ProtoContract]
    public partial class M2C_UpdateCaveInfo : Object, IActorMessage
    {
        [ProtoMember(1)]
        public List<CaveInfoProto> CaveInfo = new List<CaveInfoProto>();

    }

    [Message(OuterOpcode.Transfrom)]
    [ProtoContract]
    public partial class Transfrom : Object
    {
        [ProtoMember(9)]
        public float P_x { get; set; }

        [ProtoMember(10)]
        public float P_y { get; set; }

        [ProtoMember(11)]
        public float P_z { get; set; }

        [ProtoMember(12)]
        public float R_x { get; set; }

        [ProtoMember(13)]
        public float R_y { get; set; }

        [ProtoMember(14)]
        public float R_z { get; set; }

        [ProtoMember(15)]
        public float R_w { get; set; }

    }

    [Message(OuterOpcode.C2M_HandleKeyState)]
    [ProtoContract]
    public partial class C2M_HandleKeyState : Object, IActorLocationMessage
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(4)]
        public float TouchX { get; set; }

        [ProtoMember(5)]
        public float TouchY { get; set; }

        [ProtoMember(1)]
        public bool Key1 { get; set; }

        [ProtoMember(2)]
        public bool Key2 { get; set; }

        [ProtoMember(3)]
        public bool Ke3 { get; set; }

        [ProtoMember(6)]
        public string ModeName { get; set; }

        [ProtoMember(7)]
        public Transfrom Trans { get; set; }

    }

    [Message(OuterOpcode.M2C_HandleKeyState)]
    [ProtoContract]
    public partial class M2C_HandleKeyState : Object, IActorMessage
    {
        [ProtoMember(4)]
        public float TouchX { get; set; }

        [ProtoMember(5)]
        public float TouchY { get; set; }

        [ProtoMember(1)]
        public bool Key1 { get; set; }

        [ProtoMember(2)]
        public bool Key2 { get; set; }

        [ProtoMember(3)]
        public bool Ke3 { get; set; }

        [ProtoMember(6)]
        public string ModeName { get; set; }

        [ProtoMember(7)]
        public Transfrom Trans { get; set; }

    }

    [Message(OuterOpcode.C2M_SelectModel)]
    [ProtoContract]
    public partial class C2M_SelectModel : Object, IMessage
    {
        [ProtoMember(1)]
        public int renderModel { get; set; }

    }

    [Message(OuterOpcode.M2C_SelectModel)]
    [ProtoContract]
    public partial class M2C_SelectModel : Object, IActorMessage
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(1)]
        public int renderModel { get; set; }

    }

    [ResponseType(nameof(M2C_GetData))]
    [Message(OuterOpcode.C2M_GetData)]
    [ProtoContract]
    public partial class C2M_GetData : Object, IRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

    }

    [Message(OuterOpcode.M2C_GetData)]
    [ProtoContract]
    public partial class M2C_GetData : Object, IResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

        [ProtoMember(1)]
        public List<CaveInfoProto> CaveInfo = new List<CaveInfoProto>();

        [ProtoMember(2)]
        public List<string> ips = new List<string>();

        [ProtoMember(3)]
        public int Volume { get; set; }

    }

    [ResponseType(nameof(M2C_GetCaveInfo))]
    [Message(OuterOpcode.C2M_GetCaveInfo)]
    [ProtoContract]
    public partial class C2M_GetCaveInfo : Object, IRequest
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

    }

    [Message(OuterOpcode.M2C_GetCaveInfo)]
    [ProtoContract]
    public partial class M2C_GetCaveInfo : Object, IResponse
    {
        [ProtoMember(90)]
        public int RpcId { get; set; }

        [ProtoMember(91)]
        public int Error { get; set; }

        [ProtoMember(92)]
        public string Message { get; set; }

        [ProtoMember(1)]
        public List<CaveInfoProto> CaveInfo = new List<CaveInfoProto>();

    }

}
