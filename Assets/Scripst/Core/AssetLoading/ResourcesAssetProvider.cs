using System.Threading.Tasks;
using UnityEngine;

public class ResourcesAssetProvider : IAssetProvider
{
	public T Load<T>(string path) where T : Object
	{
		return Resources.Load<T>(path);
	}

	public T[] LoadAll<T>(string path) where T : Object
	{
		Object[] objs = Resources.LoadAll(path, typeof(T));
		T[] result = new T[objs.Length];
		for (int i = 0; i < objs.Length; i++) result[i] = objs[i] as T;
		return result;
	}

	public async Task<T> LoadAsync<T>(string path) where T : Object
	{
		ResourceRequest req = Resources.LoadAsync<T>(path);
		while (!req.isDone) await Task.Yield();
		return req.asset as T;
	}

	public Sprite LoadSprite(string path)
	{
		return Load<Sprite>(path);
	}
}


