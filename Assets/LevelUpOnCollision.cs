using UnityEngine;

public class LevelUpOnCollision : MonoBehaviour
{
    public GameObject effectOnCollide;
    private GameObject Player;
    private void OnTriggerEnter(Collider other)
    {
         if (other.gameObject.CompareTag("PlayerMesh"))
         {
            LevelManager.IncreaseLevel();
            var effect = Instantiate(effectOnCollide, Player.transform);
            effect.transform.position = new Vector3(effect.transform.position.x, effect.transform.position.y, effect.transform.position.z + 10);
            Destroy(gameObject);
         }
    }

  
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Ship");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
