namespace GameSDK.RemoteConfigs
{
    public interface IDeserializerObject
    {
        bool Check(object obj, System.Type type);
        object Deserialize(string value, object obj, System.Type type);
    }
}