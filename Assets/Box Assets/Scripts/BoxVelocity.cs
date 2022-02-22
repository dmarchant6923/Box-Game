using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxVelocity : MonoBehaviour
{
    //This is a list of velocities that will be summed to determine the player's resultant velocity in the x direction.
    //This is so that different factors can apply at once in an additive manner.
    //For example, moving right at a velocity of +15 while on a platform moving left at -3 will result in the player moving right at +12.
    // index [0]: all player-caused velocities
    // index [1]: platform-caused velocities
    // index [2]: enemy-caused velocities such as push back from occupying the same space or an attack that pushes you back for example
    // index [3]: wind effects from a windbox
    // index [4+]: all future sources of velocities, such as wind, different gravity, etc

    [HideInInspector] static public float windVelocity = 0;

    [HideInInspector] static public List<float> velocitiesX = new List<float>() { 0, 0, 0, 0 };
    [HideInInspector] static public float resultVelocitiesX;

    private void Start()
    {
        windVelocity = 0;
        for (int i = 0; i < velocitiesX.Count; i++)
        {
            velocitiesX[i] = 0;
        }
    }
    void Update()
    {
        velocitiesX[3] = Mathf.MoveTowards(velocitiesX[3], windVelocity, 15 * Time.deltaTime);
        resultVelocitiesX = velocitiesX.Sum();
    }
}
