using UnityEngine;

public class TickSystem : MonoBehaviour
{
	public static TickSystem Instance { get; private set; }

	public delegate void TickEvent(float deltaTime);
	public static event TickEvent OnTick;
	public static event System.Action OnHalfSecond;
	public static event System.Action OnSecond;

	[SerializeField]
	private float _halfSecond = 0.5f;
	[SerializeField]
	private float _oneSecond = 1f;

	private float _accumHalf;
	private float _accumOne;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Update()
	{
		float dt = Time.deltaTime;
		OnTick?.Invoke(dt);

		_accumHalf += dt;
		_accumOne += dt;

		if (_accumHalf >= _halfSecond)
		{
			_accumHalf -= _halfSecond;
			OnHalfSecond?.Invoke();
		}
		if (_accumOne >= _oneSecond)
		{
			_accumOne -= _oneSecond;
			OnSecond?.Invoke();
		}
	}
}


