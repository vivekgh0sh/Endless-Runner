using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class score : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public Transform player;
    // Update is called once per frame
    void Update()
    {
        scoreText.text = Mathf.FloorToInt(player.position.z).ToString();
    }
}
