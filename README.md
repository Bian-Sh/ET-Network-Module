# 是什么？

> ET-Network-Module

一个从 ET 6.0 中梳理出来的多次重构了的，使用  asmdef 模块化了的网络模块

# 为什么？

* 方便在不需要接入 ET 前端的情况下与 ET 后台对接。
* 方便在不喜仰或是不习惯 ET 前端的情况下与 ET 后台对接。
* 方便用户与自己喜欢的任意框架缝合
* 在无 MVC(S) 、无热重载需求的；只期望简单的、按原有直觉开发的情景下有用
* 学习目的：学习大型游戏网络框架，学习重构网络框架

# 有什么？

* 一个仅供测试的、原滋原味的 ET6.0 server 
* 保留 Google Protobuf net + TCP + RPC/非 RPC 等特性，并与 ET Server 正常通信
* 使用 Unity Assembly Definition File 拆分的网络模块，详见下图：

![](doc/arc.png)

* 简化了的、适配 MonoBehaviour 的非 RPC 消息处理器生成、订阅工作流
* 提供了 .proto 转 .cs 的一键生成工具

# 没有什么？

* 没有了 ET Entity 的概念
* 没有 ECS 各种跳跃式的分工程开发模式，回到了常规的 Unity 开发

# 如何安装？

1. Clone 本项目，将文件夹 **ET Network Module** 放置到自己的工程
2. 如果之前有脚本调用 OuterMessage.cs 将该脚本所在文件夹改名，加上一个波浪号（方便网络模块完成编译）
3. 删除  **ET Network Module/Generated** 文件夹，删除示例消息和消息处理器
4. 与后台约定，将 outermessage.proto 中 ping 消息体置顶 （* 重要 *）
5. 使用 **Tools/.proto 转 .cs 实体类** 先生成网络消息类

![](doc/proto2cs.png)

6. 紧接着使用 **Tools/生成非 RPC 消息处理器** 生成非 RPC 网络消息处理器类型

![](doc/handlerGenerator.png)

7. 将第 2 步中修改改回来即完成安装。

PS:生成的实体类与 handler 同位于 Generated 文件夹,用一个单独的程序集管理，减小编译时长，Generated 文件夹随时可一键更新.

# 如何使用？

## 非 RPC 消息处理器的使用

我们简化了事件的监听与取消订阅，使用如下方式即可获取关心的网络消息：

使用：``MessageHandler.ListenSignal`` 监听关心的非 RPC 网络消息

使用：``MessageHandler.RemoveSignal`` 删除对指定网络消息的监听

使用：`` using static MessageHandler；`` 语法糖像原生方法那样监听网络消息

```
using ET;
using UnityEngine;
using static MessageHandler;
public class HandlerUsageCase : MonoBehaviour
{
    private void Start()
    {
        ListenSignal<M2C_CreateMyUnit>(OnMyUnitCreated);
    }
    private void OnMyUnitCreated(Session arg1, M2C_CreateMyUnit arg2)
    {
        // 撰写你自己的逻辑
    }
    private void OnDestroy()
    {
        RemoveSignal<M2C_CreateMyUnit>(OnMyUnitCreated);
    }
}
```

## RPC 消息的使用

原滋原味的 RPC 使用风格，下面摘抄部分登录逻辑够熟悉不？

```
 Session forgate = NetKcpComponent.Create(NetworkHelper.ToIPEndPoint(address));
 R2C_Login r2CLogin = (R2C_Login)await forgate.Call(new C2R_Login() { Account=username.text, Password=password.text });
 forgate?.Dispose();
```

# 一些动画演示

> 动图演示的是消息类以及非 rpc 消息处理器代码生成

![](doc/generatecode.gif)

> 演示连接 Gate 服务器、登录服务器、心跳（Ping）、进入map

![](doc/demo.gif)
