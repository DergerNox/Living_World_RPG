using System;
using System.Collections.Generic;

public static class ServiceLocator
{
	private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

	public static void Register<TService>(TService instance) where TService : class
	{
		var key = typeof(TService);
		_services[key] = instance;
	}

	public static TService Resolve<TService>() where TService : class
	{
		var key = typeof(TService);
		if (_services.TryGetValue(key, out var obj))
		{
			return obj as TService;
		}
		throw new InvalidOperationException($"Service not registered: {key.Name}");
	}

	public static bool TryResolve<TService>(out TService service) where TService : class
	{
		var key = typeof(TService);
		if (_services.TryGetValue(key, out var obj))
		{
			service = obj as TService;
			return service != null;
		}
		service = null;
		return false;
	}

	public static void Clear()
	{
		_services.Clear();
	}
}


