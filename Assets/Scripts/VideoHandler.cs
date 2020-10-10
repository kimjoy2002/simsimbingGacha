using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoHandler : MonoBehaviour
{
	public RawImage mScreen = null;
	public VideoPlayer mVideoPlayer = null;
	public Button gachaButtion;

	private bool prepare = false;

	void Start()
	{
		if (mScreen != null && mVideoPlayer != null)
		{
			// 비디오 준비 코루틴 호출
			StartCoroutine(PrepareVideo("gacha00.mp4"));
		}
	}

	void Update()
	{
		if (prepare)
		{
			if (mVideoPlayer.isPlaying == false)
			{
				UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Lobby");
			}
		}

	}

	protected IEnumerator PrepareVideo(string mediaFileName)
	{
		string streamingMediaPath = Application.streamingAssetsPath + "/" + mediaFileName;
		string persistentPath = Application.persistentDataPath + "/" + mediaFileName;
		Debug.Log("streamingMediaPath"+ streamingMediaPath);
		Debug.Log("persistentPath"+ persistentPath);

		if (!File.Exists(persistentPath))
		{
			mVideoPlayer.url = streamingMediaPath;
		}
		else
		{
			mVideoPlayer.url = persistentPath;
		}


		mVideoPlayer.Prepare();
		// 비디오가 준비되는 것을 기다림
		while (!mVideoPlayer.isPrepared)
		{
			yield return new WaitForSeconds(0.5f);
		}

		gachaButtion.gameObject.SetActive(true);
		mVideoPlayer.time = 1;
		// VideoPlayer의 출력 texture를 RawImage의 texture로 설정한다
		mScreen.texture = mVideoPlayer.texture;
		mVideoPlayer.Play();
		mVideoPlayer.Pause();
	}

	public void PlayVideo()
	{
		prepare = true;
		gachaButtion.gameObject.SetActive(false);
		if (mVideoPlayer != null && mVideoPlayer.isPrepared)
		{
			// 비디오 재생
			mVideoPlayer.Play();
		}
	}

	public void StopVideo()
	{
		if (mVideoPlayer != null && mVideoPlayer.isPrepared)
		{
			// 비디오 멈춤
			mVideoPlayer.Stop();
		}
	}
}