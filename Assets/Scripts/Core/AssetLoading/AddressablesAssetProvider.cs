// Placeholder for future Addressables implementation to keep API stable
// Requires com.unity.addressables package
using System.Threading.Tasks;
using UnityEngine;

public class AddressablesAssetProvider : IAssetProvider
{
	public T Load<T>(string path) where T : Object
	{
		Debug.LogWarning("AddressablesAssetProvider.Load is not implemented. Falling back not available in this provider.");
		return null;
	}

	public T[] LoadAll<T>(string path) where T : Object
	{
		Debug.LogWarning("AddressablesAssetProvider.LoadAll is not implemented.");
		return new T[0];
	}

	public async Task<T> LoadAsync<T>(string path) where T : Object
	{
		await Task.Yield();
		Debug.LogWarning("AddressablesAssetProvider.LoadAsync is not implemented.");
		return null;
	}

	public Sprite LoadSprite(string path)
	{
		return Load<Sprite>(path);
	}
}


