using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
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
	public GameObject tempGachaPick;
	public GameObject gachaWindow;

	public int requestGacha = -1;
	public int remainGacha = 1;
	public bool resultWindow = false, start = false, end = false;
	private int randomCnt;
	private Vector2 touchPos, nowPos;
	private List<string> pickList = new List<string> ();


	public class GachaResult
	{
		public string Result;
		public List<string> ItemId;
	}


	void Start()
	{
		remainGacha = StaticManager.instance.GachaNum;
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
			if (end == true)
			{
			}
			else if (requestGacha == -1)
			{
				Vector2 diff = touchPos - nowPos;
				if ((mTouchSlice[randomCnt] == 0 && diff.x > 100)
					|| (mTouchSlice[randomCnt] == 1 && diff.y > 100)
					|| (mTouchSlice[randomCnt] == 2 && diff.x < -100)
					|| (mTouchSlice[randomCnt] == 3 && diff.y < -100)
					)
				{
					mAnimator.SetBool("isPlay", true);
					gachaButtion.gameObject.SetActive(false);
					requestGacha = remainGacha;
					var scriptRequest = new ExecuteCloudScriptRequest()
					{
						FunctionName = "Gacha",
						FunctionParameter = new { page = StaticManager.instance.GachaPage, num = requestGacha },
					};

					PlayFabClientAPI.ExecuteCloudScript(scriptRequest, (result) =>
					{
						Debug.Log(PlayFab.Json.PlayFabSimpleJson.SerializeObject(result));


						JsonObject jsonResult = (JsonObject)result.FunctionResult;

						Debug.Log(jsonResult.ToString());

						GachaResult result_ = JsonUtility.FromJson<GachaResult>(jsonResult.ToString());

						foreach (string key in result_.ItemId)
						{
							pickList.Add(key);
						}
						//KeyValuePair<string, JsonObject> entry = new KeyValuePair<string, JsonObject>((string)ItemId, CustomObj);
						//pickList.Add(entry);
						StaticManager.instance.mCharacterLocalDataChanged = true;
						requestGacha = 0;
					}, (error) =>
					{
							requestGacha = 0;
							end = true;
							Debug.Log(error.GenerateErrorReport());
					});
					UiManager.instance.Crystal -= requestGacha*100;
				}
			}
			else if (requestGacha == 0 && resultWindow == true)
			{
				remainGacha--;
				if (remainGacha == 0)
				{
					for (int i = 0; i < pickList.Count; i++)
					{
						AddIcon(i, pickList[i]);
						end = true;
					}
					gachaWindow.gameObject.SetActive(true);
				}
				else
				{
					StaticManager.instance.SettingIconImg(pickList[pickList.Count - remainGacha], tempGachaPick);
					tempGachaPick.gameObject.SetActive(true);
					tempGachaPick.GetComponent<ScaleChangeEffect>().StartAnimation();

					var paticle = Resources.Load<GameObject>("Prefabs/GachaPaticle");
					var patiObj = Instantiate(paticle, new Vector2(), Quaternion.identity) as GameObject;
				}
			}
		}

		if (start == false && mAnimator.GetCurrentAnimatorStateInfo(0).IsName("gacha") &&
			mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1
			&& !mAnimator.IsInTransition(0)
			&& mAnimator.GetCurrentAnimatorStateInfo(0).length >
				   mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime)
		{
			mAnimator.speed = 0.0f;
			start = true;
		}
		if (start == true && resultWindow == false && requestGacha == 0)
		{
			resultWindow = true;

			StaticManager.instance.SettingIconImg(pickList[0], tempGachaPick);
			tempGachaPick.gameObject.SetActive(true);
			tempGachaPick.GetComponent<ScaleChangeEffect>().StartAnimation();

			var paticle = Resources.Load<GameObject>("Prefabs/GachaPaticle");
			var patiObj = Instantiate(paticle, new Vector2(), Quaternion.identity) as GameObject;
		}
	}



	void AddIcon(int index, string itemId)
	{
		Rect rect = gachaWindow.GetComponent<RectTransform>().rect;
		float assumeSizeX = rect.width / 5;
		float assumeSizeY = rect.height / 5;

		var icon = Resources.Load<GameObject>("Prefabs/Icon");
		var IconObj = Instantiate(icon, new Vector2(assumeSizeX/2, -assumeSizeY), Quaternion.identity) as GameObject;
		IconObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
		IconObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
		IconObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
		IconObj.transform.SetParent(gachaWindow.transform, false);
		var prev = IconObj.transform.localPosition;

		prev.x = prev.x + (index % 5) * assumeSizeX;
		prev.y = prev.y - (index / 5) * assumeSizeX;
		IconObj.transform.localPosition = prev;

		StaticManager.instance.SettingIconImg(itemId, IconObj);
	}
}