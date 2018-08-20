using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Grabacr07.KanColleViewer.Models.Migration
{
	public class cef_Migration
	{
		public static void Migration()
		{
			var deletelist = new string[]
			{
				"lib\\cef.pak",
				"lib\\CefSharp.BrowserSubprocess.Core.dll",
				"lib\\CefSharp.BrowserSubprocess.Core.pdb",
				"lib\\CefSharp.BrowserSubprocess.exe",
				"lib\\CefSharp.BrowserSubprocess.pdb",
				"lib\\CefSharp.Core.dll",
				"lib\\CefSharp.Core.pdb",
				"lib\\CefSharp.Core.xml",
				"lib\\CefSharp.dll",
				"lib\\CefSharp.pdb",
				"lib\\CefSharp.Wpf.dll",
				"lib\\CefSharp.Wpf.pdb",
				"lib\\CefSharp.Wpf.XML",
				"lib\\CefSharp.XML",
				"lib\\cef_100_percent.pak",
				"lib\\cef_200_percent.pak",
				"lib\\cef_extensions.pak",
				"lib\\chrome_elf.dll",
				"lib\\d3dcompiler_47.dll",
				"lib\\devtools_resources.pak",
				"lib\\icudtl.dat",
				"lib\\libcef.dll",
				"lib\\libEGL.dll",
				"lib\\libGLESv2.dll",
				"lib\\natives_blob.bin",
				"lib\\snapshot_blob.bin",
				"lib\\v8_context_snapshot.bin",
				"lib\\widevinecdmadapter.dll",

				"lib\\locales",
				"lib\\swiftshader",

				"BrowserCache",
			};
			foreach(var item in deletelist)
				DeleteTarget(item);
		}

		private static void DeleteTarget(string filename)
		{
			var path = Path.Combine(
				Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
				filename
			);

			try
			{
				if (File.Exists(path) || Directory.Exists(path))
				{
					var attr = File.GetAttributes(path);
					if (attr.HasFlag(FileAttributes.Directory))
						Directory.Delete(path, true);
					else
						File.Delete(path);
				}
			}
			catch { }
		}
	}
}
