using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using GorillaNetworking;
using GorillaGameModes;
using BepInEx;
using HarmonyLib;

using Utilla.Models;

namespace Utilla
{
	public class GamemodeManager : MonoBehaviour
	{
		public static GamemodeManager Instance { get; private set; }

		const string BasePrefabPath = "CustomGameManager/";

		public int PageCount => Mathf.CeilToInt(Gamemodes.Count() / 4f);

		List<Gamemode> DefaultModdedGamemodes;
		public List<Gamemode> Gamemodes { get; private set; }

		List<PluginInfo> pluginInfos;

		FieldInfo fiGameModeInstance = typeof(GameMode).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
		GameMode gtGameModeInstance;

		FieldInfo fiGameModeTable = typeof(GameMode).GetField("gameModeTable", BindingFlags.Static | BindingFlags.NonPublic);
		Dictionary<int, GorillaGameManager> gtGameModeTable;

		FieldInfo fiGameModeKeyByName = typeof(GameMode).GetField("gameModeKeyByName", BindingFlags.Static | BindingFlags.NonPublic);
		Dictionary<string, int> gtGameModeKeyByName;

		FieldInfo fiGameModes = typeof(GameMode).GetField("gameModes", BindingFlags.Static | BindingFlags.NonPublic);
		List<GorillaGameManager> gtGameModes;

		List<string> gtGameModeNames;

		GameObject moddedGameModesObject;

		Dictionary<string, GameModeSelectorPath> gameModeButtonsDict = new Dictionary<string, GameModeSelectorPath>() 
		{
			{
				"Cave",
				new GameModeSelectorPath() 
				{
					name = "CaveSelector",
                    transform = "Cave_Main_Prefab/CaveComputer/GameModeSelector/Selector Buttons (1)",
                    anchorPath = "anchor",
                }
			},
			{   "Mountain",
				new GameModeSelectorPath() 
				{
					name = "MountainSelector",
                    transform = "Mountain/Geometry/goodigloo/GameModeSelector/Selector Buttons (1)",
                    anchorPath = "anchor",
                }
			},
			{   "Skyjungle",
				new GameModeSelectorPath() 
				{
					name = "CloudsSelector",
                    transform = "skyjungle/UI/GameModeSelector/Selector Buttons (1)",
                    anchorPath = "anchor",
                }
			},
			{
				"Rotating",
				new GameModeSelectorPath() 
				{
					name = "RotatingSelector",
                    transform = "RotatingPermanentEntrance/UI (1)/GameModeSelector/Selector Buttons (1)",
                    anchorPath = "anchor"
                }
			},
            {
                "Metropolis",
                new GameModeSelectorPath()
                {
                    name = "MetroSelector",
                    transform = "MetroMain/ComputerArea/GameModeSelector/Selector Buttons (1)",
                    anchorPath = "anchor"
                }
            }
        };

		void Start()
		{
			Instance = this;
			Events.RoomJoined += OnRoomJoin;
			Events.RoomLeft += OnRoomLeft;

			gtGameModeInstance = fiGameModeInstance.GetValue(null) as GameMode;
			gtGameModeTable = fiGameModeTable.GetValue(null) as Dictionary<int, GorillaGameManager>;
			gtGameModeKeyByName = fiGameModeKeyByName.GetValue(null) as Dictionary<string, int>;
			gtGameModes = fiGameModes.GetValue(null) as List<GorillaGameManager>;
			gtGameModeNames = GameMode.gameModeNames;

			moddedGameModesObject = new GameObject("Modded Game Modes");
			moddedGameModesObject.transform.SetParent(gtGameModeInstance.gameObject.transform);

            GorillaComputer.instance.currentGameMode.Value = PlayerPrefs.GetString("currentGameMode", "INFECTION");

			// prepare the main stump selector
            var defaultSelector = InitializeSelector(new GameModeSelectorPath()
            {
                name = "StumpSelector",
                transform = "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/GameModeSelector/Selector Buttons (1)",
				anchorPath = "anchor",
            });
			defaultSelector.GetSelectorGamemodes(out var gamemodes, out DefaultModdedGamemodes);

			Gamemodes = gamemodes;

			defaultSelector.ShowPage(0); // also relies on Gamemodes :3

            pluginInfos = GetPluginInfos(); // relies on Gamemodes, i did NOT know that !! and got confused for a bit

            Gamemodes.AddRange(GetGamemodes(pluginInfos));

			Gamemodes.ForEach(gamemode => AddGamemodeToPrefabPool(gamemode));

            // prepare the selector "in beach", also located in the GorillaTag scene alongside the default selector
            InitializeSelector(new GameModeSelectorPath()
            {
                name = "BeachSelector",
                transform = "Environment Objects/LocalObjects_Prefab/ForestToBeach/ForestToBeach_Prefab_V4/BeachComputer/GameModeSelector/Selector Buttons (1)",
                anchorPath = "anchor",
            });

            SceneManager.sceneLoaded += OnSceneChange;
		}

