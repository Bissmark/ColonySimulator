using UnityEngine;
using UnityEngine.UI;

public class RandomizeMapButton : MonoBehaviour
{
    public Map mapGenerator; // Reference to the Map script

    void Start() 
    {
        // Get the Button component attached to this GameObject
        Button button = GetComponent<Button>();
        // Add a listener for when the button is clicked and call RandomizeMap() method
        button.onClick.AddListener(RandomizeMap);
    }

    void RandomizeMap()
    {
        // Randomize offset values
        mapGenerator.offset.x = Random.Range(0, 99999);
        mapGenerator.offset.y = Random.Range(0, 99999);
        // Regenerate the map
        mapGenerator.GenerateMap();
    }
}
