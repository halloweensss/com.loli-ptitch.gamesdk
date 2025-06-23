using System.IO;
using System.Text;
using GameSDK.Extensions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GameSDK.Plugins.YaMetricaWeb.Editor
{
    public class BuildPostProcessor : IPostprocessBuildWithReport
    {
        private const string Name = "YaMetricaWeb";

        private const string SDKCounter = @"<script type=""text/javascript"">
        (function(m,e,t,r,i,k,a){m[i]=m[i]||function(){(m[i].a=m[i].a||[]).push(arguments)};
        m[i].l=1*new Date();
        for (var j = 0; j < document.scripts.length; j++) {if (document.scripts[j].src === r) { return; }}
        k=e.createElement(t),a=e.getElementsByTagName(t)[0],k.async=1,k.src=r,a.parentNode.insertBefore(k,a)})
        (window, document, ""script"", ""https://mc.yandex.ru/metrika/tag.js"", ""ym""); </script>";

        private const string InitializeCounter = @"<script type=""text/javascript""> ym({0}, ""init"", {1}); </script>";

        private const string ImgSrc =
            @"<noscript><div><img src=""https://mc.yandex.ru/watch/{0}"" style=""position:absolute; left:-9999px;"" alt="""" /></div></noscript>";

        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            var summary = report.summary;

            if (summary.platform != BuildTarget.WebGL)
                return;

            var indexHtmlPath = Path.Combine(summary.outputPath, "index.html");

            if (File.Exists(indexHtmlPath) == false)
                return;

            var htmlContent = File.ReadAllText(indexHtmlPath);

            var settingsArray = Resources.LoadAll<YaMetricaWebSettings>("");

            if (settingsArray == null || settingsArray.Length == 0)
                return;

            var settings = settingsArray[0];

            if (settings.IsEnable == false)
            {
                htmlContent = BuildExtension.Remove(htmlContent, Name);
                File.WriteAllText(indexHtmlPath, htmlContent);
                return;
            }

            var sb = new StringBuilder(512);

            sb.Append(SDKCounter);
            sb.AppendLine();

            var counterId = settings.CounterId;

            if (string.IsNullOrEmpty(counterId) == false)
            {
                var counterParams = settings.GetJsonParameters();
                sb.Append("\t");
                sb.AppendFormat(InitializeCounter, counterId, counterParams);
                sb.AppendLine();
                sb.Append("\t");
                sb.AppendFormat(ImgSrc, counterId);
                sb.AppendLine();

                if (settings.SendPageOpenInInitialize)
                {
                    sb.Append("\t");
                    sb.Append(GetPageOpen(counterId));
                    sb.AppendLine();
                }
            }

            htmlContent = BuildExtension.Append(htmlContent, HtmlTag.Head, Name, sb.ToString());
            File.WriteAllText(indexHtmlPath, htmlContent);
        }

        private string GetPageOpen(string counterId) =>
            "<script type=\"text/javascript\" >"+
            $"ym({counterId},'reachGoal','page_opened');" +
            "addEventListener('DOMContentLoaded', (event) => {" +
            "const pageLoadTime = performance.timing.domContentLoadedEventStart - performance.timing.navigationStart;" +
            $"ym({counterId},'reachGoal','page_loaded', {{ pageLoadTime: pageLoadTime / 1000 }});" +
            "});" +
            "</script>";
    }
}