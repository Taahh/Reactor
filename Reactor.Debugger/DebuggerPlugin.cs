using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor.Extensions;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Reactor.Debugger
{
    [BepInPlugin(Id, "Reactor.Debugger", "2021.3.31")]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    [ReactorPluginSide(PluginSide.ClientOnly)]
    public class DebuggerPlugin : BasePlugin
    {
        public const string Id = "gg.reactor.debugger";

        public Harmony Harmony { get; } = new Harmony(Id);
        public DebuggerComponent Component { get; private set; }

        public override void Load()
        {
            RegisterInIl2CppAttribute.Register();

            var gameObject = new GameObject(nameof(DebuggerPlugin)).DontDestroy();
            Component = gameObject.AddComponent<DebuggerComponent>();

            GameOptionsData.MaxImpostors = GameOptionsData.RecommendedImpostors = Enumerable.Repeat((int) byte.MaxValue, byte.MaxValue).ToArray();
            GameOptionsData.MinPlayers = Enumerable.Repeat(1, 4).ToArray();

            System.Console.WriteLine("Patching started.");
            List<string> blacklist = new List<string>();
            blacklist.Add("FixedUpdate");
            blacklist.Add("Awake");
            foreach (var method in typeof(ShipStatus).GetMethods()) {
                if (method.ReturnType == typeof(void) && method.GetParameters().Length == 0 && !blacklist.Contains(method.Name)) {
                    System.Console.WriteLine("Patching method " + method.Name + " ( PATCHED )");
                }
            }
            System.Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAa");
            foreach (var method in typeof(ShipStatus).GetMethods()) {
                if (method.ReturnType == typeof(void) && method.GetParameters().Length == 2 && !blacklist.Contains(method.Name)) {
                    System.Console.WriteLine("Patching method " + method.Name + " ( PATCHED " + method.GetParameters()[0].ParameterType.Name + " - " + method.GetParameters()[1].ParameterType.Name + " )");
                }
            }

            System.Console.WriteLine("Patching ended.");
            
            Harmony.PatchAll();
        }

        [RegisterInIl2Cpp]
        public class DebuggerComponent : MonoBehaviour
        {
            [HideFromIl2Cpp]
            public bool DisableGameEnd { get; set; }

            [HideFromIl2Cpp]
            public DragWindow TestWindow { get; }

            public DebuggerComponent(IntPtr ptr) : base(ptr)
            {
                TestWindow = new DragWindow(new Rect(20, 20, 0, 0), "Debugger", (a) =>
                {
                    GUILayout.Label("Name: " + SaveManager.PlayerName, new Il2CppReferenceArray<GUILayoutOption>(0));
                    DisableGameEnd = GUILayout.Toggle(DisableGameEnd, "Disable game end", new Il2CppReferenceArray<GUILayoutOption>(0));

                    if (AmongUsClient.Instance.AmHost && ShipStatus.Instance && GUILayout.Button("Force game end", new Il2CppReferenceArray<GUILayoutOption>(0)))
                    {
                        //PBKIGLMJEDH, NAKGJLBMEND, KDONFIJIKKG, JPJJIIFKOFM
                        ShipStatus.Instance.enabled = false;
                        ShipStatus.KDONFIJIKKG(GameOverReason.ImpostorDisconnect, false);
                    }

                    if (TutorialManager.InstanceExists && GUILayout.Button("Spawn a dummy", new Il2CppReferenceArray<GUILayoutOption>(0)))
                    {
                        var playerControl = Instantiate(TutorialManager.Instance.PlayerPrefab);
                        var i = playerControl.PlayerId = (byte) GameData.Instance.GetAvailableId();
                        GameData.Instance.AddPlayer(playerControl);
                        AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);
                        playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                        playerControl.GetComponent<DummyBehaviour>().enabled = true;
                        playerControl.NetTransform.enabled = false;
                        playerControl.SetName("Dummy " + (i + 1));
                        playerControl.SetColor((byte) (i % Palette.PlayerColors.Length));
                        GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
                    }

                    if (PlayerControl.LocalPlayer)
                    {
                        var position = PlayerControl.LocalPlayer.transform.position;
                        GUILayout.Label($"x: {position.x}", new Il2CppReferenceArray<GUILayoutOption>(0));
                        GUILayout.Label($"y: {position.y}", new Il2CppReferenceArray<GUILayoutOption>(0));
                    }
                })
                {
                    Enabled = false
                };
            }

            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    System.Console.WriteLine("AA");
                    TestWindow.Enabled = !TestWindow.Enabled;
                }
            }

            private void OnGUI()
            {
                TestWindow.OnGUI();
            }
        }
    }
}
