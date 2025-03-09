using System;
using System.IO;
using System.Reflection;

namespace PMGInjector
{
    public class Entrypoint
    {
        public static void OnLoad()
        {
            // load all library DLLs
            foreach (string dll in Directory.GetFiles(Path.Combine(ModAPI.Metadata.MetaLocation, "Assets", "DLLs"), "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(dll));
                    Console.WriteLine($"Loaded: {assembly.FullName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load {dll}: {ex.Message}");
                }
            }
            // load main DLL
            Assembly.Load(File.ReadAllBytes(Path.Combine(ModAPI.Metadata.MetaLocation, "PMG.Internal.dll"))).GetType("ProjectMultiground.Entrypoint").GetMethod("OnLoad")?.Invoke(null, new object[] { ModAPI.Metadata.MetaLocation });
        }
    }
}