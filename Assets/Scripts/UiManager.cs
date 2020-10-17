using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoSingleton<UiManager>
{
	enum ROOT
	{
		TOPUI
	}
	enum TOPUI
	{
		USERNAME,
		STAMINA,
		BACK,
		CRYSTAL
	}
	enum T_STAMINABAR
	{
		TEXT,
		BAR,
		REMAIN
	}
	enum T_CRYSTAL
	{
		IMAGE,
		BAR,
		NUM
	}

	private string username;
	public string UserName
	{
		get => username;
		set
		{
			username = value;
			try
			{
				txt_Username.text = username;
			}
			catch
			{
				Debug.Log("Not Found : UserName");
			}
		}
	}

	private int stamina_min;
	public int StaminaMin
	{
		get => stamina_min;
		set
		{
			stamina_min = value;
			try
			{
				txt_StaminaMin.text = ((int)stamina_min).ToString();
			}
			catch
			{
				Debug.Log("Not Found : Stamina (Set Min Value)");
			}
		}
	}

	private int stamina_max = 100;
	public int StaminaMax
	{
		get => stamina_max;
		set
		{
			stamina_max = value;
			try
			{
				txt_StaminaMax.text = ((int)stamina_max).ToString();
			}
			catch
			{
				Debug.Log("Not Found : Stamina (Set Max Value)");
			}
		}
	}

	private string remain_time;
	public string RemainTime
	{
		get => remain_time;
		set
		{
			remain_time = value;
			try
			{
				txt_RemainTime.text = remain_time;
			}
			catch
			{
				Debug.Log("Not Found : Remain Time");
			}
		}
	}
	private Button back_button;
	public Button BackButton
	{
		get => back_button;
	}

	private int crystal;
	public int Crystal
	{
		get => crystal;
		set
		{
			crystal = value;
			try
			{
				txt_Crystal.text = ((int)crystal).ToString();
			}
			catch
			{
				Debug.Log("Not Found : Crystal");
			}
		}
	}

	private GameObject canvas;
	private GameObject Root;

	private Dictionary<int, Transform> dic_root;
	private Dictionary<int, Transform> dic_topui;

	private Dictionary<int, Transform> dic_t_stamina;
	private Dictionary<int, Transform> dic_t_crystal;

	private Text txt_Username;
	private Text txt_StaminaMin;
	private Text txt_StaminaMax;
	private Text txt_RemainTime;
	private Text txt_Crystal;
	private bool isLoading = false;

	DateTime nextFreeTicket = new DateTime();
	private void Start()
	{
		GameObject rootCanvas = GameObject.Find("StaticCanvas");
		DontDestroyOnLoad(rootCanvas);
		canvas = Resources.Load<GameObject>("Prefabs/GameUI");
		Root = Instantiate(canvas, rootCanvas.transform) as GameObject;

		InitDictionaries();
		InitUI();

		GetInventory();
		isLoading = true;
	}

	private void InitDictionaries()
	{
		UI_to_Dic(Root, ref dic_root);
		UI_to_Dic(dic_root[(int)ROOT.TOPUI], ref dic_topui);

		UI_to_Dic(dic_topui[(int)TOPUI.STAMINA], ref dic_t_stamina);
		UI_to_Dic(dic_topui[(int)TOPUI.CRYSTAL], ref dic_t_crystal);

	}
	private void InitUI()
	{
		txt_Username = dic_topui[(int)TOPUI.USERNAME].GetComponent<Text>();
		txt_StaminaMin = dic_t_stamina[(int)T_STAMINABAR.BAR].GetChild(0).GetComponent<Text>();
		txt_StaminaMax = dic_t_stamina[(int)T_STAMINABAR.BAR].GetChild(1).GetComponent<Text>();
		txt_RemainTime = dic_t_stamina[(int)T_STAMINABAR.REMAIN].GetComponent<Text>();
		txt_Crystal = dic_t_crystal[(int)T_CRYSTAL.NUM].GetComponent<Text>();

		back_button = dic_topui[(int)TOPUI.BACK].GetComponent<Button>();
		back_button.onClick.AddListener((delegate { UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Lobby"); }));
		PlayFabManager.instance.GetUserInfo((result) =>
		{
			if(result.AccountInfo.Username != null)
			{
				UserName = result.AccountInfo.Username;
			}
			else if(result.AccountInfo.CustomIdInfo != null)
			{
				UserName = result.AccountInfo.CustomIdInfo.CustomId;
			}
		});
		PlayFabManager.instance.GetUserInventory((result) =>
		{
			StaminaMin = result.VirtualCurrency["ST"];
		});
	}

	private void UI_to_Dic<T>(GameObject parent, ref Dictionary<int, T> dic)
	{
		UI_to_Dic<T>(parent.transform, ref dic);
	}
	private void UI_to_Dic<T>(Transform parent, ref Dictionary<int, T> dic)
	{
		if (dic is null)
			dic = new Dictionary<int, T>();

		dic.Clear();

		int len = parent.childCount;
		for (int i = 0; i < len; i++)
		{
			dic.Add(i, parent.GetChild(i).GetComponent<T>());
		}
	}
	private void ClearChild(Transform tr)
	{
		int childcount = tr.childCount;
		for (int i = childcount; i > 0; i--)
		{
			Destroy(tr.GetChild(i));
		}
	}

	public bool StaminaCapped()
	{
		if (stamina_max <= stamina_min)
		{
			return true;
		}
		return false;
	}


	private void Update()
	{
		if (PlayFabClientAPI.IsClientLoggedIn())
		{
			if (UiManager.instance.StaminaCapped() == false && isLoading == false)
			{
				if (nextFreeTicket.Subtract(DateTime.Now).TotalSeconds <= 0)
				{
					GetInventory();
				}
				else
				{
					var rechargeTime = nextFreeTicket.Subtract(DateTime.Now);
					UiManager.instance.RemainTime = string.Format("{0:0}:{1:00}남음", rechargeTime.Minutes, rechargeTime.Seconds);
				}
			}
		}
	}

	public void GetInventory()
	{
		isLoading = true;
		PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest()
		, (result) =>
		{
			isLoading = false;
			result.VirtualCurrency.TryGetValue("ST", out int staminaBalance);

			if (result.VirtualCurrencyRechargeTimes.TryGetValue("ST", out VirtualCurrencyRechargeTime rechargeDetails))
			{
				UiManager.instance.StaminaMin = staminaBalance;
				UiManager.instance.StaminaMax = rechargeDetails.RechargeMax;

				if (staminaBalance < rechargeDetails.RechargeMax)
				{
					nextFreeTicket = DateTime.Now.AddSeconds(rechargeDetails.SecondsToRecharge);
					var rechargeTime = nextFreeTicket.Subtract(DateTime.Now);

					UiManager.instance.RemainTime = string.Format("{0:0}:{1:00}남음", rechargeTime.Minutes, rechargeTime.Seconds);
				}
				else
				{
					UiManager.instance.RemainTime = string.Empty;
				}
			}

			result.VirtualCurrency.TryGetValue("CY", out int crysyalBalance);
			UiManager.instance.Crystal = crysyalBalance;
		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
			isLoading = false;
		});
	}

}
