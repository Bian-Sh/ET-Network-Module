namespace ET
{
    public abstract class Object
    {
        public override string ToString() => LitJson.JsonMapper.ToJson(this);
    }
}