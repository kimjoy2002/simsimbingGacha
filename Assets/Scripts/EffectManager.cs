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

	void PlayParticle(Vector2 position, string effName, float startDelay)
	{
		var paticle = Resources.Load<GameObject>("Prefabs/Effect/" + effName);
		var patiObj = Instantiate(paticle, position, Quaternion.identity) as GameObject;

		foreach (Transform child in patiObj.transform)
		{
			ParticleSystem parti = child.GetComponent<ParticleSystem>();
			parti.startDelay = startDelay;
		}
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

	float getAnimationClipLength(Animator anim, string clipName)
	{
		float length = 10;
		if (anim != null)
		{
			
			RuntimeAnimatorController ac = anim.runtimeAnimatorController;

			AnimationClip[] clips = ac.animationClips;

			foreach (AnimationClip clip in clips)
			{
				if (clip.name.EndsWith(clipName))
				{
					length = clip.length;
				}

			}
		}
		return length;
	}

	public IEnumerator AttackToObject(CharacterObject attackerObj, CharacterObject defenderObj, Action func)
	{
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

		float attack_time = getAnimationClipLength(animator, "attack");

		PlayParticle(defenderObj.mGameObject.transform.position, "FX_HIT_01", 0.83f);
		CameraShake(0.1f, 20.0f, 0.83f);
		PlayParticle(defenderObj.mGameObject.transform.position, "FX_HIT_01", 1.0f);
		CameraShake(0.1f, 20.0f, 1.0f);
		yield return new WaitForSeconds(attack_time);

		animator.SetBool("isAttack", false);
		defender.GetComponent<Animator>().SetBool("isAttacked", false);

		yield return JumpToOriginPos(attackerObj, 1, 100);

		func();
	}
}
