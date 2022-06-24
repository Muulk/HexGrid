using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
	[HideInInspector]
	public int cityTypeIndex;

	// Start is called before the first frame update

	public HexCell Location
	{
		get
		{
			return location;
		}
		set
		{
			if (location)
			{
				location.city = null;
			}
			location = value;
			value.city = this;
			transform.localPosition = value.Position;
		}
	}

	HexCell location;


    // Update is called once per frame
    void Update()
    {
        
    }
}
