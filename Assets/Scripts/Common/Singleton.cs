using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>, new()
{
	private static T _instance = null;
	private static GameObject _gameObject = null;
	private static bool _isInitialize = false;

	protected virtual void OnInitialize() { }


	public static bool isAlive()
	{
		return _instance != null;
	}

	public static T instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType(typeof(T)) as T;

				if (_instance == null)
				{
					_gameObject = new GameObject("__" + typeof(T).ToString());
					_instance = _gameObject.AddComponent<T>();

					DontDestroyOnLoad(_gameObject);

					if (_instance == null)
					{
						//Debug.LogError("Problemduring the creation of" + typeof(T).ToString());
					}
				}

				if (!_isInitialize)
				{
					_instance.OnInitialize();
					_isInitialize = true;
				}
			}

			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance == null)
			_instance = this as T;

		if (!_isInitialize)
		{
			OnInitialize();
			_isInitialize = true;
		}
	}

	protected virtual void OnDestroy()
	{
		if (_gameObject != null)
			Destroy(_gameObject);

		_instance = null;
		_isInitialize = false;
	}

	void OnApplicationQuit()
	{
		_instance = null;
	}
}

public class GSingleton<T> where T : class, new()
{
	public static T instance
	{
		get;
		private set;
	}

	static GSingleton()
	{
		if (GSingleton<T>.instance == null)
		{
			GSingleton<T>.instance = new T();
		}
	}

	~GSingleton() { instance = null; }
}