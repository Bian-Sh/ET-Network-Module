using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ET
{
    internal class OpcodeInfo
    {
        public string Name;
        public int Opcode;
    }

    public class Proto2CSHandler : EditorWindow
    {
        static string MessagePath;
        static string ProtoPath;
        static string ProtoPathKey = $"{nameof(Proto2CSHandler)}-ProtoPath-Key";
        DefaultAsset asset;
        GUIContent initBt_cnt = new GUIContent("请选择 proto 文件", "请选择用于生成 .cs 实体类的 proto 文件");
        GUIContent updateBt_cnt = new GUIContent("更新", "选择新的 proto 文件，如果此文件在工程外，将会复制到工程内，覆盖原有的 proto 文件");
        GUIContent tips = new GUIContent("操作完成，请等待编译...");
        string notice = @"1. 选择的 .proto 文件不在工程中则拷贝至工程中
2. 拷贝的副本只存在一份，永远执行覆盖操作
3. 选择的 .proto 文件位于工程中则不做上述处理
4. 内置的 Ping  消息不再生成，但占用 Opcode: 10002、10003 
5. 约定： .proto 文件中 Ping 消息结构体必须置顶";
        static EditorWindow window;
        [MenuItem("Tools/.proto 转 .cs 实体类")]
        public static void ShowWindow()
        {
            window = GetWindow(typeof(Proto2CSHandler));
        }
        public void OnEnable()
        {
            MessagePath = $"{Application.dataPath}/ET Network Module/Generated/Message";
            if (!Directory.Exists(MessagePath))
            {
                Directory.CreateDirectory(MessagePath);
            }
            ProtoPath = EditorPrefs.GetString(ProtoPathKey);
            if (!string.IsNullOrEmpty(ProtoPath))
            {
                asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(ProtoPath);
            }
            minSize = new Vector2(360, 220);
        }

        private void OnGUI()
        {
            if (!asset)
            {
                //获取当前 editorwindow 宽高
                var rect = EditorGUILayout.GetControlRect();
                rect.height = 48;
                rect.width = 200;
                rect.x = (position.width - rect.width) / 2;
                rect.y = (position.height - rect.height) / 2;
                if (GUI.Button(rect, initBt_cnt))
                {
                    SelectAndLoadProtoFile();
                }
                return;
            }
            GUILayout.Space(15);
            using (new GUILayout.HorizontalScope())
            {
                asset = EditorGUILayout.ObjectField("Proto 文件：", asset, typeof(DefaultAsset), false) as DefaultAsset;
                if (GUILayout.Button(updateBt_cnt, GUILayout.Width(60)))
                {
                    SelectAndLoadProtoFile();
                }
            }

            GUI.enabled = false;
            var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(FileUtil.GetProjectRelativePath(MessagePath));
            EditorGUILayout.ObjectField("消息存储路径：", folder, typeof(DefaultAsset), false);
            GUI.enabled = true;

            GUILayout.Space(15);
            EditorGUILayout.HelpBox(notice, MessageType.Info);
            GUILayout.Space(15);
            var rt = GUILayoutUtility.GetLastRect();
            rt.width = 200;
            rt.height = 48;
            rt.x = (position.width - rt.width) / 2;
            rt.y= position.height - rt.height-10;
            if (GUI.Button(rt,"生成 .cs 实体类"))
            {
                TryCreateAssemblyDefinitionFile();
                InnerProto2CS.Proto2CS(asset, MessagePath);
                ShowNotification(tips);
                AssetDatabase.Refresh();
            }
            // 检测ObjectField是否有修改
            if (GUI.changed)
            {
                ProtoPath = asset ? AssetDatabase.GetAssetPath(asset) : string.Empty;
                EditorPrefs.SetString(ProtoPathKey, ProtoPath);
            }
        }

        private void SelectAndLoadProtoFile()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("请选择 .proto 文件", Application.dataPath, new string[] { "Protobuf file", "proto" });
            if (!string.IsNullOrEmpty(path))
            {
                ProtoPath = FileUtil.GetProjectRelativePath(path);
                if (string.IsNullOrEmpty(ProtoPath)) //.proto 文件不在工程内，则拷贝到工程中,且覆盖原有的 proto 文件
                {
                    var fileName = Path.GetFileName(path);
                    var destPath = $"{MessagePath}/{fileName}";
                    File.Copy(path, destPath, true); 
                    ProtoPath = FileUtil.GetProjectRelativePath(destPath);
                    AssetDatabase.Refresh();
                }
                EditorPrefs.SetString(ProtoPathKey, ProtoPath);
                asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(ProtoPath);
            }
        }
        /// <summary>
        /// 为降低反射遍历消息的次数、减小编译时长，故使用 AssemblyDefinition 
        /// </summary>
        private static void TryCreateAssemblyDefinitionFile()
        {
            string file = "com.network.generated.asmdef";
            string content = @"{
    ""name"": ""com.network.generated"",
    ""references"": [
        ""GUID:97baa7ef701375d4992b10159aec3da7"",
        ""GUID:348e1548f7bc88348b10043acbbf70df""
    ],
    ""autoReferenced"": true
}";
            var path = Path.Combine(MessagePath, "../", file);
            if (!File.Exists(path))
            {
                File.WriteAllText(path, content, System.Text.Encoding.UTF8);
                Debug.Log($"{nameof(HandlerGenerator)}: Assembly Definition File 生成 {file} 成功！");
            }
        }
    }

    public static class InnerProto2CS
    {
        private static readonly char[] splitChars = { ' ', '\t' };
        private static readonly List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();

        public static void Proto2CS(DefaultAsset proto, string messagePath)
        {
            msgOpcode.Clear();
            var startOpcode = OpcodeRangeDefine.OuterMinOpcode + 2; // 保留2个 opcode  给 Ping
            Proto2CS("ET", proto, messagePath, "OuterOpcode", startOpcode);
            GenerateOpcode("ET", "OuterOpcode", messagePath);
        }

        public static void Proto2CS(string ns, DefaultAsset proto, string outputPath, string opcodeClassName, int startOpcode)
        {
            msgOpcode.Clear();
            string csPath = Path.Combine(outputPath, $"{proto.name}.cs");
            var protoPath = AssetDatabase.GetAssetPath(proto);
            var protoContent = File.ReadAllText(protoPath);
            StringBuilder sb = new StringBuilder();
            sb.Append("using ET;\n");
            sb.Append("using ProtoBuf;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append($"namespace {ns}\n");
            sb.Append("{\n");

            bool isMsgStart = false;
            foreach (string line in protoContent.Split('\n'))
            {
                string newline = line.Trim();

                if (newline == "")
                {
                    continue;
                }

                if (newline.StartsWith("//ResponseType")&&!newline.EndsWith("G2C_Ping")) //后半个判断过滤 Ping 消息举措
                {
                    string responseType = line.Split(' ')[1].TrimEnd('\r', '\n');
                    sb.AppendLine($"\t[ResponseType(nameof({responseType}))]");
                    continue;
                }

                if (newline.StartsWith("//") && !newline.EndsWith("G2C_Ping"))
                {
                    sb.Append($"{newline}\n");
                    continue;
                }

                if (newline.StartsWith("message")&&!newline.Contains("C2G_Ping")&&!newline.Contains("G2C_Ping"))
                {
                    string parentClass = "";
                    isMsgStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }

                    msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode });

                    sb.Append($"\t[Message({opcodeClassName}.{msgName})]\n");
                    sb.Append($"\t[ProtoContract]\n");
                    sb.Append($"\tpublic partial class {msgName}: Object");
                    if (parentClass == "IActorMessage" || parentClass == "IActorRequest" || parentClass == "IActorResponse")
                    {
                        sb.Append($", {parentClass}\n");
                    }
                    else if (parentClass != "")
                    {
                        sb.Append($", {parentClass}\n");
                    }
                    else
                    {
                        sb.Append("\n");
                    }

                    continue;
                }

                if (isMsgStart)
                {
                    if (newline == "{")
                    {
                        sb.Append("\t{\n");
                        continue;
                    }

                    if (newline == "}")
                    {
                        isMsgStart = false;
                        sb.Append("\t}\n\n");
                        continue;
                    }

                    if (newline.Trim().StartsWith("//"))
                    {
                        sb.AppendLine(newline);
                        continue;
                    }

                    if (newline.Trim() != "" && newline != "}")
                    {
                        if (newline.StartsWith("repeated"))
                        {
                            Repeated(sb, ns, newline);
                        }
                        else
                        {
                            Members(sb, newline, true);
                        }
                    }
                }
            }

            sb.Append("}\n");
            using FileStream txt = new FileStream(csPath, FileMode.Create, FileAccess.ReadWrite);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }

        private static void GenerateOpcode(string ns, string outputFileName, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic static partial class {outputFileName}");
            sb.AppendLine("\t{");
            foreach (OpcodeInfo info in msgOpcode)
            {
                sb.AppendLine($"\t\t public const ushort {info.Name} = {info.Opcode};");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string csPath = Path.Combine(outputPath, outputFileName + ".cs");

            using FileStream txt = new FileStream(csPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }

        private static void Repeated(StringBuilder sb, string ns, string newline)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                type = ConvertType(type);
                string name = ss[2];
                int n = int.Parse(ss[4]);

                sb.Append($"\t\t[ProtoMember({n})]\n");
                sb.Append($"\t\tpublic List<{type}> {name} = new List<{type}>();\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }

        private static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "int16":
                    typeCs = "short";
                    break;
                case "int32":
                    typeCs = "int";
                    break;
                case "bytes":
                    typeCs = "byte[]";
                    break;
                case "uint32":
                    typeCs = "uint";
                    break;
                case "long":
                    typeCs = "long";
                    break;
                case "int64":
                    typeCs = "long";
                    break;
                case "uint64":
                    typeCs = "ulong";
                    break;
                case "uint16":
                    typeCs = "ushort";
                    break;
                default:
                    typeCs = type;
                    break;
            }

            return typeCs;
        }

        private static void Members(StringBuilder sb, string newline, bool isRequired)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[0];
                string name = ss[1];
                int n = int.Parse(ss[3]);
                string typeCs = ConvertType(type);

                sb.Append($"\t\t[ProtoMember({n})]\n");
                sb.Append($"\t\tpublic {typeCs} {name} {{ get; set; }}\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }
    }
}