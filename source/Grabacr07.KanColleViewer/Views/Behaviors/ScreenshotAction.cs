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
using IServiceProvider = Grabacr07.KanColleViewer.Win32.IServiceProvider;
using WebView = Microsoft.Toolkit.Win32.UI.Controls.WPF.WebView;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Grabacr07.KanColleViewer.Views.Behaviors
{
	/// <summary>
	/// 艦これの Flash 部分を画像として保存する機能を提供します。
	/// </summary>
	internal class ScreenshotAction : InteractionMessageAction<WebView>
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
		private void SaveCore(string path)
		{
			const string notFoundMessage = "칸코레 Flash를 찾을 수 없습니다.";

			var view = this.AssociatedObject;
			if (view == null)
			{
				throw new Exception(notFoundMessage);
			}

			TakeScreenshot(view, path);
		}

		private void TakeScreenshot(WebView elem, string path)
		{
			var size = elem.RenderSize;
			int width = (int)elem.Width;
			int height = (int)elem.Height;

			RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
				(int)size.Width,
				(int)size.Height,
				96, 96,
				PixelFormats.Pbgra32
			);

			elem.Measure(size); //Important
			elem.Arrange(new Rect(size)); //Important

			DrawingVisual visual = new DrawingVisual();
			using (DrawingContext context = visual.RenderOpen())
			{
				VisualBrush brush = new VisualBrush(elem);
				brush.Stretch = Stretch.Uniform;
				context.DrawRectangle(
					brush,
					null,
					new Rect(
						new System.Windows.Point(),
						new System.Windows.Size(elem.Width, elem.Height)
					)
				);
			}
			renderBitmap.Render(visual);

			using (MemoryStream ms = new MemoryStream())
			{
				BitmapEncoder encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
				encoder.Save(ms);

				using (var image = Bitmap.FromStream(ms))
				{
					var format = Path.GetExtension(path) == ".jpg"
						? ImageFormat.Jpeg
						: ImageFormat.Png;

					image.Save(path, format);
				}
			}
		}
	}
}
