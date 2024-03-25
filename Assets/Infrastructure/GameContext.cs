using System.ComponentModel;
using UnityEngine;
using Zenject;

public class GameContext : MonoInstaller
{
    [SerializeField] private TextLogsDisplay logger;
    private TargetPool pool = new TargetPool();
    public override void InstallBindings()
    {
        Container.Bind<TargetPool>().FromInstance(pool).AsSingle();
        Container.Bind<ILogger>().FromInstance(logger).AsSingle();
    }

}