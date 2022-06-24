using UnityEngine;
using System.IO;

public class HexUnit : MonoBehaviour {

	public static HexUnit unitPrefab;

	//New variables
	public bool canFormCity; //Set this to true in inspector if you want the unit to be able to form a city
	public City cityPrefab; //Attach the relevant city prefab in the inspector
	[HideInInspector]
	public int unitTypeIndex; //The unit type

	public HexCell Location {
		get {
			return location;
		}
		set {
			if (location) {
				location.Unit = null;
			}
			location = value;
			value.Unit = this;
			transform.localPosition = value.Position;
		}
	}

	HexCell location;

	public float Orientation {
		get {
			return orientation;
		}
		set {
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	float orientation;

	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public bool IsValidDestination (HexCell cell) {
		return !cell.IsUnderwater && !cell.Unit;
	}

	public void Die () {
		location.Unit = null;
		Destroy(gameObject);
	}


	public void Save (BinaryWriter writer) {
		location.coordinates.Save(writer);
		writer.Write(orientation);
	}

	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		float orientation = reader.ReadSingle();
		grid.AddUnit(
			Instantiate(unitPrefab), grid.GetCell(coordinates), orientation
		);
	}
}