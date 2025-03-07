using System;

namespace ProjectMultiground
{
    public class Entrypoint
    {
        public static void Main()
        {
            // load all DLLs
            foreach (string dll in Directory.GetFiles(Path.Combine(ModAPI.Metadata.MetaLocation, "Assets", "DLLs"), "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(dll));
                    Console.WriteLine($"Loaded: {assembly.FullName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load {dllPath}: {ex.Message}");
                }
            }
            Assembly.Load(File.ReadAllBytes(Path.Combine(ModAPI.Metadata.MetaLocation, "Assets", "PMG.Internal.dll"))).GetType("ProjectMultiground.Entrypoint").GetMethod("OnLoad").Invoke(null, ModAPI);
        }
    }
}
