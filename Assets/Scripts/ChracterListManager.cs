using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChracterListManager : MonoBehaviour
{
	enum SORT_TYPE {
		NEW,
		OLD,
		RARE_DES,
		RARE_ASC,
		EMO,
		LEVEL
	};

	public GameObject mScrollRect;
	public GameObject mScrollBackground;

	private Rect rect;

	// Start is called before the first frame update
	void Start()
	{
		rect = mScrollRect.GetComponent<RectTransform>().rect;
		mScrollBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rect.height);
		TableRefresh(SORT_TYPE.NEW);
	}


	private void TableRefresh(SORT_TYPE sortType)
	{
		StaticManager.instance.GetCharacterDataList(
		(result) =>
		{
			int index = 0;
			List<ItemInstance> itemValue = new List<ItemInstance>();
			foreach (var val in result)
			{
				//copy
				itemValue.Add(val);
			}

			itemValue.Sort(delegate (ItemInstance A, ItemInstance B)
			{
				switch(sortType)
				{
					case SORT_TYPE.NEW:
						return DateTime.Compare(B.PurchaseDate.GetValueOrDefault(), A.PurchaseDate.GetValueOrDefault());
					case SORT_TYPE.OLD:
						return DateTime.Compare(A.PurchaseDate.GetValueOrDefault(), B.PurchaseDate.GetValueOrDefault());
					case SORT_TYPE.RARE_DES:
						return StaticManager.instance.GetCharRareToInt(A.ItemId) < StaticManager.instance.GetCharRareToInt(B.ItemId) ? 1 : (
							StaticManager.instance.GetCharRareToInt(A.ItemId) > StaticManager.instance.GetCharRareToInt(B.ItemId) ? -1 : 0);
					case SORT_TYPE.RARE_ASC:
						return StaticManager.instance.GetCharRareToInt(A.ItemId) > StaticManager.instance.GetCharRareToInt(B.ItemId) ? 1 : (
							StaticManager.instance.GetCharRareToInt(A.ItemId) < StaticManager.instance.GetCharRareToInt(B.ItemId) ? -1 : 0);
					case SORT_TYPE.EMO:
						return StaticManager.instance.GetCharEmoToInt(A.ItemId) < StaticManager.instance.GetCharEmoToInt(B.ItemId) ? 1 : (
							StaticManager.instance.GetCharEmoToInt(A.ItemId) > StaticManager.instance.GetCharEmoToInt(B.ItemId) ? -1 : 0);
					case SORT_TYPE.LEVEL:
						return 0; //not yet
					default:
						return 0;
				}
			});

			foreach (ItemInstance item in itemValue)
			{
				AddIcon(index++, item);
			}
		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
		});
	}

    // Update is called once per frame
    void AddIcon(int index, ItemInstance item)
	{
		var icon = Resources.Load<GameObject>("Prefabs/Icon");
		var IconObj = Instantiate(icon, new Vector2(80, -80), Quaternion.identity) as GameObject;
		IconObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
		IconObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
		IconObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
		IconObj.transform.SetParent(mScrollBackground.transform ,false);
		var prev = IconObj.transform.localPosition;
		float init_x = prev.x;
		while (index > 0)
		{
			prev.x += 120;
			if(rect.width < prev.x + 120)
			{
				prev.x = init_x;
				prev.y -= 120;
			}
			index--;
		}
		IconObj.transform.localPosition = prev;

		if (rect.height < (prev.y-80) * -1)
		{
			mScrollBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (prev.y - 80) * -1);
		}

		StaticManager.instance.SettingIconImg(item.ItemId, IconObj);
	}
	public void ChangeSort(Dropdown target)
	{
		int value = target.value;
		Debug.Log("value = "+value);
		TableRefresh((SORT_TYPE)value);
	}
}
