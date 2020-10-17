using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

	public void Start()
	{
		_ = UiManager.instance;
		_ = StaticManager.instance;
	}

	public void Lobby()
	{
		Debug.Log("toLobby");
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Lobby");
	}


	public void StatusWindow()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("ChracterList");
	}

	public void Eungae()
	{
		PlayFabManager.instance.UseStamina((result) =>
		{
			UiManager.instance.StaminaMin = result.Balance;
		}, 1);
	}

	public void Gacha()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GachaMenu");
	}
	public void Index()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Index");
	}
}
