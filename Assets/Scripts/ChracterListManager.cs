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
	enum SORT_TYPE
	{
		NEW,
		OLD,
		RARE_DES,
		RARE_ASC,
		EMO,
		LEVEL
	};

	public GameObject mScrollRect;
	public GameObject mScrollBackground;
	public bool isCharacterSelect;
	public List<GameObject> mBoxList;

	private List<GameObject> mCharacter = new List<GameObject>(4);
	private List<int> orderLayer = new List<int>(4);
	private Rect rect;
	private int currentBox = -1;

	 // Start is called before the first frame update
	 void Start()
	{
		rect = mScrollRect.GetComponent<RectTransform>().rect;
		mScrollBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rect.height);
		TableRefresh(SORT_TYPE.NEW);
		for(int i =0; i < 4;i ++)
		{
			mCharacter.Add(null);
		}
		orderLayer.Add(2);
		orderLayer.Add(1);
		orderLayer.Add(4);
		orderLayer.Add(3);
	}
	void Update()
	{
		if(mBoxList != null && mBoxList.Count > 0)
		{
			if(Input.GetMouseButton(0))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

				if (hit.collider != null)
				{
					int colliderBox = -1;
					for(int i = 0; i < 4; i++)
					{
						if(mBoxList[i].transform == hit.collider.transform)
						{
							colliderBox = i;
						}
					}
					if (currentBox >= 0)
					{
						SpriteRenderer spr = mBoxList[currentBox].GetComponent<SpriteRenderer>();
						Color color = spr.color;
						color.a = 0f;
						spr.color = color;
					}

					if (colliderBox >= 0)
					{
						currentBox = colliderBox;
						SpriteRenderer spr = mBoxList[currentBox].GetComponent<SpriteRenderer>();

						Color color = spr.color;
						color.a = 255f;
						spr.color = color;
					}
				}
			}

		}
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
				switch (sortType)
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
		IconObj.transform.SetParent(mScrollBackground.transform, false);
		var prev = IconObj.transform.localPosition;
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

		StaticManager.instance.SettingIconImg(item.ItemId, IconObj);
		IconObj.name = item.ItemId;
		IconObj.GetComponent<Button>().onClick.AddListener(() => ClickButton(IconObj));
	}
	public void ChangeSort(Dropdown target)
	{
		int value = target.value;
		TableRefresh((SORT_TYPE)value);
	}

	void ClickButton(GameObject button)
	{
		if (isCharacterSelect && currentBox >= 0)
		{
			bool general = false;
			if (mCharacter[currentBox] != null)
			{
				Destroy(mCharacter[currentBox]);
			}
			var baseCh = Resources.Load<GameObject>("Prefabs/Character/" + button.name);
			if (baseCh == null)
			{
				baseCh = Resources.Load<GameObject>("Prefabs/Character/general");
				general = true;
			}

			Vector2 vec = mBoxList[currentBox].transform.position;
			vec.y += 90;
			mCharacter[currentBox] = Instantiate(baseCh, vec, Quaternion.identity) as GameObject;
			mCharacter[currentBox].transform.localScale = new Vector2(75, 75);
			mCharacter[currentBox].GetComponent<SpriteRenderer>().sortingOrder = orderLayer[currentBox];


			var head = mCharacter[currentBox].transform.Find("head");
			if (head != null)
			{
				var headRanderer = head.gameObject.GetComponent<SpriteRenderer>();
				headRanderer.sortingOrder = orderLayer[currentBox];
				if (general)
				{
					var headImg = Resources.Load<Sprite>("Character/Face/" + button.name);
					if (headImg)
					{
						headRanderer.sprite = headImg;
					}
				}
			}

		}
	}


	public void ChangePosition()
	{
		foreach (var chars in mCharacter)
		{
			if (chars != null)
			{
				chars.GetComponent<Animator>().SetBool("isMove", UnityEngine.Random.Range(0, 2) == 1 ? true : false);
				chars.GetComponent<Animator>().SetBool("isAttack", UnityEngine.Random.Range(0, 2) == 1 ? true : false);
				chars.GetComponent<Animator>().SetBool("isAttacked", UnityEngine.Random.Range(0, 2) == 1 ? true : false);
			}
		}
	}
}
