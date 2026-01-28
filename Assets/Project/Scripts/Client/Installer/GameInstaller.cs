using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameClient
{
    public class GameInstaller : MonoInstaller
    {

        [SerializeField] private NetworkManager _networkManager;

        public override void InstallBindings()
        {
            Container.Bind<ICustomSerializer>().To<BaseUnitySerializer>().AsSingle();
            Container.Bind<LocalDatastore>().AsSingle().NonLazy();
            Container.Bind<IDeviceInput>().To<DesktopDeviceInput>().AsSingle();

            Container.Bind<NetworkManager>().FromInstance(_networkManager).AsSingle();
            Container.BindInterfacesAndSelfTo<ServerProvider>().AsSingle().NonLazy();
        }
    }
}
