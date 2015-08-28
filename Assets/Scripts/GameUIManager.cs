using UnityEngine;
using System.Collections;

public class GameUIManager : MonoBehaviour {

	public CardControl cardPrefab;
	SingleGame.GameManager _game;
	//SingleGame
	public RectTransform [] handSlots=new RectTransform[7];
	public RectTransform deck;
	public RectTransform flipper;

	// Use this for initialization
	void Start () {
		_game=new SingleGame.GameManager();

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
