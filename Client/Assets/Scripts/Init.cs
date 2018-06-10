using System;
using UnityEngine;
using UnityEngine.UI;
using Model;
using System.Net;

public class Init : MonoBehaviour
{
    public static Init Instance;
    public ILRuntime.Runtime.Enviorment.AppDomain AppDomain;

    public InputField pname;
    public InputField password;
    public Button loginBut;
    public Button enterMap;

    GameObject uiLogin;
    GameObject uiLobby;

    private async void Start()
    {
        try
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;

            Game.EventSystem.Add(DLLType.Model, typeof(Game).Assembly);

            
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<NetOuterComponent>();
            Game.Scene.AddComponent<ResourcesComponent>();
            
            Game.Scene.AddComponent<PlayerComponent>();
            Game.Scene.AddComponent<UnitComponent>();
            Game.Scene.AddComponent<ClientFrameComponent>();

            await BundleHelper.DownloadBundle();

            // 加载配置
            Game.Scene.GetComponent<ResourcesComponent>().LoadBundle("config.unity3d");
            Game.Scene.AddComponent<ConfigComponent>();
            Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle("config.unity3d");

            //MessageDispatherComponent
            Game.Scene.AddComponent<MessageDispatherComponent>();


            uiLogin = GameObject.Find("UILogin");
            uiLobby = GameObject.Find("UILobby");
            uiLobby.SetActive(false);

            loginBut.onClick.AddListener(OnLogin);
            enterMap.onClick.AddListener(EnterMap);

            Game.EventSystem.Run(EventIdType.InitSceneStart);
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }



    public async void OnLogin()
    {
        IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint("127.0.0.1:10002");
        Session session = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
        string text = pname.text;

        R2C_Login r2CLogin = (R2C_Login)await session.Call(new C2R_Login() { Account = text, Password = "111111" }, true);
        if (r2CLogin.Error != ErrorCode.ERR_Success)
        {
            Log.Error(r2CLogin.Error.ToString());
            return;
        }

        connetEndPoint = NetworkHelper.ToIPEndPoint(r2CLogin.Address);
        Session gateSession = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
        Game.Scene.AddComponent<SessionComponent>().Session = gateSession;

        G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await SessionComponent.Instance.Session.Call(new C2G_LoginGate() { Key = r2CLogin.Key }, true);

        Log.Info("登陆gate成功!");
        uiLogin.SetActive(false);
        uiLobby.SetActive(true);

        // 创建Player
        Player player = Model.EntityFactory.CreateWithId<Player>(g2CLoginGate.PlayerId);
        PlayerComponent playerComponent = Game.Scene.GetComponent<PlayerComponent>();
        playerComponent.MyPlayer = player;
    }
    
    private async void EnterMap()
    {
        try
        {
            Debug.Log(SessionComponent.Instance.Session);
            G2C_EnterMap g2CEnterMap = await SessionComponent.Instance.Session.Call<G2C_EnterMap>(new C2G_EnterMap());


            uiLobby.SetActive(false);
            Log.Info("EnterMap...");
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }


    private void Update()
    {
        //this.update?.Run();
        Game.EventSystem.Update();
    }

    private void LateUpdate()
    {
        //this.lateUpdate?.Run();
        Game.EventSystem.LateUpdate();
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Game.Close();
        //this.onApplicationQuit?.Run();
    }
}
