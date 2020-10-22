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
	private List<ItemInstance> mCharacterInstance = new List<ItemInstance>(4);
	private List<int> orderLayer = new List<int>(4);
	private Rect rect;
	private int currentBox = -1;
	private Dictionary<GameObject, ItemInstance> characterInfoMap = new Dictionary<GameObject, ItemInstance>();



	private bool characterListLoadingFinish = false;
	private GetUserDataResult partyResult = null;

	// Start is called before the first frame update
	void Start()
	{
		rect = mScrollRect.GetComponent<RectTransform>().rect;
		mScrollBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rect.height);
		TableRefresh(SORT_TYPE.NEW);
		for(int i =0; i < 4;i ++)
		{
			mCharacter.Add(null);
			mCharacterInstance.Add(null);
		}
		orderLayer.Add(2);
		orderLayer.Add(1);
		orderLayer.Add(4);
		orderLayer.Add(3);
		if(isCharacterSelect)
		{
			GetPartyOnServer();
		}
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

	public void GetPartyOnServer()
	{
		List<string> partyKey = new List<string>();
		partyKey.Add("Party1");
		partyKey.Add("Party2");
		partyKey.Add("Party3");
		partyKey.Add("Party4");

		var dataRequest = new GetUserDataRequest()
		{
			Keys = partyKey
		};

		PlayFabClientAPI.GetUserData(dataRequest, (result) =>
		{
			Debug.Log("GetUserData complete");
			Debug.Log(PlayFab.Json.PlayFabSimpleJson.SerializeObject(result));
			partyResult = result;
			if (characterListLoadingFinish)
			{
				settingParty();
			}
		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
		});
	}

	private void settingParty()
	{
		foreach (var data in partyResult.Data)
		{
			currentBox = data.Key == "Party1" ? 0 :
						 data.Key == "Party2" ? 1 :
						 data.Key == "Party3" ? 2 :
						 data.Key == "Party4" ? 3 : -1;
			GameObject button = null;
			foreach (var buttonMap in characterInfoMap)
			{
				if (buttonMap.Value.ItemInstanceId == data.Value.Value)
				{
					button = buttonMap.Key;
				}
			}
			if (button != null)
			{
				ClickButton(button);
			}
		}
		currentBox = -1;
		partyResult = null;
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
			characterListLoadingFinish = true;

		    if(partyResult != null)
				settingParty();

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
		characterInfoMap.Add(IconObj, item);
	}
	public void ClearMenu()
	{
		Transform[] childList = mScrollBackground.GetComponentsInChildren<Transform>(true);
		if (childList != null && childList.Length > 1)
		{
			for (int i = 1; i < childList.Length; i++)
			{
				if (childList[i] != transform)
					Destroy(childList[i].gameObject);
			}
		}
	}
	public void ChangeSort(Dropdown target)
	{
		int value = target.value;
		characterInfoMap.Clear();
		ClearMenu();
		TableRefresh((SORT_TYPE)value);
	}

	void ClickButton(GameObject button)
	{
		if (isCharacterSelect && currentBox >= 0)
		{
			bool general = false;
			for(int i = 0; i < 4; i++)
			{
				if(mCharacterInstance[i] == characterInfoMap[button])
				{
					mCharacterInstance[i] = null;
					Destroy(mCharacter[i]);
					mCharacter[i] = null;
				}
			}
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

			mCharacterInstance[currentBox] = characterInfoMap[button];


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

	public void UpdateCharacterParty()
	{
		var dataRequest = new UpdateUserDataRequest()
		{
			Data = new Dictionary<string, string>() {
				{"Party1", mCharacterInstance[0] != null ? mCharacterInstance[0].ItemInstanceId : ""},
				{"Party2", mCharacterInstance[1] != null ? mCharacterInstance[1].ItemInstanceId : ""},
				{"Party3", mCharacterInstance[2] != null ? mCharacterInstance[2].ItemInstanceId : ""},
				{"Party4", mCharacterInstance[3] != null ? mCharacterInstance[3].ItemInstanceId : ""}
			},
		};

		PlayFabClientAPI.UpdateUserData(dataRequest, (result) =>
		{
			Debug.Log("complete");
		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
		});
	}




	public void ChangePosition()
	{
		UpdateCharacterParty();
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
