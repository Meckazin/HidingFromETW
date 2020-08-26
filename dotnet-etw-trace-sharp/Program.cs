using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;

namespace dotnet_etw_trace_sharp
{
	class ProcessInfo
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
	class Program
    {
		static void Main(string[] args)
		{

			using (var session = new TraceEventSession(Environment.OSVersion.Version.Build >= 9200 ? "MyKernelSession" : KernelTraceEventParser.KernelSessionName))
			{
				session.EnableProvider(
					ClrTraceEventParser.ProviderGuid,
					TraceEventLevel.Verbose,
					(ulong)(ClrTraceEventParser.Keywords.Loader | ClrTraceEventParser.Keywords.Jit)
					);

				var parser = session.Source.Clr;
				parser.LoaderAssemblyLoad += e => {
                    //if (e.FullyQualifiedAssemblyName.Contains("Seatbelt"))
					if(true)
                    {
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"{e.TimeStamp}.{e.TimeStamp.Millisecond:D3}: Process: {e.ProcessID} " + "\t" + $" AssemblyName: {e.FullyQualifiedAssemblyName}");
					}
				};

				parser.MethodLoadVerbose += e => {
                    //if (e.MethodNamespace.Contains("Seatbelt"))
					if(true)
                    {
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine($"{e.TimeStamp}.{e.TimeStamp.Millisecond:D3}: NameSpace: {e.MethodNamespace} " + "\t" + $" MethodName: {e.MethodName} " + "\t" + $" Signature: {e.MethodSignature} ");
					}
				};

				bool run = true;
				while (run) {
					session.Source.Process();
				}
				Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
					e.Cancel = true;
					run = false;
				};

				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("[+] Exit");
				return;
			}
		}
	}
}
