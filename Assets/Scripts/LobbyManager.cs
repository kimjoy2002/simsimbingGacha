using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	DateTime nextFreeTicket = new DateTime();

	private void Update()
	{
		if (PlayFabClientAPI.IsClientLoggedIn())
		{
			if(UiManager.instance.StaminaCapped() == false)
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

	void GetInventory()
	{
		PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest()
		, (result) =>
		{
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
		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
		});
	}

	public void eungae()
    {
		PlayFabManager.instance.UseStamina((result) =>
		{
			UiManager.instance.StaminaMin = result.Balance;
		}, 1);
	}

	public void gacha()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Gacha");
	}
}
