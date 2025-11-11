using UnityEngine;
using LivingWorldRPG.PartySystem;

namespace LivingWorldRPG.Core
{
	public class DebugHUD : MonoBehaviour
	{
		[SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
		[SerializeField] private bool visible;

		private float fps;
		private float accum;
		private int frames;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		private void Update()
		{
			if (Input.GetKeyDown(toggleKey))
			{
				visible = !visible;
			}

			frames++;
			accum += Time.unscaledDeltaTime;
			if (accum >= 0.5f)
			{
				fps = frames / accum;
				accum = 0f;
				frames = 0;
			}
		}

		private void OnGUI()
		{
			if (!visible)
			{
				return;
			}

			GUI.color = Color.white;
			GUI.Label(new Rect(10, 10, 300, 20), $"FPS: {fps:F1}");
			GUI.Label(new Rect(10, 30, 300, 20), $"Parties: {SafeCountParties()}");
		}

		private static int SafeCountParties()
		{
			var pm = PartyManager.Instance;
			return pm != null && pm.AllParties != null ? pm.AllParties.Count : 0;
		}
	}
}
