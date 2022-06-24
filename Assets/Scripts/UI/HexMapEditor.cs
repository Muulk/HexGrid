using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.IO;

public class HexMapEditor : MonoBehaviour {

	public HexGrid hexGrid;

	public Material terrainMaterial;

	//Unit data
	public HexUnit unitPrefab;
	public HexUnit[] unitArray; //An array with all the different unit prefabs
	int unitIndex = 0;


	int activeElevation;
	int activeWaterLevel;

	int activeUrbanLevel, activeFarmLevel, activePlantLevel, activeSpecialIndex;

	int activeTerrainTypeIndex;

	int brushSize;


	bool applyElevation = true;
	bool applyWaterLevel = true;

	bool applyUrbanLevel, applyFarmLevel, applyPlantLevel, applySpecialIndex;

	enum OptionalToggle {
		Ignore, Yes, No
	}

	OptionalToggle riverMode, roadMode, walledMode;

	bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;

	public void SetTerrainTypeIndex (int index) {
		activeTerrainTypeIndex = index;
	}

	public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}

	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	public void SetApplyWaterLevel (bool toggle) {
		applyWaterLevel = toggle;
	}

	public void SetWaterLevel (float level) {
		activeWaterLevel = (int)level;
	}

	public void SetApplyUrbanLevel (bool toggle) {
		applyUrbanLevel = toggle;
	}

	public void SetUrbanLevel (float level) {
		activeUrbanLevel = (int)level;
	}

	public void SetApplyFarmLevel (bool toggle) {
		applyFarmLevel = toggle;
	}

	public void SetFarmLevel (float level) {
		activeFarmLevel = (int)level;
	}

	public void SetApplyPlantLevel (bool toggle) {
		applyPlantLevel = toggle;
	}

	public void SetPlantLevel (float level) {
		activePlantLevel = (int)level;
	}

	public void SetApplySpecialIndex (bool toggle) {
		applySpecialIndex = toggle;
	}

	public void SetSpecialIndex (float index) {
		activeSpecialIndex = (int)index;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void SetRiverMode (int mode) {
		riverMode = (OptionalToggle)mode;
	}

	public void SetRoadMode (int mode) {
		roadMode = (OptionalToggle)mode;
	}

	public void SetWalledMode (int mode) {
		walledMode = (OptionalToggle)mode;
	}

	public void SetEditMode (bool toggle) {
		enabled = toggle;
	}

	public void ShowGrid (bool visible) {
		if (visible) {
			terrainMaterial.EnableKeyword("GRID_ON");
		}
		else {
			terrainMaterial.DisableKeyword("GRID_ON");
		}
	}

	void Awake () {
		terrainMaterial.DisableKeyword("GRID_ON");
		SetEditMode(false);
	}

	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (Input.GetMouseButton(0)) {
				HandleInput();
				return;
			}
			if (Input.GetKeyDown(KeyCode.U)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					DestroyUnit();
				}
				else {
					CreateUnit();
				}
				return;
			}

			if (Input.GetKeyDown(KeyCode.B))
            {
				CreateCity();
				return;
            }

			if (Input.GetKeyDown(KeyCode.N))
            {
				Debug.Log("Train Unit");
				TrainUnit();
				return;
            }

		}

		//Unit Selection Input
		if (Input.GetKeyDown(KeyCode.Alpha1))
        {
			unitIndex = 0;
        } else if (Input.GetKeyDown(KeyCode.Alpha2)){
			unitIndex = 1;

		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			unitIndex = 2;

		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			unitIndex = 3;

		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			unitIndex = 4;

		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			unitIndex = 5;

		}
		else if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			unitIndex = 6;

		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			unitIndex = 7;

		}
		else if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			unitIndex = 8;

		}
		else if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			unitIndex = 9;
		}

		previousCell = null;
	}

	HexCell GetCellUnderCursor () {
		return
			hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
	}

	void CreateUnit () {
		HexCell cell = GetCellUnderCursor();
		if (cell && !cell.Unit) {
			hexGrid.AddUnit(
				Instantiate(unitArray[unitIndex]), cell, Random.Range(0f, 360f)
			);
			cell.Unit.unitTypeIndex = unitIndex;
		}
	}

	void DestroyUnit () {
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Unit) {
			hexGrid.RemoveUnit(cell.Unit);
		}
	}

	//Creates a city
	void CreateCity()
    {
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Unit && cell.Unit.canFormCity){
			hexGrid.AddCity(cell.Unit, cell,
				Instantiate(cell.Unit.cityPrefab));
			cell.city.cityTypeIndex = cell.Unit.unitTypeIndex; //Sets the city type to match the unit that is consumed
			hexGrid.RemoveUnit(cell.Unit);
		}
    }

	//Follows the same logic as AddUnit() but adjusted for flexibility
	void TrainUnit()
    {
		HexCell cell = GetCellUnderCursor();
		unitIndex = cell.city.cityTypeIndex;
		if (cell && !cell.Unit)
		{
			hexGrid.AddUnit(
				Instantiate(unitArray[unitIndex]), cell, Random.Range(0f, 360f)
			);
		}
	}

	void HandleInput () {
		HexCell currentCell = GetCellUnderCursor();
		if (currentCell) {
			if (previousCell && previousCell != currentCell) {
				ValidateDrag(currentCell);
			}
			else {
				isDrag = false;
			}
			EditCells(currentCell);
			previousCell = currentCell;
		}
		else {
			previousCell = null;
		}
	}

	void ValidateDrag (HexCell currentCell) {
		for (
			dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
			dragDirection++
		) {
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
	}

	void EditCells (HexCell center) {
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;

		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
	}

	void EditCell (HexCell cell) {
		if (cell) {
			if (activeTerrainTypeIndex >= 0) {
				cell.TerrainTypeIndex = activeTerrainTypeIndex;
			}
			if (applyElevation) {
				cell.Elevation = activeElevation;
			}
			if (applyWaterLevel) {
				cell.WaterLevel = activeWaterLevel;
			}
			if (applySpecialIndex) {
				cell.SpecialIndex = activeSpecialIndex;
			}
			if (applyUrbanLevel) {
				cell.UrbanLevel = activeUrbanLevel;
			}
			if (applyFarmLevel) {
				cell.FarmLevel = activeFarmLevel;
			}
			if (applyPlantLevel) {
				cell.PlantLevel = activePlantLevel;
			}
			if (riverMode == OptionalToggle.No) {
				cell.RemoveRiver();
			}
			if (roadMode == OptionalToggle.No) {
				cell.RemoveRoads();
			}
			if (walledMode != OptionalToggle.Ignore) {
				cell.Walled = walledMode == OptionalToggle.Yes;
			}
			if (isDrag) {
				HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
				if (otherCell) {
					if (riverMode == OptionalToggle.Yes) {
						otherCell.SetOutgoingRiver(dragDirection);
					}
					if (roadMode == OptionalToggle.Yes) {
						otherCell.AddRoad(dragDirection);
					}
				}
			}
		}
	}
}