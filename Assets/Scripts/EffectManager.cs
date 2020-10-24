using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
	public Camera mainCamera;

	private Vector2 cameraOrigin;

	void Start()
	{
		cameraOrigin = mainCamera.transform.position;
	}

	IEnumerator JumpToObject(CharacterObject attackerObj, CharacterObject defenderObj, float moveTime, float parabolaHeight)
	{
		GameObject attacker = attackerObj.mGameObject;
		GameObject mGameObject = defenderObj.mGameObject;

		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 384,
			"to", 200,
			"time", moveTime,
			"onUpdate", "CameraWork",
			"easetype", iTween.EaseType.easeOutCirc
			));
		iTween.MoveTo(mainCamera.gameObject, iTween.Hash(
			"x", defenderObj.mAttackedPosition.x,
			"y", defenderObj.mAttackedPosition.y,
			"time", moveTime,
			"easetype", iTween.EaseType.easeOutCirc
		));

		float diff = parabolaHeight - (defenderObj.mAttackedPosition.y - attacker.transform.position.y);
		iTween.MoveTo(attacker, iTween.Hash(
			"x", defenderObj.mAttackedPosition.x,
			"y", defenderObj.mAttackedPosition.y,
			"time", moveTime,
			"easetype", iTween.EaseType.easeOutCirc
			));
		yield return new WaitForSeconds(moveTime);
	}

	IEnumerator JumpToOriginPos(CharacterObject attackerObj, float moveTime, float parabolaHeight)
	{
		GameObject attacker = attackerObj.mGameObject;

		iTween.ValueTo(gameObject, iTween.Hash(
			"from", 200,
			"to", 384,
			"time", moveTime,
			"onUpdate", "CameraWork",
			"easetype", iTween.EaseType.easeOutCirc
		));
		iTween.MoveTo(mainCamera.gameObject, iTween.Hash(
			"x", cameraOrigin.x,
			"y", cameraOrigin.y,
			"time", moveTime,
			"easetype", iTween.EaseType.easeOutCirc
		));

		iTween.MoveTo(attacker, iTween.Hash(
			"x", attackerObj.mStartPosition.x,
			"y", attackerObj.mStartPosition.y,
			"time", moveTime,
			"easetype", iTween.EaseType.easeOutCirc
		));
		yield return new WaitForSeconds(moveTime);
	}


	void CameraShake(float shakeTime, float shakePower, float delay = 0.0f)
	{
		iTween.ShakePosition(mainCamera.gameObject, iTween.Hash(
			"x", shakePower,
			"y", shakePower,
			"delay", delay,
			"time", shakeTime
		));
	}

	void CameraWork(int size)
	{
		mainCamera.orthographicSize = size;
	}

	public IEnumerator AttackToObject(CharacterObject attackerObj, CharacterObject defenderObj, Action func)
	{
		IEnumerator iter;
		GameObject attacker = attackerObj.mGameObject;
		GameObject defender = defenderObj.mGameObject;
		if (attacker == null || defender == null)
		{
			yield break;
		}


		yield return JumpToObject(attackerObj, defenderObj, 1, 200);


		var animator = attacker.GetComponent<Animator>();
		animator.SetBool("isAttack", true);
		defender.GetComponent<Animator>().SetBool("isAttacked", true);

	    CameraShake(0.1f, 20.0f,1.0f);
		yield return new WaitForSeconds(3.0f);

		animator.SetBool("isAttack", false);
		defender.GetComponent<Animator>().SetBool("isAttacked", false);

		yield return JumpToOriginPos(attackerObj, 1, 100);

		func();
	}
}
