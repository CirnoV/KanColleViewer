using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using CefSharp.Enums;
using CefSharp.Internals;
using CefSharp.Structs;
using CefSharp.Wpf.Rendering;

using Size = System.Drawing.Size;
using Point = System.Drawing.Point;
using System.IO.MemoryMappedFiles;

namespace CefSharp.Wpf
{
	public class ChromiumWebBrowserEx : ChromiumWebBrowser
	{
		[DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
		private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

		public ChromiumWebBrowserEx() : base()
		{
		}

		public Bitmap ScreenshotOrNull()
		{
			if (RenderHandler == null)
			{
				throw new NullReferenceException("RenderHandler cannot be null");
			}

			var renderHandler = RenderHandler as InteropBitmapRenderHandler;

			if (renderHandler == null)
			{
				throw new Exception("RenderHandler cannot be customized");
			}

			var accessor = renderHandler.GetType()
				.GetField("viewMemoryMappedViewAccessor", BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(RenderHandler) as MemoryMappedViewAccessor;
			if(accessor == null)
			{
				throw new Exception("MemoryMappedViewAccessor of RenderHandler not initialized");
			}

			var bitmap = new Bitmap((int)this.Width, (int)this.Height, PixelFormat.Format32bppArgb);
			var bd = bitmap.LockBits(
				new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				ImageLockMode.ReadWrite,
				PixelFormat.Format32bppArgb
			);
			CopyMemory(bd.Scan0, accessor.SafeMemoryMappedViewHandle.DangerousGetHandle(), (uint)(bd.Width * bd.Height * 4));
			bitmap.UnlockBits(bd);

			return bitmap;
		}
	}
}
