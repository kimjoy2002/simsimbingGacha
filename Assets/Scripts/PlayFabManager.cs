using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using System;

public class PlayFabManager : MonoSingleton<PlayFabManager>
{
	public Text ErrorMsg;
	public InputField EmailInput, PasswordInput;
	public InputField SignEmailInput, SignPasswordInput, SignUsernameInput;
	public GameObject SignWindow, ResultWindow;

	private string DeviceUniqueIdentifier()
	{
		string deviceId = "";

#if UNITY_EDITOR
		deviceId = SystemInfo.deviceUniqueIdentifier + "-editor";
#elif UNITY_ANDROID
		AndroidJavaClass up = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject> ("getContentResolver");
		AndroidJavaClass secure = new AndroidJavaClass ("android.provider.Settings$Secure");
		deviceId = secure.CallStatic<string> ("getString", contentResolver, "android_id");
#elif UNITY_WEBGL
		if (!PlayerPrefs.HasKey("UniqueIdentifier"))
			PlayerPrefs.SetString("UniqueIdentifier", Guid.NewGuid().ToString());
		deviceId = PlayerPrefs.GetString("UniqueIdentifier");
#else
		deviceId = SystemInfo.deviceUniqueIdentifier;
#endif
		return deviceId;
	}


	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (EmailInput.isFocused == true)
				PasswordInput.Select();

			if (SignEmailInput.isFocused == true)
				SignUsernameInput.Select();
			else if (SignUsernameInput.isFocused == true)
				SignPasswordInput.Select();
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{

			if (EmailInput.isFocused == true || PasswordInput.isFocused == true)
			{
				Login();
			}
			else if (SignEmailInput.isFocused == true || SignUsernameInput.isFocused == true || SignPasswordInput.isFocused == true)
			{
				SignUpComfirm();
			}
		}
	}

	public void SignUp()
	{
		SignWindow.gameObject.SetActive(true);
	}


	public void SignUpComfirm()
	{
		var request = new RegisterPlayFabUserRequest
		{
			Email = SignEmailInput.text,
			Username = SignUsernameInput.text,
			Password = SignPasswordInput.text
		};
		PlayFabClientAPI.RegisterPlayFabUser(request, OnSignUpSuccess, OnSignUpFailure);
	}


	public void CancleSignWindow()
	{
		SignWindow.gameObject.SetActive(false);
	}

	public void CancleResultWindow()
	{
		ResultWindow.gameObject.SetActive(false);
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
		ErrorMsg.text = temp;
		SignWindow.gameObject.SetActive(false);
		ResultWindow.gameObject.SetActive(true);
		Debug.Log("회원가입 성공");
	}

	private void OnSignUpFailure(PlayFabError error)
	{
		string temp = "회원가입 실패";
		string detail = "알 수 없는 이유";
		if(error.Error != PlayFabErrorCode.Unknown)
		{
			detail = error.GenerateErrorReport();
		}
		ErrorMsg.text = temp + "\n" + detail;
		ResultWindow.gameObject.SetActive(true);
		Debug.LogError(temp);
	}

	private void OnLoginSuccess(LoginResult result)
	{
		string temp = "로그인 성공";
		Debug.Log(temp);
		SceneManager.LoadSceneAsync("Lobby");
	}

	private void OnLoginFailure(PlayFabError error)
	{
		string temp = "로그인 실패";
		string detail = "알 수 없는 이유";
		if (error.Error != PlayFabErrorCode.Unknown)
		{
			detail = error.GenerateErrorReport();
		}
		ErrorMsg.text = temp + "\n" + detail;
		ResultWindow.gameObject.SetActive(true);
		Debug.LogError(temp);
	}

	public void Guest()
	{
		string deviceId = DeviceUniqueIdentifier();
		var request = new LoginWithCustomIDRequest
		{
			TitleId = "54F4F",
			CustomId = deviceId,
			CreateAccount = true
		};
		PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
	}


	private void CommonFailure(PlayFabError error)
	{
		string temp = "Prefab 실패" + error.ToString();
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