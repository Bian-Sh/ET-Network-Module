using ET;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
            count = 0;
            messages.ForEach(GenerateCode);
            Debug.Log($"{nameof(HandlerGenerator)}: 生成 Handler {count} 个，操作完成！");
            AssetDatabase.Refresh();
        }
    }
    static int count;
    static void GenerateCode(Type message)
    {
        var dirInfo = GetSaveLocation();
        var type = message.Name;
        var name = $"{type}Handler";
        var file = Path.Combine(dirInfo.FullName, $"{name}.cs");
        if (!File.Exists(file))
        {
            var content = @$"namespace ET
{{
    [MessageHandler]
    public class {name} : AMHandler<{type}> {{}}
}}";
            File.WriteAllText(file, content, System.Text.Encoding.UTF8);
            Debug.Log($"{nameof(HandlerGenerator)}: 生成 {name} 成功！");
            count++;
        }
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
