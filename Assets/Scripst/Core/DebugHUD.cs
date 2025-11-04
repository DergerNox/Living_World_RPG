using UnityEngine;

public class DebugHUD : MonoBehaviour
{
	public KeyCode toggleKey = KeyCode.BackQuote;
	public bool visible;
	private float _fps;
	private float _accum;
	private int _frames;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	void Update()
	{
		if (Input.GetKeyDown(toggleKey)) visible = !visible;
		_frames++;
		_accum += Time.unscaledDeltaTime;
		if (_accum >= 0.5f)
		{
			_fps = _frames / _accum;
			_accum = 0f;
			_frames = 0;
		}
	}

	void OnGUI()
	{
		if (!visible) return;
		GUI.color = Color.white;
		GUI.Label(new Rect(10, 10, 300, 20), $"FPS: {_fps:F1}");
		GUI.Label(new Rect(10, 30, 300, 20), $"Parties: {SafeCountParties()}");
	}

	int SafeCountParties()
	{
		var pm = FindFirstObjectByType<PartyManager>();
		return pm != null && pm.AllParties != null ? pm.AllParties.Count : 0;
	}
}


