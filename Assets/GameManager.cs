using ET;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Button button;
    public string address = "127.0.0.1";
    public const int port = 10002;
    TextMeshProUGUI text;
    NetKcpComponent NetKcpComponent;
    Session session;

    private void Awake()
    {
        text = button.GetComponentInChildren<TextMeshProUGUI>();
        NetKcpComponent = GetComponent<NetKcpComponent>();
    }
    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
        text.text = "Connect";
    }

    private void Update() => TimeInfo.Instance.Update();

    bool isConnected => session != null && !session.IsDisposed;
    private async void OnButtonClick()
    {
        if (isConnected)
        {
            text.text = "Connect";
            session.Dispose();
            session = null;
        }
        else
        {
            var host = $"{address}:{port}";
            var result = await LoginAsync(host, 1);
            text.text = result ? "Connected" : "Try again";
        }
    }

    public async ETTask<bool> LoginAsync(string address, int role)
    {
        bool isconnected = true;
        try
        {
            // 创建一个ETModel层的Session
            R2C_Login r2CLogin;
            Session forgate = null;
            forgate = NetKcpComponent.Create(NetworkHelper.ToIPEndPoint(address));
            r2CLogin = (R2C_Login)await forgate.Call(new C2R_Login() );
            forgate?.Dispose();
            // 创建一个gate Session,并且保存到SessionComponent中
            session = NetKcpComponent.Create(NetworkHelper.ToIPEndPoint(r2CLogin.Address));
            session.ping = new ET.Ping(session);
            G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await session.Call(new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId });
            Debug.Log("登陆gate成功!");
        }
        catch (Exception e)
        {
            isconnected = false;
            Debug.LogError($"登陆失败 - {e}");
        }
        return isconnected;
    }
}
