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
	private Dictionary<string, JsonObject> mCharaterDirectory = new Dictionary<string, JsonObject>();

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
				mCharaterDirectory.Add(item.ItemId, CustomObj);
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
		return mCharaterDirectory.ContainsKey(characterId);
	}

	public JsonObject GetCustomData(string characterId)
	{
		//TODO 실시간으로 가져오기, 시간차이 조정하기
		return mCharaterDirectory[characterId];
	}


	public List<string> GetCharacterList(Func<JsonObject, bool> func)
	{
		List<string> list = new List<string>();

		foreach (var entry in mCharaterDirectory)
		{
			if (func(entry.Value))
			{
				list.Add(entry.Key);
			}
		}
		return list;
	}
}
