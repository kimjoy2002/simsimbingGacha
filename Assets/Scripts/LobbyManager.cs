using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

	public GameObject LoginBonusWindow;

	public class OnLoginResult
	{
		public string Result;
		public string LoginBonus;
		public int BonusAmount;
	}


	public void Start()
	{
		_ = UiManager.instance;
		_ = StaticManager.instance;
		GetLoginBonusOnServer();
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

	public void LoginBonusOk()
	{
		LoginBonusWindow.gameObject.SetActive(false);
	}

	public void GetLoginBonusOnServer()
	{
		var scriptRequest = new ExecuteCloudScriptRequest()
		{
			FunctionName = "OnLogin",
			FunctionParameter = new {},
		};

		PlayFabClientAPI.ExecuteCloudScript(scriptRequest, (result) =>
		{
			Debug.Log(PlayFab.Json.PlayFabSimpleJson.SerializeObject(result));
			JsonObject jsonResult = (JsonObject)result.FunctionResult;
			Debug.Log(jsonResult.ToString());

			OnLoginResult result_ = JsonUtility.FromJson<OnLoginResult>(jsonResult.ToString());

			if(result_.LoginBonus == "true")
			{
				LoginBonusWindow.gameObject.SetActive(true);
				UiManager.instance.GetInventory();
			}
		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
		});
	}


	public void Gacha()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GachaMenu");
	}
	public void Index()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Index");
	}
	public void Party()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("CharacterSelect");
	}
	public void Battle()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GameScreen");
	}
}
