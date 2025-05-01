using UnityEngine;

namespace GameSDK.RemoteConfigs
{
    [CreateAssetMenu(fileName = "DefaultRemoteValuesConfig", menuName = "GameSDK/RemoteConfigs/DefaultRemoteValuesConfig")]
    public class DefaultRemoteValuesConfig : ScriptableObject
    {
        public DefaultRemoteValue[] DefaultValues;
    }
}