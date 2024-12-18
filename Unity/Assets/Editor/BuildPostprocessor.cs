using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class BuildPostProcessor
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            PBXProject project = new PBXProject();
            string sPath = PBXProject.GetPBXProjectPath(path);
            project.ReadFromFile(sPath);


            // Add framework to app target
            string appTarget = project.GetUnityMainTargetGuid();
            CopyAndReplaceDirectory(UnityEngine.Application.dataPath + "/Plugins/iOS/QUICClient.framework", Path.Combine(path, "Frameworks/QUICClient.framework"));
            string fileGuid = project.AddFile("Frameworks/QUICClient.framework", "Frameworks/QUICClient.framework", PBXSourceTree.Source);
            project.AddFileToBuild(appTarget, fileGuid);
            project.SetBuildProperty(appTarget, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(appTarget, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks/**");
            UnityEditor.iOS.Xcode.Extensions.PBXProjectExtensions.AddFileToEmbedFrameworks(project, appTarget, fileGuid);

            project.AddFrameworkToProject(appTarget, "Network.framework", false);
            project.AddFrameworkToProject(appTarget, "Foundation.framework", false);

            project.SetBuildProperty(appTarget, "ENABLE_BITCODE", "false");


            // Remove framework from UnityFramework target
            string unityFwTarget = project.TargetGuidByName("UnityFramework");
            string oldFileGuid = project.FindFileGuidByProjectPath("Frameworks/Plugins/iOS/QUICClient.framework");
            project.RemoveFileFromBuild(unityFwTarget, oldFileGuid);
            project.RemoveFile(oldFileGuid);


            File.WriteAllText(sPath, project.WriteToString());
        }
    }

    internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
    {
        if (Directory.Exists(dstPath))
            Directory.Delete(dstPath);
        if (File.Exists(dstPath))
            File.Delete(dstPath);

        Directory.CreateDirectory(dstPath);

        foreach (var file in Directory.GetFiles(srcPath))
            File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));

        foreach (var dir in Directory.GetDirectories(srcPath))
            CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
    }
}