		void OnSceneChange(Scene scene, LoadSceneMode loadMode)
		{
			if (gameModeButtonsDict.TryGetValue(scene.name, out var buttonData))
			{
				InitializeSelector(buttonData);
			}
		}

        // void InitializeSelector(string name, Transform parent, string buttonPath, string gamemodesPath)
        GamemodeSelector InitializeSelector(GameModeSelectorPath gmPathData)
		{
			try
			{
				Transform parent = GameObject.Find(gmPathData.transform)?.transform;

                GamemodeSelector selector = new GameObject($"{gmPathData.name} (Utilla)").AddComponent<GamemodeSelector>();

				Transform anchor = parent.Find(gmPathData.anchorPath);

				// child objects might be removed when gamemodes is released, keeping default behaviour for now
				Transform buttonLayout = null;

				foreach(Transform child in anchor) 
				{
					if (child.gameObject.name.StartsWith("ENABLE FOR BETA"))
					{
                        buttonLayout = child;
						break;
					}
				}

				selector.Initialize(parent, anchor, buttonLayout);

				return selector;
			}

            catch (Exception e)
			{
				Debug.LogError($"Utilla: Failed to initialize {name}: {e}");
			}

			return null;
		}

		List<Gamemode> GetGamemodes(List<PluginInfo> infos)
		{
			List<Gamemode> gamemodes = new List<Gamemode>();
			gamemodes.AddRange(DefaultModdedGamemodes);

			HashSet<Gamemode> additonalGamemodes = new HashSet<Gamemode>();
			foreach (var info in infos)
			{
				additonalGamemodes.UnionWith(info.Gamemodes);
			}

			foreach (var gamemode in DefaultModdedGamemodes)
			{
				additonalGamemodes.Remove(gamemode);
			}

			gamemodes.AddRange(additonalGamemodes);

			return gamemodes;
		}

		List<PluginInfo> GetPluginInfos()
		{
			List<PluginInfo> infos = new List<PluginInfo>();
			foreach (var info in BepInEx.Bootstrap.Chainloader.PluginInfos)
			{
				if (info.Value == null) continue;
				BaseUnityPlugin plugin = info.Value.Instance;
				if (plugin == null) continue;
				Type type = plugin.GetType();

				IEnumerable<Gamemode> gamemodes = GetGamemodes(type);

				if (gamemodes.Count() > 0)
				{
					infos.Add(new PluginInfo
					{
						Plugin = plugin,
						Gamemodes = gamemodes.ToArray(),
						OnGamemodeJoin = CreateJoinLeaveAction(plugin, type, typeof(ModdedGamemodeJoinAttribute)),
						OnGamemodeLeave = CreateJoinLeaveAction(plugin, type, typeof(ModdedGamemodeLeaveAttribute))
					});
				}
			}

			return infos;
		}

		Action<string> CreateJoinLeaveAction(BaseUnityPlugin plugin, Type baseType, Type attribute)
		{
			ParameterExpression param = Expression.Parameter(typeof(string));
			ParameterExpression[] paramExpression = new ParameterExpression[] { param };
			ConstantExpression instance = Expression.Constant(plugin);
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			Action<string> action = null;
			foreach (var method in baseType.GetMethods(bindingFlags).Where(m => m.GetCustomAttribute(attribute) != null))
			{
				var parameters = method.GetParameters();
				MethodCallExpression methodCall;
				if (parameters.Length == 0)
				{
					methodCall = Expression.Call(instance, method);
				}
				else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
				{
					methodCall = Expression.Call(instance, method, param);
				}
				else
				{
					continue;
				}

				action += Expression.Lambda<Action<string>>(methodCall, paramExpression).Compile();
			}

			return action;
		}

