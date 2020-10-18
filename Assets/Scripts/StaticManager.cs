using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticManager : MonoSingleton<StaticManager>
{
	private Dictionary<string, JsonObject> mCharacterCustomDataDirectory = new Dictionary<string, JsonObject>();

	public bool mCharacterLocalDataChanged = true;
	private List<ItemInstance> mCharacterInventoryList = new List<ItemInstance>();

	public void Start()
	{
		var catalogRequest = new GetCatalogItemsRequest()
		{
			CatalogVersion = "character",
		};

		PlayFabClientAPI.GetCatalogItems(catalogRequest, (result) =>
		{
			foreach (var item in result.Catalog)
			{
				JsonObject CustomObj = (JsonObject)PlayFab.Json.PlayFabSimpleJson.DeserializeObject(item.CustomData);
				mCharacterCustomDataDirectory.Add(item.ItemId, CustomObj);
			}
		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
		});
	}

	public string GachaPage
	{
		get;
		set;
	}
	public int GachaNum
	{
		get;
		set;
	}

	public void SettingIconImg(string ItemId, GameObject iconObj)
	{
		Sprite img = Resources.Load<Sprite>("Character/Face/" + (string)ItemId);
		iconObj.transform.Find("Image").gameObject.GetComponent<Image>().sprite = img;
		if(IsContainsCharacter(ItemId))
		{
			JsonObject CustomObj = GetCustomData(ItemId);
			CustomObj.TryGetValue("RARE", out object Rare);
			CustomObj.TryGetValue("EMO", out object Emo);
			Sprite rareImg = Resources.Load<Sprite>("Character/Rare/" + Rare);
			iconObj.transform.Find("Rare").gameObject.GetComponent<Image>().sprite = rareImg;
			Sprite borderImg = Resources.Load<Sprite>("Character/Border/" + Rare);
			iconObj.transform.Find("Border").gameObject.GetComponent<Image>().sprite = borderImg;
			Sprite emoImg = Resources.Load<Sprite>("Character/Emotial/" + Emo);
			iconObj.transform.Find("Emotial").gameObject.GetComponent<Image>().sprite = emoImg;
		}
	}

	public bool IsContainsCharacter(string characterId)
	{
		//TODO 실시간으로 가져오기, 시간차이 조정하기
		return mCharacterCustomDataDirectory.ContainsKey(characterId);
	}

	public JsonObject GetCustomData(string characterId)
	{
		//TODO 실시간으로 가져오기, 시간차이 조정하기
		return mCharacterCustomDataDirectory[characterId];
	}
	public int GetCharRareToInt(string characterId)
	{
		JsonObject CustomObj = mCharacterCustomDataDirectory[characterId];
		CustomObj.TryGetValue("RARE", out object Rare);
		if ((string)Rare == "SSR")
			return 3;
		else if ((string)Rare == "SR")
			return 2;
		else
			return 1;
	}

	public int GetCharEmoToInt(string characterId)
	{
		JsonObject CustomObj = mCharacterCustomDataDirectory[characterId];
		CustomObj.TryGetValue("EMO", out object Emo);
		if ((string)Emo == "angry")
			return 5;
		else if ((string)Emo == "sad")
			return 4;
		else if ((string)Emo == "smile")
			return 3;
		else if ((string)Emo == "stupid")
			return 2;
		else
			return 1;
	}

	public List<string> GetCharacterList(Func<JsonObject, bool> func)
	{
		List<string> list = new List<string>();

		foreach (var entry in mCharacterCustomDataDirectory)
		{
			if (func(entry.Value))
			{
				list.Add(entry.Key);
			}
		}
		return list;
	}


	public void GetCharacterDataList(Action<List<ItemInstance>> resultCallback, Action<PlayFabError> errorCallback)
	{
		if(mCharacterLocalDataChanged == false)
		{
			resultCallback.Invoke(mCharacterInventoryList);
		}
		else
		{
			PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest
			{
			}, (result) =>
			{
				mCharacterInventoryList.Clear();
				foreach (ItemInstance item in result.Inventory)
				{
					if (item.CatalogVersion == "character")
					{
						mCharacterInventoryList.Add(item);
					}
				}
				mCharacterLocalDataChanged = false;
				resultCallback.Invoke(mCharacterInventoryList);
				
			}, (error) =>
			{
				errorCallback.Invoke(error);
			});
		}
	}
}
