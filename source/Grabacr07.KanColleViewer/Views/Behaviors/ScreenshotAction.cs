using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.ViewModels.Messages;
using Grabacr07.KanColleViewer.Win32;
using Livet.Behaviors.Messaging;
using Livet.Messaging;
using mshtml;
using SHDocVw;
using CefSharp;
using CefSharp.Wpf;
using IServiceProvider = Grabacr07.KanColleViewer.Win32.IServiceProvider;
//using WebBrowser = System.Windows.Controls.WebBrowser;

namespace Grabacr07.KanColleViewer.Views.Behaviors
{
	/// <summary>
	/// 艦これの Flash 部分を画像として保存する機能を提供します。
	/// </summary>
	internal class ScreenshotAction : InteractionMessageAction<ChromiumWebBrowserEx>
	{
		protected override void InvokeAction(InteractionMessage message)
		{
			var screenshotMessage = message as ScreenshotMessage;
			if (screenshotMessage == null)
			{
				return;
			}

			try
			{
				this.SaveCore(screenshotMessage.Path);
				screenshotMessage.Response = new Processing();
			}
			catch (Exception ex)
			{
				screenshotMessage.Response = new Processing(ex);
			}
		}


		/// <summary>
		/// <see cref="WebBrowser.Document"/> (<see cref="HTMLDocument"/>) から艦これの Flash 要素を特定し、指定したパスにスクリーンショットを保存します。
		/// </summary>
		/// <remarks>
		/// 本スクリーンショット機能は、「艦これ 司令部室」開発者の @haxe さんより多大なご協力を頂き実装できました。
		/// ありがとうございました。
		/// </remarks>
		/// <param name="path"></param>
		private async void SaveCore(string path)
		{
			const string notFoundMessage = "칸코레를 찾을 수 없습니다.";

			var browser = this.AssociatedObject.GetBrowser();
			if (browser == null) throw new Exception(notFoundMessage);

			var frame = browser.MainFrame;
			if (frame == null) throw new Exception(notFoundMessage);

			var frame_exists = (bool)(await frame.EvaluateScriptAsync("document.getElementById('game_frame')!=null")).Result;
			if (!frame_exists) throw new Exception(notFoundMessage);

			var width = (int)(await frame.EvaluateScriptAsync("document.getElementById('game_frame').clientWidth")).Result;
			var height = (int)(await frame.EvaluateScriptAsync("document.getElementById('game_frame').clientHeight")).Result;

			TakeScreenshot((int)width, (int)height, this.AssociatedObject, path);
		}

		private static void TakeScreenshot(int width, int height, ChromiumWebBrowserEx browser, string path)
		{
			var image = browser.ScreenshotOrNull();

			var format = Path.GetExtension(path) == ".jpg"
				? ImageFormat.Jpeg 
				: ImageFormat.Png;

			image.Save(path, format);
		}
	}
}
