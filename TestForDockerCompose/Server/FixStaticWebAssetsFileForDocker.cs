using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace TestForDockerCompose.Server;

//in docker fast build mode it makes a staticwebassets.runtime.json that contains path from the HOST, we need to convert that to the docker
partial class FixStaticWebAssetsFileForDocker
{
    [GeneratedRegex("(?<basepath>[a-zA-Z]:\\\\.+?\\\\)(?:Server|Client)\\\\", RegexOptions.IgnoreCase)]
    private static partial Regex Basepath();
    [GeneratedRegex("(?<basepath>[a-zA-Z]:\\\\.+?\\\\)\\.nuget\\\\", RegexOptions.IgnoreCase)]
    private static partial Regex NugetBasePath();

    public static void FixIt()
    {
        //https://stackoverflow.com/questions/59228315/debug-blazor-wasm-using-visual-studio-container-tools
        //enumerate all of the "*.staticwebassets.runtime.json" in the folder of the current assembly
        var directoryOfAssembly = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        foreach (var file in Directory.EnumerateFiles(directoryOfAssembly, "*.staticwebassets.runtime.json"))
        {
            var json = File.ReadAllText(file);
            //detect the basepath
            var group = Basepath().Match(json).Groups["basepath"];
            //means that visual studio compiled it correctly, or we previously replaced it properly
            if (!group.Success) return;
            var basePath = group.Value;
            Console.WriteLine($"Detected basepath {basePath}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) FixBasePath(ref json, basePath, "Server\\\\", "/app/");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) FixBasePath(ref json, basePath, "Server\\\\", "c:\\\\inetpub\\\\wwwroot\\\\");

            //to match the mapping in the docker compose file to compensate for docker SDK not supporting blazor apps
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) FixBasePath(ref json, basePath, "Client\\\\", "/app/Client/");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) FixBasePath(ref json, basePath, "Client\\\\", "c:\\\\Client\\\\");

            // //to match the mapping in the docker compose file to compensate for docker SDK not supporting blazor apps
            // if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) FixBasePath(ref json, basePath, "Client2\\\\", "/Client2/");
            // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) FixBasePath(ref json, basePath, "Client2\\\\", "c:\\\\Client2\\\\");

            var nugetbasePath = NugetBasePath().Match(json).Groups["basepath"].Value ?? throw new NullReferenceException("nugetbasePath");
            Console.WriteLine($"Detected nugetbasePath {nugetbasePath}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) FixBasePath(ref json, nugetbasePath, "", "/root/");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) FixBasePath(ref json, nugetbasePath, "", "c:\\\\.nuget\\\\");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) json = json.Replace("\\\\", "/");

            File.WriteAllText(file, json);
            static void FixBasePath(ref string json, string basePath, string extra, string newbasePath) => 
                json = json.Replace(basePath + extra, newbasePath);
        }
    }
}