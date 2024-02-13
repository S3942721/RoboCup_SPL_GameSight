// public class SpawnBall : MonoBehaviour
// {
//     public GameObject prefab;
//     public float spawnDistance = 5f;

//     // Update is called once per frame
//     void Update()
//     {
//         if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
//         {
//             // Get the position and direction of the controller
//             Vector3 controllerPosition = transform.position;
//             Vector3 controllerForward = transform.forward;

//             // Calculate the spawn position on the wall
//             Vector3 spawnPosition = controllerPosition + controllerForward * spawnDistance;

//             // Spawn the object at the calculated position
//             GameObject spawnedBall = Instantiate(prefab, spawnPosition, Quaternion.identity);
//             Rigidbody spawnedBallRB = spawnedBall.GetComponent<Rigidbody>();
//             spawnedBallRB.velocity = Vector3.zero; // You may want to set the initial velocity to zero or adjust as needed
//         }
//     }
// }