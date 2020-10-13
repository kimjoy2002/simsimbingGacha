using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChracterListManager : MonoBehaviour
{
	public GameObject mScrollRect;
	public GameObject mScrollBackground;

	private Rect rect;

	// Start is called before the first frame update
	void Start()
    {
		rect = mScrollRect.GetComponent<RectTransform>().rect;
		mScrollBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rect.height);
		var request = new GetUserInventoryRequest();
		PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest
		{
		}, (result) =>
		{
			int index = 0;
			foreach (ItemInstance item in result.Inventory)
			{
				if(item.CatalogVersion == "character")
				{
					AddIcon(index++, item);
				}
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
		var IconObj = Instantiate(icon, mScrollBackground.transform) as GameObject;
		IconObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
		IconObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
		IconObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

		var prev = IconObj.transform.position;
		float init_x = prev.x;
		while (index > 0)
		{
			prev.x += 120;
			if(rect.width < prev.x)
			{
				prev.x = init_x;
				prev.y -= 120;
			}
			index--;
		}
		IconObj.transform.position = prev;

		Debug.Log("Character/Face/" + item.ItemId);

		Sprite img = Resources.Load<Sprite>("Character/Face/" + item.ItemId);
		Debug.Log("Sprite:" + img);
		IconObj.GetComponent<Image>().sprite = img;
	}
}
