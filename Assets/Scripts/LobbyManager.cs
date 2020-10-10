using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
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
