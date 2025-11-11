using UnityEngine;
using LivingWorldRPG.World;

namespace LivingWorldRPG.Core
{
	public class Bootstrapper : MonoBehaviour
	{
		private static bool _initialized;

		private void Awake()
		{
			if (_initialized)
			{
				Destroy(gameObject);
				return;
			}

			_initialized = true;
			DontDestroyOnLoad(gameObject);

			EnsureManager<AudioManager>();
			EnsureManager<TimeManager>();
			EnsureManager<WeatherManager>();
			EnsureManager<WorldHandler>();
			EnsureManager<WorldGenerator>();
			EnsureManager<PartyManager>();
			EnsureManager<QuestService>();
			EnsureManager<TradeService>();
			EnsureManager<SkillManager>();

			RegisterServices();
		}

		private static void EnsureManager<T>() where T : Component
		{
			var existing = FindFirstObjectByType<T>();
			if (existing == null)
			{
				var go = new GameObject(typeof(T).Name);
				DontDestroyOnLoad(go);
				go.AddComponent<T>();
			}
		}

		private void RegisterServices()
		{
			// Register known singletons where appropriate; if not present, TryResolve pattern is used elsewhere
			var audio = FindFirstObjectByType<AudioManager>();
			if (audio != null)
			{
				ServiceLocator.Register<AudioManager>(audio);
				ServiceLocator.Register<IAudioService>(audio);
			}

			var time = FindFirstObjectByType<TimeManager>();
			if (time != null)
			{
				ServiceLocator.Register<TimeManager>(time);
				ServiceLocator.Register<ITimeService>(time);
			}

			var weather = FindFirstObjectByType<WeatherManager>();
			if (weather != null)
			{
				ServiceLocator.Register<WeatherManager>(weather);
			}

			var worldGen = FindFirstObjectByType<WorldGenerator>();
			if (worldGen != null)
			{
				ServiceLocator.Register<WorldGenerator>(worldGen);
				ServiceLocator.Register<IWorldGenerator>(worldGen);
			}

			var worldHandler = FindFirstObjectByType<WorldHandler>();
			if (worldHandler != null)
			{
				ServiceLocator.Register<WorldHandler>(worldHandler);
				ServiceLocator.Register<IWorldStateRepository>(worldHandler);
			}

			if (worldGen != null && worldHandler != null)
			{
				var simulationService = new WorldSimulationService(worldHandler, worldGen);
				ServiceLocator.Register<IWorldSimulationService>(simulationService);
			}

			var party = FindFirstObjectByType<PartyManager>();
			if (party != null)
			{
				ServiceLocator.Register<PartyManager>(party);
				ServiceLocator.Register<IPartyService>(party);
			}

			var skills = FindFirstObjectByType<SkillManager>();
			if (skills != null)
			{
				ServiceLocator.Register<SkillManager>(skills);
			}

			var questService = FindFirstObjectByType<QuestService>();
			if (questService != null)
			{
				ServiceLocator.Register<QuestService>(questService);
				ServiceLocator.Register<IQuestService>(questService);
			}

			var tradeService = FindFirstObjectByType<TradeService>();
			if (tradeService != null)
			{
				ServiceLocator.Register<TradeService>(tradeService);
				ServiceLocator.Register<ITradeService>(tradeService);
			}

			ServiceLocator.Register<IBattleResolver>(new Battle());

			// Asset provider (Resources fallback by default)
			ServiceLocator.Register<IAssetProvider>(new ResourcesAssetProvider());
		}
	}
}
