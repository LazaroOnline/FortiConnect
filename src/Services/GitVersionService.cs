using System.Reflection;

namespace FortiConnect.Services;

public class GitVersionService
{
	public static GitVersionAssemblyInfo GetGitVersionAssemblyInfo()
	{
		//var mainAssembly = typeof(DaiusService.Controllers.StoreController).Assembly;
		var mainAssembly = Assembly.GetExecutingAssembly();
		return GetGitVersionAssemblyInfo(mainAssembly);
	}

	public static GitVersionAssemblyInfo GetGitVersionAssemblyInfo(Assembly assembly)
	{
		var assemblyFileName = System.IO.Path.GetFileName(assembly.Location);
		var creationDate     = System.IO.File.GetCreationTime(assembly.Location);
		var modificationDate = System.IO.File.GetLastWriteTime(assembly.Location);
		var version = assembly.GetName()?.Version?.ToString();
		var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location)?.FileVersion;
		var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

		var versionInfo = new GitVersionAssemblyInfo {
			AssemblyFileName = assemblyFileName,
			CreationDate = creationDate,
			ModificationDate = modificationDate,
			Version = version,
			FileVersion = fileVersion,
			InformationalVersion = informationalVersion,
		};
		// return JsonConvert.SerializeObject(versionInfo);
		return versionInfo;
	}
}
