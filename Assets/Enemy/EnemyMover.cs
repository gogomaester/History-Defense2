using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyMover : MonoBehaviour
{
    [SerializeField] [Range(0f, 5f)] float speed = 1f;

    List<Node> path = new List<Node>();
    Enemy enemy;
    PathFinder pathFinder;
    GridManager gridManager;

    private void OnEnable()
    {
        ReturnEnemyToStart();
        RecalculatePath(true);
    }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        gridManager = FindObjectOfType<GridManager>();
        pathFinder = FindObjectOfType<PathFinder>();
    }

    // Finds the path according to the path tags from the current position
    private void RecalculatePath(bool resetPath)
    {
        Vector2Int coordinates = new Vector2Int();

        if (resetPath)
        {
            coordinates = pathFinder.StartCoordinates;
        } 
        else
        {
            coordinates = gridManager.GetCoordinatesFromPosition(transform.position);
        }

        // Stop following old path
        StopAllCoroutines();
        path.Clear();
        path = pathFinder.GetNewPath(coordinates);
        // Follow assigned path
        StartCoroutine(FollowPath());
    }

    // Returns / places the enemy to start position of the path
    private void ReturnEnemyToStart()
    {
        this.transform.position = gridManager.GetPositionFromCoordiantes(pathFinder.StartCoordinates);
    }

    // Prints all waypoints name for this enemy - COROUTINE
    IEnumerator FollowPath()
    {
        // i = 1 -> start from the second waypoint
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition = gridManager.GetPositionFromCoordiantes(path[i].coordinates);
            float travelPercent = 0f;

            // Move progress
            while (travelPercent < 1f)
            {
                travelPercent += Time.deltaTime * speed;
                // LERP the start & beginning vectors to return the new position
                Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, travelPercent);

                // Call the Handle Rotation method to return
                //Quaternion newRotation = Quaternion.Euler(0f, HandleRotation(startPosition, endPosition), 0f);
                transform.LookAt(endPosition);

                // Sets the final enemy position
                this.transform.position = newPosition;
                //this.transform.SetPositionAndRotation(newPosition, newRotation);

                // Update each frame
                yield return new WaitForEndOfFrame();
            }
        }
        // End of path reached
        EnemyEndOfPathReached();
    }

    private void EnemyEndOfPathReached()
    {
        // (Destroy) deactivate the enemy
        GetRidOfEnemy();
        // Damage the player (steal their money)
        enemy.StealGold();
        // TODO: screen shake
    }

    private void GetRidOfEnemy()
    {
        // Enemy destruction, particles, sound effects etc.
        this.gameObject.SetActive(false);
    }

}
