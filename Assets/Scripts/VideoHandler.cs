using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoHandler : MonoBehaviour
{
	public RuntimeAnimatorController[] mAnimatorList;
	public int[] mTouchSlice;
	public Animator mAnimator = null;
	public GameObject gachaButtion;
	public Image tempGachaPick;

	public bool resultWindow = false, end = false;
	private int randomCnt;
	private Vector2 touchPos, nowPos;

	void Start()
	{
		randomCnt = Random.Range(0, mAnimatorList.Length);
		mAnimator.runtimeAnimatorController = mAnimatorList[randomCnt];

		RectTransform rectTransform = gachaButtion.transform.GetChild(0).GetComponent<RectTransform>();

		rectTransform.Rotate(new Vector3(0, 0, 90* mTouchSlice[randomCnt]+90));


		gachaButtion.gameObject.SetActive(true);
	}


	public void Update()
	{
		if (Input.GetMouseButton(0))
		{
			nowPos = (Input.touchCount == 0) ? (Vector2)Input.mousePosition : Input.GetTouch(0).position;

			if (Input.GetMouseButtonDown(0))
			{
				touchPos = nowPos;
			}
		}

		if (Input.GetMouseButtonUp(0))  //터치 끝
		{
			if(end == true)
			{
				UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Lobby");
			}
			else
			{
				Vector2 diff = touchPos - nowPos;
				if ((mTouchSlice[randomCnt] == 0 && diff.x > 100)
					|| (mTouchSlice[randomCnt] == 1 && diff.x < -100)
					|| (mTouchSlice[randomCnt] == 2 && diff.y > 100)
					|| (mTouchSlice[randomCnt] == 3 && diff.y < -100)
					)
				{
					mAnimator.SetBool("isPlay", true);
					gachaButtion.gameObject.SetActive(false);
				}
			}
		}

		if (resultWindow == false && mAnimator.GetCurrentAnimatorStateInfo(0).IsName("gacha") &&
			mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 
			&& !mAnimator.IsInTransition(0)
			&& mAnimator.GetCurrentAnimatorStateInfo(0).length >
				   mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime)
		{
			mAnimator.speed = 0.0f;
			resultWindow = true;
			var catalogRequest = new GetCatalogItemsRequest() { CatalogVersion = "character" };
			PlayFabClientAPI.GetCatalogItems(catalogRequest, (result) =>
			{
				Debug.Log("result.Catalog.Count : " + result.Catalog.Count);
				if (result.Catalog.Count > 0)
				{
					int random = Random.Range(0, result.Catalog.Count - 1);
					Debug.Log("Debuga : " + random);
					CatalogItem item = result.Catalog[random];
					Debug.Log("get gacha : " + item.ItemId);
					var itemRequest = new PurchaseItemRequest()
					{
						CatalogVersion = item.CatalogVersion,
						ItemId = item.ItemId,
						VirtualCurrency = "SB",
						Price = (int)item.VirtualCurrencyPrices["SB"],
					};
					PlayFabClientAPI.PurchaseItem(itemRequest, (itemResult) =>
					{
						Sprite img = Resources.Load<Sprite>("Character/Face/" + item.ItemId);
						tempGachaPick.sprite = img;
						tempGachaPick.gameObject.SetActive(true);
						end = true;
					}
					, (error) =>
					{
						end = true;
						Debug.Log(error.GenerateErrorReport());
					}
					);
				}

			}, (error) =>
			{
				end = true;
				Debug.Log(error.GenerateErrorReport());
			});			
		}
	}
}