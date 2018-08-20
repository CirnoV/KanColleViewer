using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.ViewModels.Messages;
using Grabacr07.KanColleViewer.Win32;
using Grabacr07.KanColleViewer.Models;
using Livet.Behaviors.Messaging;
using Livet.Messaging;
using mshtml;
using SHDocVw;
using IServiceProvider = Grabacr07.KanColleViewer.Win32.IServiceProvider;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace Grabacr07.KanColleViewer.Views.Behaviors
{
	/// <summary>
	/// 艦これの Flash 部分を画像として保存する機能を提供します。
	/// </summary>
	internal class ScreenshotAction : InteractionMessageAction<WebBrowser>
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
				this.SaveCore(screenshotMessage.Path, screenshotMessage.Format);
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
		private void SaveCore(string path, SupportedImageFormat format)
		{
			const string notFoundMessage = "칸코레 Canvas를 찾을 수 없습니다.";

			var browser = Helper.GetGameFrame(this.AssociatedObject);
			if (browser == null) throw new Exception(notFoundMessage);

			var document = browser?.Document as HTMLDocument;
			if (document == null) throw new Exception(notFoundMessage);

			var mimetype = format.ToMimeType();

			var rawResult = document.parentWindow.execScript($"takeScreenshot('{mimetype}');", "JavaScript");

			var communicator = document.getElementById("communicator");
			if (communicator == null) throw new Exception("커뮤니케이터를 찾는데 실패했습니다.");

			var dataUrl = communicator.innerHTML;
			communicator.innerHTML = "";

			var array = dataUrl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 2) throw new Exception($"無効な形式: {dataUrl}");

			var base64 = array[1];
			var bytes = Convert.FromBase64String(base64);
			using (var ms = new MemoryStream(bytes))
			{
				var image = System.Drawing.Image.FromStream(ms);
				image.Save(path, ImageFormat.Png);
			}
		}
	}
}