		HashSet<Gamemode> GetGamemodes(Type type)
		{
			IEnumerable<ModdedGamemodeAttribute> attributes = type.GetCustomAttributes<ModdedGamemodeAttribute>();

			HashSet<Gamemode> gamemodes = new HashSet<Gamemode>();
			if (attributes != null)
			{
				foreach (ModdedGamemodeAttribute attribute in attributes)
				{
					if (attribute.gamemode != null)
					{
						gamemodes.Add(attribute.gamemode);
					}
					else
					{
						gamemodes.UnionWith(DefaultModdedGamemodes);
					}
				}
			}

			return gamemodes;
		}

		void AddGamemodeToPrefabPool(Gamemode gamemode)
		{
			if (gamemode.GameManager is null) return;
            if (gtGameModeKeyByName.ContainsKey(gamemode.GamemodeString) || gtGameModeKeyByName.ContainsKey(gamemode.DisplayName))
            {
                Debug.LogError($"game with name \"{gamemode.GamemodeString}\" or \"{gamemode.DisplayName}\" already exists");
                return;
            }

			Type gmType = gamemode.GameManager;
			if (gmType == null || !gmType.IsSubclassOf(typeof(GorillaGameManager)))
			{
				GameModeType? gmKey = gamemode.BaseGamemode switch
				{
					BaseGamemode.Casual => GameModeType.Casual,
					BaseGamemode.Infection => GameModeType.Infection,
					BaseGamemode.Hunt => GameModeType.Hunt,
					BaseGamemode.Battle => GameModeType.Paintbrawl,
					BaseGamemode.Ambush => GameModeType.Ambush,
                    _ => null
				};

				if (gmKey == null)
				{
					return;
				}

				gtGameModeKeyByName[gamemode.GamemodeString] = (int)gmKey;
				gtGameModeKeyByName[gamemode.DisplayName] = (int)gmKey;
				gtGameModeNames.Add(gamemode.DisplayName);
				return;
			}
			
			GameObject prefab = new GameObject(gamemode.ID);
			prefab.SetActive(false);
			var gameMode = prefab.AddComponent(gamemode.GameManager) as GorillaGameManager;
			int gameModeKey = (int)gameMode.GameType();		

			if (gtGameModeTable.ContainsKey(gameModeKey))
			{
				Debug.LogError($"GameMode {gtGameModeTable[gameModeKey].GameModeName()} is already using GameType {gameModeKey}");
				GameObject.Destroy(prefab);
				return;
			}

			gtGameModeTable[gameModeKey] = gameMode;
			gtGameModeKeyByName[gamemode.GamemodeString] = gameModeKey;
			gtGameModeKeyByName[gamemode.DisplayName] = gameModeKey;
			gtGameModeNames.Add(gamemode.DisplayName);
			gtGameModes.Add(gameMode);

			prefab.transform.SetParent(moddedGameModesObject.transform);
			prefab.SetActive(true);

        }

		internal void OnRoomJoin(object sender, Events.RoomJoinedArgs args)
		{
			string gamemode = args.Gamemode;

			foreach (var pluginInfo in pluginInfos)
			{
				if (pluginInfo.Gamemodes.Any(x => gamemode.Contains(x.GamemodeString)))
				{
					try
					{
						pluginInfo.OnGamemodeJoin?.Invoke(gamemode);
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
				}
			}
		}

		internal void OnRoomLeft(object sender, Events.RoomJoinedArgs args)
		{
			string gamemode = args.Gamemode;

			foreach (var pluginInfo in pluginInfos)
			{
				if (pluginInfo.Gamemodes.Any(x => gamemode.Contains(x.GamemodeString)))
				{
					try
					{
						pluginInfo.OnGamemodeLeave?.Invoke(gamemode);
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
				}
			}
		}
	}
}
