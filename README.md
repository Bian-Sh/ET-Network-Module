# ET-Network-Module

从 ET 中梳理出来的可以与 ET Server 通信的网络模块（仅 TCP 协议，可自行扩展）

# 演示连接 Gate 服务器、登录服务器、心跳

![](doc/demo.gif)

图示为网络消息生成工具，可以选中 .proto 文件生成 .cs

![](doc/proto2cs.png)



动图演示的是消息类以及非 rpc 消息处理器代码生成

![](doc/generatecode.gif)



本项目特色：

1. 基于 ET 6.0 ,故而可以与 ET6.0 Server 正常通信

2. 对 网络模块 进行了重构，模块与模块拆分的非常细致，在没有投入使用时，Generated 文件夹中的所有类型甚至可以完全删除而不报错。

3. 为方便接入非 ET ECS 架构的常规 unity 项目，按以往直觉开发故而剔除了 网络模块对 ET.Entity 的依赖

4. 改善了 非 RPC 网络消息的订阅与取消订阅，如下所示即可在继承了 MonoBehaviour 的类型中监听来自 ET 框架的非 RPC 网络消息
   
   ```
   using ET;
   using UnityEngine;
   using static MessageHandlerCenter;
   public class HandlerUsageCase : MonoBehaviour
   {
       private void Start()
       {
           ListenSignal<M2C_CreateMyUnit>(OnMyUnitCreated);
           ListenSignal<M2C_RemoveUnits>(OnMyUnitRemoved);
       }
       private void OnMyUnitRemoved(Session arg1, M2C_RemoveUnits arg2)
       {
           // 撰写你自己的逻辑
       }
       private void OnMyUnitCreated(Session arg1, M2C_CreateMyUnit arg2)
       {
           // 撰写你自己的逻辑
       }
       private void OnDestroy()
       {
           RemoveSignal<M2C_CreateMyUnit>(OnMyUnitCreated);
           RemoveSignal<M2C_RemoveUnits>(OnMyUnitRemoved);
       }
   }
   ```
   
   

5. 提供自动生成 非 RPC 网络消息处理器的编辑器工具

6. 提供根据 .proto 生成 .cs 消息实体类的工具，前后端完全可以通过 .proto 文件对协议而不需要交换 .cs 文件。
