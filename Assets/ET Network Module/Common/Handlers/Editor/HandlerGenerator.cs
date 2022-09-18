using ET;
using System;
using System.IO;
using System.Linq;
using UnityEditor;

public static class HandlerGenerator
{
    [MenuItem("Tools/生成非RPC消息处理器")]
    static void Generate()
    {
        var messages = typeof(IMessage).Assembly.GetTypes()
                  .Where(v => v.IsClass)
                  .Where(v => typeof(IMessage).IsAssignableFrom(v) && !typeof(IRequest).IsAssignableFrom(v) && !typeof(IResponse).IsAssignableFrom(v))
                  .ToList();
        if (messages.Count > 0)
        {
            messages.ForEach(GenerateCode);
            AssetDatabase.Refresh();
        }
    }

    static void GenerateCode(Type message)
    {
        var dirInfo = GetSaveLocation();
        var type = message.Name;
        var name = $"{type}Handler";
        var content = @$"namespace ET
{{
    [MessageHandler]
    public class {name} : AMHandler<{type}> {{}}
}}";
        var file = Path.Combine(dirInfo.FullName, $"{name}.cs");
        File.WriteAllText(file, content, System.Text.Encoding.UTF8);
    }
    private static DirectoryInfo GetSaveLocation()
    {
        var scripts = AssetDatabase.FindAssets($"t:Script {nameof(HandlerGenerator)}");
        var path = AssetDatabase.GUIDToAssetPath(scripts[0]);
        var fileinfo = new FileInfo(path);
        var dir = $"{fileinfo.Directory.FullName}/../Generated";
        var dirInfo = new DirectoryInfo(dir);
        if (!dirInfo.Exists)
        {
            dirInfo = Directory.CreateDirectory(dir);
        }
        return dirInfo;
    }
}
