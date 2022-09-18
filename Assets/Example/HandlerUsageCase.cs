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
        // ׫д���Լ����߼�
    }

    private void OnMyUnitCreated(Session arg1, M2C_CreateMyUnit arg2)
    {
        // ׫д���Լ����߼�
    }
    private void OnDestroy()
    {
        RemoveSignal<M2C_CreateMyUnit>(OnMyUnitCreated);
        RemoveSignal<M2C_RemoveUnits>(OnMyUnitRemoved);
    }
}
