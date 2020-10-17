using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndexManager : MonoBehaviour
{
	public GameObject mScrollRect;
	public GameObject mScrollBackground;
	public GameObject menu;
	public Material bw_mat;


	private string emote = "netural";
	private Rect rect;
	private bool loading = false;

	private Dictionary<string, bool> existMap = new Dictionary<string, bool>();



	// Start is called before the first frame update
	void Start()
    {
		rect = mScrollRect.GetComponent<RectTransform>().rect;
		mScrollBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rect.height);


		Rect menuRect = menu.GetComponent<RectTransform>().rect;
		int len = menu.transform.childCount;
		float diff = menuRect.height / len;
		for (int i = 0; i < len; i++)
		{
			RectTransform rRect = menu.transform.GetChild(i).GetComponent<RectTransform>();
			var prev = menu.transform.GetChild(i).localPosition;
			prev.y = diff*-0.5f + - diff * i;
			menu.transform.GetChild(i).localPosition = prev;
		}

		PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest
		{
		}, (result) =>
		{
			foreach (ItemInstance item in result.Inventory)
			{
				if (item.CatalogVersion == "character")
				{
					if (existMap.ContainsKey(item.ItemId) == false)
					{
						existMap.Add(item.ItemId, true);
					}
				}
			}
			loading = true;
			UpdateMenu();
		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
		});
	}
	public void ClearMenu()
	{
		Transform[] childList = mScrollBackground.GetComponentsInChildren<Transform>(true);
		if (childList != null && childList.Length>1)
		{
			for (int i = 1; i < childList.Length; i++)
			{
				if (childList[i] != transform)
					Destroy(childList[i].gameObject);
			}
		}
	}

	public void UpdateMenu()
	{
		int index = 0;
		float last_y = 0, add_y = 0;
		var ssrList = StaticManager.instance.GetCharacterList((customeData) =>
		{
			customeData.TryGetValue("RARE", out object Rare);
			customeData.TryGetValue("EMO", out object Emo);
			return (emote == "netural" || emote == (string)Emo)
			&& ((string)Rare == "SSR");
		});

		foreach (string ssr in ssrList)
		{
			last_y = AddIcon(add_y, index++, ssr);
		}
		add_y = last_y - 80;
		index = 0;

				var srList = StaticManager.instance.GetCharacterList((customeData) =>
		{
			customeData.TryGetValue("RARE", out object Rare);
			customeData.TryGetValue("EMO", out object Emo);
			return (emote == "netural" || emote == (string)Emo)
			&& ((string)Rare == "SR");
		});

		foreach (string sr in srList)
		{
			last_y = AddIcon(add_y, index++, sr);
		}
		add_y = last_y - 80;
		index = 0;

		
		var nList = StaticManager.instance.GetCharacterList((customeData) =>
		{
			customeData.TryGetValue("RARE", out object Rare);
			customeData.TryGetValue("EMO", out object Emo);
			return (emote == "netural" || emote == (string)Emo)
			&& ((string)Rare == "N");
		});

		foreach (string n in nList)
		{
			last_y = AddIcon(add_y, index++, n);
		}
	}

	float AddIcon(float add_y, int index, string item)
	{
		var icon = Resources.Load<GameObject>("Prefabs/Icon");
		var IconObj = Instantiate(icon, new Vector2(80, -80), Quaternion.identity) as GameObject;
		IconObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
		IconObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
		IconObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
		IconObj.transform.SetParent(mScrollBackground.transform, false);
		var prev = IconObj.transform.localPosition;
		prev.y += add_y;
		float init_x = prev.x;
		while (index > 0)
		{
			prev.x += 120;
			if (rect.width < prev.x + 120)
			{
				prev.x = init_x;
				prev.y -= 120;
			}
			index--;
		}
		IconObj.transform.localPosition = prev;

		if (rect.height < (prev.y - 80) * -1)
		{
			mScrollBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (prev.y - 80) * -1);
		}

		StaticManager.instance.SettingIconImg(item, IconObj);
		if (existMap.ContainsKey(item) == false)
		{
			IconObj.transform.Find("Exist").gameObject.SetActive(true);
			//IconObj.transform.Find("Image").gameObject.GetComponent<Image>().material = bw_mat;
		}

		return prev.y;
	}

	public void ChangeButton(Button button)
	{
		if (loading == false)
			return;
		emote = button.gameObject.name;
		ClearMenu();
		UpdateMenu();
	}
}
