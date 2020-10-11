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
				Debug.Log("ts : " + nowPos.x + ", " + nowPos.y);
			}
		}

		if (Input.GetMouseButtonUp(0))  //터치 끝
		{
			Debug.Log("te : " + nowPos.x + ", " + nowPos.y);
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

		if(mAnimator.GetCurrentAnimatorStateInfo(0).IsName("gacha") &&
			mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 
			&& !mAnimator.IsInTransition(0)
			&& mAnimator.GetCurrentAnimatorStateInfo(0).length >
				   mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime)
		{
			mAnimator.speed = 0.0f;
			UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Lobby");
		}
	}
}