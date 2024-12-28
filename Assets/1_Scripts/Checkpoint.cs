using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Timeline;

public class Checkpoint : MonoBehaviour
{
    public Vector3 checkpointPosition;
    // Start is called before the first frame update
    void Start()
    {
        checkpointPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")){
            Debug.Log("체크포인트 오브젝트에 도달했음");
            Player playerObj = other.GetComponent<Player>();
            playerObj.SetCheckpoint(checkpointPosition);
            //playerObj.respawnPosition = transform.position + playerObj.respawnPositionOrignal;
        }
    }
}
