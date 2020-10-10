using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;


public class NetworkManager : Photon.MonoBehaviour
{
	public Text stausText;
	public InputField id, password;

	// Update is called once per frame
	void Update() => stausText.text = PhotonNetwork.NetworkStatisticsToString();


	public void login()
	{


	}

	public void signUp()
	{


	}
}
