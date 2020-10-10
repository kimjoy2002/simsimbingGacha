using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class PlayFabManager : MonoSingleton<PlayFabManager>
{
	public Text ErrorMsg;
	public InputField EmailInput, PasswordInput, UsernameInput;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void SignUp()
	{
		Debug.Log("Email:" + EmailInput.text  + ", username: " + UsernameInput.text + "password:" + PasswordInput.text);
		var request = new RegisterPlayFabUserRequest {
			Email = EmailInput.text,
			Username = UsernameInput.text,
			Password = PasswordInput.text
		};
		PlayFabClientAPI.RegisterPlayFabUser(request, OnSignUpSuccess, OnSignUpFailure);
	}

	public void Login()
	{
		var request = new LoginWithEmailAddressRequest
		{
			Email = EmailInput.text,
			Password = PasswordInput.text
		};
		PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
	}

	private void OnSignUpSuccess(RegisterPlayFabUserResult result)
	{
		string temp = "회원가입 성공";
		ErrorMsg.text = temp + "\n";
		Debug.Log("회원가입 성공");
	}

	private void OnSignUpFailure(PlayFabError error)
	{
		string temp = "회원가입 실패" + error.ToString();
		ErrorMsg.text = temp + "\n";
		Debug.LogError(temp);
	}

	private void OnLoginSuccess(LoginResult result)
	{
		string temp = "로그인 성공";
		ErrorMsg.text = temp + "\n";
		Debug.Log(temp);
		SceneManager.LoadSceneAsync("Lobby");
	}

	private void OnLoginFailure(PlayFabError error)
	{
		string temp = "로그인 실패" + error.ToString();
		ErrorMsg.text = temp + "\n";
		Debug.Log(temp);
	}


	private void CommonFailure(PlayFabError error)
	{
		string temp = "Prefab 실패" + error.ToString();
		ErrorMsg.text = temp + "\n";
		Debug.Log(temp);
	}

	public void GetUserInfo(System.Action<GetAccountInfoResult> returnCallback)
	{
		GetAccountInfoRequest request = new GetAccountInfoRequest();
		PlayFabClientAPI.GetAccountInfo(request, returnCallback, CommonFailure);
	}

	public void GetUserInventory(System.Action<GetUserInventoryResult> returnCallback)
	{
		GetUserInventoryRequest request = new GetUserInventoryRequest();
		PlayFabClientAPI.GetUserInventory(request, returnCallback, CommonFailure);
	}

	public void UseStamina(System.Action<ModifyUserVirtualCurrencyResult> returnCallback, int amount)
	{
		SubtractUserVirtualCurrencyRequest request = new SubtractUserVirtualCurrencyRequest()
		{
			VirtualCurrency = "ST", Amount = amount
		};
		PlayFabClientAPI.SubtractUserVirtualCurrency(request, returnCallback, CommonFailure);
	}
}