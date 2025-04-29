using GameSDK.RemoteConfigs;
using UnityEngine;

namespace Test
{
    public class RemoteEntityElement : MonoBehaviour
    {
        [RemoteValue("test")] public TestRemote test;
    }
}