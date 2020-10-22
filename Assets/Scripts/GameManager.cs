using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	private List<GameObject> mCharacter = new List<GameObject>(8);
	private List<ItemInstance> mCharacterInstance = new List<ItemInstance>(8);
	private List<int> orderLayer = new List<int>(8);


	private List<Vector2> characterPos = new List<Vector2>(8);
	private Dictionary<string, ItemInstance> characterInfoMap = new Dictionary<string, ItemInstance>();
	private bool characterListLoadingFinish = false;
	private GetUserDataResult partyResult = null;
	private bool ready = false;
	// Start is called before the first frame update
	void Start()
	{
		characterPos.Add(new Vector2(-450, -50));
		characterPos.Add(new Vector2(-300, 25));
		characterPos.Add(new Vector2(-300, -125));
		characterPos.Add(new Vector2(-150, -50));
		orderLayer.Add(2);
		orderLayer.Add(1);
		orderLayer.Add(4);
		orderLayer.Add(3);

		characterPos.Add(new Vector2(150, -50));
		characterPos.Add(new Vector2(300, 25));
		characterPos.Add(new Vector2(300, -125));
		characterPos.Add(new Vector2(450, -50));
		orderLayer.Add(2);
		orderLayer.Add(1);
		orderLayer.Add(4);
		orderLayer.Add(3);
		for (int i = 0; i < 8; i++)
		{
			mCharacter.Add(null);
			mCharacterInstance.Add(null);
		}

		GetPartyOnServer();
		GetCharacterDataList();
		CreateAiMonster();
	}

	public void GetPartyOnServer()
	{
		List<string> partyKey = new List<string>();
		partyKey.Add("Party1");
		partyKey.Add("Party2");
		partyKey.Add("Party3");
		partyKey.Add("Party4");

		var dataRequest = new GetUserDataRequest()
		{
			Keys = partyKey
		};

		PlayFabClientAPI.GetUserData(dataRequest, (result) =>
		{
			Debug.Log(PlayFab.Json.PlayFabSimpleJson.SerializeObject(result));
			partyResult = result;
			if (characterListLoadingFinish)
			{
				SettingParty();
			}
		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
		});
	}

	private void SettingParty()
	{
		foreach (var data in partyResult.Data)
		{
			int chracterPos = data.Key == "Party1" ? 0 :
						 data.Key == "Party2" ? 1 :
						 data.Key == "Party3" ? 2 :
						 data.Key == "Party4" ? 3 : -1;
			ItemInstance instance = characterInfoMap[data.Value.Value];

			SettingCharacter(chracterPos, instance);
		}
		partyResult = null;
		ready = true;
	}


	public void CreateAiMonster()
	{
		for (int i = 0; i < 4; i++)
		{
			ItemInstance tempItem = new ItemInstance();

			tempItem.ItemId = "hungrydog";
			SettingCharacter(i + 4, tempItem);
		}
	}



	void SettingCharacter(int position, ItemInstance item)
	{
		bool general = false;
		var baseCh = Resources.Load<GameObject>("Prefabs/Character/" + item.ItemId);
		if (baseCh == null)
		{
			baseCh = Resources.Load<GameObject>("Prefabs/Character/general");
			general = true;
		}

		Vector2 vec = characterPos[position];
		mCharacter[position] = Instantiate(baseCh, vec, Quaternion.identity) as GameObject;
		mCharacter[position].transform.localScale = new Vector2(75, 75);
		mCharacter[position].GetComponent<SpriteRenderer>().sortingOrder = orderLayer[position];

		mCharacterInstance[position] = item;
		if (position >= 4)
		{
			var temprot = mCharacter[position].transform.rotation;
			temprot.y = 180;
			mCharacter[position].transform.rotation = temprot;
		}



		var head = mCharacter[position].transform.Find("head");
		if (head != null)
		{
			var headRanderer = head.gameObject.GetComponent<SpriteRenderer>();
			headRanderer.sortingOrder = orderLayer[position];
			if (general)
			{
				var headImg = Resources.Load<Sprite>("Character/Face/" + item.ItemId);
				if (headImg)
				{
					headRanderer.sprite = headImg;
				}
			}
		}
	}



	private void GetCharacterDataList()
	{
		StaticManager.instance.GetCharacterDataList(
		(result) =>
		{
			foreach (var instance in result)
			{
				characterInfoMap.Add(instance.ItemInstanceId, instance);
			}

			characterListLoadingFinish = true;
			if (partyResult != null)
				SettingParty();

		}, (error) =>
		{
			Debug.Log(error.GenerateErrorReport());
		});
	}



	public void StartBattle(Button button)
	{
		if ((mCharacter[0] == null && mCharacter[1] == null && mCharacter[2] == null && mCharacter[3] == null) ||
			(mCharacter[4] == null && mCharacter[5] == null && mCharacter[6] == null && mCharacter[7] == null))
		{
			return;
		}
		if (button != null)
			button.gameObject.SetActive(false);


		int attacker = Random.Range(0, 4);
		int defender = Random.Range(4, 8);
		if (Random.Range(0, 2) == 0)
		{
			int temp = attacker;
			attacker = defender;
			defender = temp;
		}

		StartCoroutine(AttackToObject(attacker, defender));

	}

	IEnumerator AttackToObject(int attackerNum, int defenderNum)
	{
		var attacker = mCharacter[attackerNum];
		var defender = mCharacter[defenderNum];
		if (attacker == null || defender == null)
		{
			yield break;
		}

		var animator = attacker.GetComponent<Animator>();
		animator.SetBool("isAttack", true);


		Vector2 start = attacker.transform.position;
		Vector2 end = defender.transform.position;
		float t = 0f;


		while (true)
		{
			t += Time.deltaTime*1.5f;

			Vector2 current = Vector2.Lerp(start, end, t);

			attacker.transform.position = current;
			if (Vector2.Distance(current, end) < 50)
			{
				break;
			}
			yield return 0;
		}
		defender.GetComponent<Animator>().SetBool("isAttacked", true);

		for (int i = 0; i < 30; i++)
		{
			yield return 0;
		}

		animator.SetBool("isAttack", false);
		defender.GetComponent<Animator>().SetBool("isAttacked", false);

		Vector2 sstart = attacker.transform.position;
		t = 0f;
		while (true)
		{
			t += Time.deltaTime * 1.5f;

			Vector2 current = Vector2.Lerp(sstart, start, t);

			attacker.transform.position = current;
			if (Vector2.Distance(current, start) < 1)
			{
				break;
			}
			yield return 0;
		}


		attacker.transform.position = start;
		StartBattle(null);
	}
}
