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
