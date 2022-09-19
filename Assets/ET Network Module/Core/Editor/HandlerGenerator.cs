using ET;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public static class HandlerGenerator
{
    // 生成 Handler 保存的位置
    static string path = $"{Application.dataPath}/ET Network Module/Generated/Handlers";

    [MenuItem("Tools/生成非RPC消息处理器")]
    static void Generate()
    {
        var messages = typeof(IMessage).Assembly.GetTypes()
                  .Where(v => v.IsClass)
                  .Where(v => typeof(IMessage).IsAssignableFrom(v) && !typeof(IRequest).IsAssignableFrom(v) && !typeof(IResponse).IsAssignableFrom(v))
                  .ToList();
        if (messages.Count > 0)
        {
            TryCreateAssemblyDefinitionFile();
            count = 0;
            messages.ForEach(GenerateCode);
            Debug.Log($"{nameof(HandlerGenerator)}: {(count == 0 ? "Handler 无新增" : $"生成 Handler {count}个")}，操作完成！");
            if (count > 0)
            {
                AssetDatabase.Refresh();
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<DefaultAsset>(FileUtil.GetProjectRelativePath(path)));
            }
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
        var dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }
        return dirInfo;
    }
    /// <summary>
    /// 为降低反射遍历消息的次数、减小编译时长，故使用 AssemblyDefinition 
    /// </summary>
    private static void TryCreateAssemblyDefinitionFile()
    {
        string file = "com.network.handlers.asmdef";
        string content = @"{
    ""name"": ""com.network.handlers"",
    ""references"": [
        ""GUID:97baa7ef701375d4992b10159aec3da7"",
        ""GUID:33b949c5888978348a9a6fbe701b5022""
    ],
    ""autoReferenced"": true
}";
        var path = Path.Combine(GetSaveLocation().FullName,file);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, content, System.Text.Encoding.UTF8);
            Debug.Log($"{nameof(HandlerGenerator)}: Assembly Definition File 生成 {file} 成功！");
        }
    }
}
