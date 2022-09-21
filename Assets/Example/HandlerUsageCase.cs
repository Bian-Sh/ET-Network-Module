using ET;
using UnityEngine;
using static MessageHandlerCenter;
public class HandlerUsageCase : MonoBehaviour
{
    private void Start()
    {
        ListenSignal<M2C_CreateMyUnit>(OnMyUnitCreated);
        ListenSignal<M2C_CreateUnits>(OnUnitsCreated);
    }

    private void OnUnitsCreated(Session arg1, M2C_CreateUnits arg2)
    {
        // 撰写你自己的逻辑
        foreach (var item in arg2.Units)
        {
            Debug.LogWarning($"服务器请求初始化敌人 {item.UnitId}，位置为：x = {item.X},y = {item.Y} ");
        }
    }
    private void OnMyUnitCreated(Session arg1, M2C_CreateMyUnit arg2)
    {
        // 撰写你自己的逻辑
        Debug.LogWarning($"服务器请求 {arg1.Id} 初始化玩家，位置为：x = {arg2.Unit.X},y = {arg2.Unit.Y} ");
    }
    private void OnDestroy()
    {
        RemoveSignal<M2C_CreateMyUnit>(OnMyUnitCreated);
        RemoveSignal<M2C_CreateUnits>(OnUnitsCreated);
    }
}
