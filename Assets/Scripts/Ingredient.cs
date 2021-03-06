﻿using System.IO;
using System.Collections;
using UnityEngine;

public class IngredientSprites {
	private Object[] sprites;

	public IngredientSprites() {
		sprites = Resources.LoadAll("IngredientSprites", typeof(Sprite));
	}

	public Sprite GetSpriteFromName(string name) {
		foreach (Sprite s in sprites) {
			if (s.name == name) {
				return s;
			}
		}

		return null;
	}
}

[System.Serializable]
public class IngredientData {
	private static IngredientData[] allIngredients;
	private static int counter = -1;

	public string name;
	public Buffs buff;

	public static IngredientData GetRandomIngredient() {
		if (allIngredients == null) {
			allIngredients = AllIngredients.GetAllIngredients ();
		}
		counter = (counter + 1) % 2;

		return allIngredients [counter];
	}
}

[System.Serializable]
public class AllIngredients {
	public IngredientData[] ingredients;

	public static IngredientData[] GetAllIngredients() {
		string dataAsJson = File.ReadAllText ("Assets/JSON/ingredients.json"); 
		AllIngredients ret = JsonUtility.FromJson<AllIngredients> (dataAsJson);
		return ret.ingredients;
	}
}

public class Ingredient : Draggable {
	
	public static IngredientSprites ingredientSprites;
	public float turningFrequency = 6f;
	public float turningAllowance = 2f;

	private IngredientData data;
	private Quaternion normalAngle;
	private bool turningRight = false;
	private float rotationTolerance = 0f;
	private SpriteRenderer spriteRenderer;
	private string originalSortingLayerName;

	private float negativeTurningAllowance;
	private float turningFrequencyTimesTurningAllowance;

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		if (ingredientSprites == null) {
			ingredientSprites = new IngredientSprites ();
		}

		spriteRenderer = this.GetComponent <SpriteRenderer> ();
		originalSortingLayerName = spriteRenderer.sortingLayerName;

		this.SetData ();
		transform.rotation = normalAngle;

		negativeTurningAllowance = turningAllowance * -1f;
		turningFrequencyTimesTurningAllowance = turningFrequency * turningAllowance;
	}
	
	// Update is called once per frame
	void Update () {
		if (clickedOn) {
			if (turningRight) {
				float toRotate = turningFrequencyTimesTurningAllowance * Time.deltaTime * -1f;
				transform.Rotate (new Vector3(0f, 0f, toRotate));
				rotationTolerance += toRotate;
				if (rotationTolerance <= negativeTurningAllowance) {
					turningRight = false;
				}
			} else {
				float toRotate = turningFrequencyTimesTurningAllowance * Time.deltaTime;
				transform.Rotate (new Vector3(0f, 0f, toRotate));
				rotationTolerance += toRotate;
				if (rotationTolerance >= turningAllowance) {
					turningRight = true;
				}
			}
			if (spriteRenderer.sortingLayerName == originalSortingLayerName) {
				spriteRenderer.sortingLayerName = "Interactables";
			}
		}
	}

	protected override void OnMouseUp () {
		transform.rotation = normalAngle;
		spriteRenderer.sortingLayerName = originalSortingLayerName;
		base.OnMouseUp ();
	}

	private void SetData() {
		data = IngredientData.GetRandomIngredient ();
		this.spriteRenderer.sprite = ingredientSprites.GetSpriteFromName (data.name);
		Vector3 newSize = spriteRenderer.sprite.bounds.size;
		GetComponent <BoxCollider2D> ().size = new Vector2 (newSize.x, newSize.y);
	}

	protected override void DroppedOn (Mixing other) {
		other.Drop (this.data);
		this.SetData ();
		this.ResetPosition ();
	}
}
