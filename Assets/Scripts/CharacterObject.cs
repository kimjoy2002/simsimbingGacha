using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterObject
{
	public bool isLeft = false;
	public GameObject mGameObject;
	public Vector2 mStartPosition;
	public Vector2 mAttackedPosition;



	public void SetObject(GameObject mGameObject, bool isLeft)
	{
		this.mGameObject = mGameObject;
		this.isLeft = isLeft;

		mStartPosition = mGameObject.transform.position;
		mAttackedPosition = mGameObject.transform.position
			+ (isLeft?new Vector3(-100, 0): new Vector3(100, 0));
	}
}
