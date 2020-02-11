using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoiseTest;

public class MoreCubes : MonoBehaviour
{
	public GameObject betterCubeThanBefore;
	public OpenSimplexNoise whateverTheheckYouWantBasically;
	public long seed;
	public int range;
	public float scale;
	public float scale2;
	public double tempResult;
	public double rarity;
	
    // Start is called before the first frame update
    void Start() {
		seed = Random.Range(-99999, 99999);
		whateverTheheckYouWantBasically = new OpenSimplexNoise(seed);
		
		for (int x = -range; x <= range; x++) {
			for (int y = -range; y <= range; y++) {
				tempResult = whateverTheheckYouWantBasically.Evaluate(x/scale, y/scale) + 1;
				tempResult /= 2;
				tempResult *= (whateverTheheckYouWantBasically.Evaluate(x/scale2, y/scale2) + 1.25)/2;
				if (tempResult <= rarity) {
					Instantiate(betterCubeThanBefore, new Vector3(3*x, 3*y, 2), Quaternion.identity);
				}
			}
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
