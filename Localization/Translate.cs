using System.Threading.Tasks;
using UnityEngine.Networking;

namespace GameSDK.Localization
{
	public static class Translate
	{
		public static async Task<string> Process(string targetLang, string sourceText)
		{
			string sourceLang = "auto";
			string url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl="
			             + sourceLang + "&tl=" + targetLang + "&dt=t&q=" + UnityWebRequest.EscapeURL(sourceText);

			UnityWebRequest request = UnityWebRequest.Get(url);

			request.SendWebRequest();

			while (request.isDone == false)
			{
				await Task.Yield();
			}

			if (!request.isDone) return string.Empty;
			if (!string.IsNullOrEmpty(request.error)) return string.Empty;
			string translate = request.downloadHandler.text.Split(',')[0].Replace("[","").Replace("\"", "");
			return translate;
		}

		public static async Task<string> Process(string sourceLang, string targetLang, string sourceText)
		{
			string url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl="
			             + sourceLang + "&tl=" + targetLang + "&dt=t&q=" + UnityWebRequest.EscapeURL(sourceText);

			UnityWebRequest request = UnityWebRequest.Get(url);

			request.SendWebRequest();

			while (request.isDone == false)
			{
				await Task.Yield();
			}

			if (!request.isDone) return string.Empty;
			if (!string.IsNullOrEmpty(request.error)) return string.Empty;
			string translate = request.downloadHandler.text.Split(',')[0].Replace("[", "").Replace("\"", "");
			return translate;
		}
	}
}