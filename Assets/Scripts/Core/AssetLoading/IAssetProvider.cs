using System.Threading.Tasks;
using UnityEngine;

public interface IAssetProvider
{
	T Load<T>(string path) where T : Object;
	T[] LoadAll<T>(string path) where T : Object;
	Task<T> LoadAsync<T>(string path) where T : Object;
	Sprite LoadSprite(string path);
}


