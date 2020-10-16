using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaMenuManager : MonoBehaviour
{
	public ScrollRect mScrollRectComponent;
	public Scrollbar mScrollbar;
	public RectTransform contentPanel;
	public List<RectTransform> mContainer;

	private float mTargetValue;
	private bool mNeedMove = false;
	private const float MOVE_SPEED = 1F;
	private const float SMOOTH_TIME = 0.2F;
	private float mMoveSpeed = 0f;
	private int currentGachaPage = 0;

	public void OnPointerDown()
	{
		mNeedMove = false;
	}

	void Start()
	{
		currentGachaPage = 0;
	}

	public void OnPointerUp()
	{
		int count = mContainer.Count;
		float border_diff = 1.0f / count;
		float center_dif = 1.0f / (count - 1);

		float border = border_diff;
		float center = 0.0f;
		currentGachaPage = 0;

		while (count>0)
		{
			if (mScrollbar.value <= border || count == 1)
			{
				mTargetValue = center;
				break;
			}
			border += border_diff;
			center += center_dif;
			currentGachaPage++;
			count--;
		}

		mNeedMove = true;
		mMoveSpeed = 0;
	}



	// Update is called once per frame
	void Update()
    {
		if (mNeedMove)
		{
			if (Mathf.Abs(mScrollbar.value - mTargetValue) < 0.01f)
			{
				mScrollbar.value = mTargetValue;
				mNeedMove = false;
				return;
			}
			mScrollbar.value = Mathf.SmoothDamp(mScrollbar.value, mTargetValue, ref mMoveSpeed, SMOOTH_TIME);
		}
	}



	public void GachaOne()
	{
		StaticManager.instance.GachaPage = contentPanel.GetChild(currentGachaPage).gameObject.name;
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Gacha");
	}

	public void GachaTen()
	{


	}


}
