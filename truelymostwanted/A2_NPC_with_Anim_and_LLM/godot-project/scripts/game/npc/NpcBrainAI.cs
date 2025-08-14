using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using LabyrinthExplorer3D.scripts.game.ai;
using LabyrinthExplorer3D.scripts.game.npc;
using LabyrinthExplorer3D.scripts.game.player;

[GlobalClass]
public partial class NpcBrainAI : Node
{
    public static NpcBrainAI Instance { get; private set; }
    
    [Export] public NpcController3D NpcController;
    
    [Export] public LlmClient LlmClient = new LlmClient();
    public CancellationTokenSource Cts = new CancellationTokenSource();


    public async void UpdateNpcAsync()
    {
        while (!Cts.IsCancellationRequested)
        {
            await Task.Delay(2000, Cts.Token);
            
            var isPause = GetTree().IsPaused();
            if (isPause)
                continue;

            try
            {
                var result = await LlmClient.SendNpcBrainRequestAsync(NpcController3D.Instance.CurrentNpc, PlayerController3D.Instance.CurrentPlayer);
                GD.Print(result);
            }
            catch (Exception e)
            {
                GD.Print(e);
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        Task.Run(UpdateNpcAsync);
    }

    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what == NotificationWMCloseRequest)
        {
            Cts.Cancel();
        }
    }
}