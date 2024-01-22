#if UNITY_EDITOR
using GameSDK.Core.Tools;
using UnityEditor;
using UnityEngine;

namespace GameSDK.Core.Properties
{
    [CreateAssetMenu(fileName = "GameSDKProperties", menuName = "GameSDK/SDKProperties")]
    public class GameSDKProperties : ScriptableObject
    {
        [SerializeField]
        [GameSDK.Core.PropertyAttributes.ReadOnly]
        private BuildTarget _platform;

        [SerializeField]
        private WebGLTemplate _webGLTemplate;

        public void OnValidate()
        {
            _platform = EditorUserBuildSettings.activeBuildTarget;
        
            if (_platform == BuildTarget.WebGL)
            {
                SwitchTemplate();
            }
        }

        private void SwitchTemplate()
        {
            switch (_webGLTemplate)
            {
                case WebGLTemplate.None:
                    break;
                case WebGLTemplate.YaGames:
                    AssetTools.SwitchTemplate(PluginServicesType.YaGames);
                    break;
                case WebGLTemplate.VkGames:
                    AssetTools.SwitchTemplate(PluginServicesType.VkGames);
                    break;
            }  
        }
    }
}
#endif