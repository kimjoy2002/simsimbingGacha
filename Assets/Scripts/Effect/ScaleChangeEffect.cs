using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleChangeEffect : MonoBehaviour
{
	int paral = 0;

	public float time;
	public float mStartSize, mEndSize;

	// Start is called before the first frame update
	void Start()
    {
        
    }
	IEnumerator ScaleChangeAnimation(float time, int paral_this)
	{
		float i = 0;
		float rate = 1 / time;

		Vector3 fromScale = new Vector3(mStartSize, mStartSize);
		Vector3 toScale = new Vector3(mEndSize, mEndSize);
		while (i < 1 && paral_this == paral)
		{
			i += Time.deltaTime * rate;

			//transform.localScale = Mathf.SmoothDamp(mStartSize, mEndSize, ref mMoveSpeed, SMOOTH_TIME);

			//float prec = Mathf.Sin(i / time * Mathf.PI * 0.5f) ;
			//prec *= prec * prec * i;
			float t = i / time;
			float prec = 1 - (1 - t) * (1 - t) * (1 - t);
			transform.localScale = Vector3.Lerp(fromScale, toScale, prec* time);
			yield return 0;
		}
	}

	public void StartAnimation()
	{
		Vector3 fromScale = new Vector3(mStartSize, mStartSize);
		transform.localScale = fromScale;
		StartCoroutine(ScaleChangeAnimation(time, ++paral));
	}
}
