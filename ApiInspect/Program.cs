using System;
using System.Linq;
using System.Reflection;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace ApiInspect
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Inspecting OpenCvSharp DNN API (assembly types)...\n");

			void DumpType(string name, Type? t)
			{
				Console.WriteLine($"=== {name} ===");
				if (t == null) { Console.WriteLine("(not found)\n"); return; }
				var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.OrderBy(m => m.Name)
					.Select(m => m.ToString()).Take(200);
				foreach (var m in methods) Console.WriteLine(m);
				Console.WriteLine();
			}

			var asm = typeof(Net).Assembly;
			DumpType("OpenCvSharp.Dnn.CvDnn", asm.GetType("OpenCvSharp.Dnn.CvDnn"));
			DumpType("OpenCvSharp.Dnn.DnnInvoke", asm.GetType("OpenCvSharp.Dnn.DnnInvoke"));
			DumpType("OpenCvSharp.Dnn.Net", asm.GetType("OpenCvSharp.Dnn.Net"));
			DumpType("OpenCvSharp.Net (top-level)", asm.GetType("OpenCvSharp.Net"));

			Console.WriteLine("Done.");
		}
	}
}
