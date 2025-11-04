using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
	private static bool _initialized;

	void Awake()
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
		EnsureManager<WorldHandlerHost>();
		EnsureManager<PartyManager>();
		EnsureManager<SkillManager>();

		RegisterServices();
	}

	private void EnsureManager<T>() where T : Component
	{
		var existing = FindFirstObjectByType<T>();
		if (existing == null)
		{
			var go = new GameObject(typeof(T).Name);
			DontDestroyOnLoad(go);
			existing = go.AddComponent<T>();
		}
	}

	private void RegisterServices()
	{
		// Register known singletons where appropriate; if not present, TryResolve pattern is used elsewhere
		var audio = FindFirstObjectByType<AudioManager>();
		if (audio != null) ServiceLocator.Register<AudioManager>(audio);

		var time = FindFirstObjectByType<TimeManager>();
		if (time != null) ServiceLocator.Register<TimeManager>(time);

		var weather = FindFirstObjectByType<WeatherManager>();
		if (weather != null) ServiceLocator.Register<WeatherManager>(weather);

		var party = FindFirstObjectByType<PartyManager>();
		if (party != null) ServiceLocator.Register<PartyManager>(party);


		var skills = FindFirstObjectByType<SkillManager>();
		if (skills != null) ServiceLocator.Register<SkillManager>(skills);

		// Asset provider (Resources fallback by default)
		ServiceLocator.Register<IAssetProvider>(new ResourcesAssetProvider());
	}
}

// WorldHandler is documented as a static hub; create a host MonoBehaviour for lifecycle if needed
public class WorldHandlerHost : MonoBehaviour { }


