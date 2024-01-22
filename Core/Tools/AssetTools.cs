#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using GameSDK.Core.Properties;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameSDK.Core.Tools
{
    public class AssetTools
    {
        private const string SDKName = "com.cucumber.gamesdk";
        private const string WebGLTemplates = "WebGLTemplates";

        internal static string GetSDKPath()
        {
            var parent = Directory.GetParent(Application.dataPath)?.FullName;

            if (parent == null)
            {
                return string.Empty;
            }

            var packagePath = Path.Combine(parent, "Packages");
            var directories = Directory.GetDirectories(packagePath, "*", SearchOption.TopDirectoryOnly).ToList();
            packagePath = Path.Combine(parent, "Library\\PackageCache");
            directories.AddRange(Directory.GetDirectories(packagePath, "*", SearchOption.TopDirectoryOnly));

            foreach (var directory in directories)
            {
                if (Path.GetFileName(directory).ToLower().Contains(SDKName))
                {
                    return directory;
                }
            }

            return string.Empty;
        }

        internal static bool TryGetPathPlugin(PluginServicesType plugin, out string path)
        {
            var sdkPath = GetSDKPath();

            if (string.IsNullOrEmpty(sdkPath))
            {
                path = string.Empty;
                return false;
            }
            
            var pluginName = GetPluginName(plugin);
            path = string.Empty;

            var directories = Directory.GetDirectories(sdkPath, "*", SearchOption.AllDirectories);

            foreach (var directory in directories)
            {
                if (Path.GetFileName(directory) != pluginName) continue;

                path = directory;
                return true;
            }
            
            return false;
        }

        internal static bool TryGetPathTemplatePlugin(PluginServicesType plugin, out string path)
        {
            if (TryGetPathPlugin(plugin, out path) == false)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{SDKName}]: Plugin: {plugin} not found!");
#endif
                return false;
            }

            path += "\\Template";

            if (Directory.Exists(path) == false)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{SDKName}]: Plugin {plugin} template not found!");
#endif
                return false;
            }

            return true;
        }
        
        internal static bool TemplateExist(PluginServicesType plugin)
        {
            var pathTemplate = Path.Combine(GetTemplatePath(), GetPluginName(plugin));

            if (Directory.Exists(pathTemplate))
            {
                if (TryGetPathTemplatePlugin(plugin, out var templatePath) == false) return false;

                var unityTemplatePath = GetTemplatePath();

                var unityTemplatePathWithPlugin = Path.Combine(unityTemplatePath, $"{GetPluginName(plugin)}");
                
                if (AreFoldersEqual(new DirectoryInfo(templatePath), new DirectoryInfo(unityTemplatePathWithPlugin))) return true;
            }

            return false;
        }

        internal static string GetTemplatePath()
        {
            var editorPath = Application.dataPath;

            var pathTemplate = Path.Combine(editorPath, WebGLTemplates);
            return pathTemplate;
        }

        internal static bool TryCreateTemplate(PluginServicesType plugin)
        {
            if (TryGetPathTemplatePlugin(plugin, out var templatePath) == false) return false;

            var unityTemplatePath = GetTemplatePath();

            if (Directory.Exists(unityTemplatePath) == false)
            {
                Directory.CreateDirectory(unityTemplatePath);
            }

            var unityTemplatePathWithPlugin = Path.Combine(unityTemplatePath, $"{GetPluginName(plugin)}");

            if (AreFoldersEqual(new DirectoryInfo(templatePath), new DirectoryInfo(unityTemplatePathWithPlugin))) return false;

            if (Directory.Exists(unityTemplatePathWithPlugin))
            {
                Directory.Delete(unityTemplatePathWithPlugin, true);
            }

            try
            {
                CopyDirectory(templatePath, unityTemplatePathWithPlugin, true);
            }
            catch (UnauthorizedAccessException)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{SDKName}]: The template could not be copied. You don't have permission to write the file!" +
                                 $"\nCopy {templatePath} folder to {unityTemplatePath} and select template in Player Settings!" +
                                 $"\nOr open Unity Editor with Administrator permissions!");
#endif
            }

            return true;
        }

        private static string GetPluginName(PluginServicesType pluginServicesType)
        {
            return pluginServicesType switch
            {
                PluginServicesType.None => string.Empty,
                PluginServicesType.YaGames => PluginNames.YaGames,
                _ => string.Empty
            };
        }

        internal static bool SwitchTemplate(PluginServicesType plugin)
        {
            if (plugin == PluginServicesType.None)
            {
                PlayerSettings.WebGL.template = "APPLICATION:Default";
                return true;
            }
            
            if (TemplateExist(plugin))
            {
                PlayerSettings.WebGL.template = $"PROJECT:{GetPluginName(plugin)}";
                return true;
            }

            if (TryCreateTemplate(plugin))
            {
                PlayerSettings.WebGL.template = $"PROJECT:{GetPluginName(plugin)}";
                return true;
            }
            
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            return false;
        }
        
        private static bool AreFoldersEqual(DirectoryInfo folder1, DirectoryInfo folder2)
        {
            // Check if both folders exist
            if (!folder1.Exists || !folder2.Exists)
            {
                return false;
            }

            // Check if both folders have the same number of files
            FileInfo[] files1 = folder1.GetFiles();
            FileInfo[] files2 = folder2.GetFiles();
            if (files1.Length != files2.Length)
            {
                return false;
            }

            // Check if both folders have the same number of subfolders
            DirectoryInfo[] subfolders1 = folder1.GetDirectories();
            DirectoryInfo[] subfolders2 = folder2.GetDirectories();
            if (subfolders1.Length != subfolders2.Length)
            {
                return false;
            }

            // Compare files in both folders
            for (int i = 0; i < files1.Length; i++)
            {
                if (files1[i].Name != files2[i].Name || files1[i].Length != files2[i].Length ||
                    (files1[1].FullName.Split(".")[^1] != "meta" &&
                     files1[2].FullName.Split(".")[^1] != "meta" &&
                     files1[i].LastWriteTimeUtc != files2[i].LastWriteTimeUtc))
                {
                    return false;
                }
            }

            // Compare subfolders in both folders recursively
            for (int i = 0; i < subfolders1.Length; i++)
            {
                if (!AreFoldersEqual(subfolders1[i], subfolders2[i]))
                {
                    return false;
                }
            }

            return true;
        }
        
        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
#endif