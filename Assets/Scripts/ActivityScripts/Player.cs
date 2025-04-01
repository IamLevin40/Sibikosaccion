using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int currentTileIndex = 0;
    public Board board;
    public bool isMoving = false;

    public void Move(int steps)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveStepByStep(steps));
        }
    }

    private IEnumerator MoveStepByStep(int steps)
    {
        isMoving = true;
        for (int i = 0; i < steps; i++)
        {
            currentTileIndex = (currentTileIndex + 1) % board.tiles.Length;
            Vector3 targetPosition = board.tiles[currentTileIndex].transform.position + (Vector3.up * 0.2f);
            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
                yield return null;
            }
            transform.position = targetPosition;
            yield return new WaitForSeconds(0.2f);
        }
        board.tiles[currentTileIndex].tileData.OnLand(this);
        isMoving = false;
    }
}
