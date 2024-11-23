using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private Transform goal;

    private int score;

    private void LateUpdate()
    {
        if ((goal.position - transform.position).sqrMagnitude < 1f)
        {
            score++;
            transform.position = Vector3.zero;

            SetGoalRandomPosition();

            Debug.Log($"Current Score : {score:n0}");
        }
    }

    private void SetGoalRandomPosition()
    {
        float x = Random.Range(-4f, 4f);
        float y = Random.Range(-4f, 4f);

        Vector3 position = new Vector3(x, y, 0f);

        goal.position = position;
    }

    public void MoveTo(Vector3 direction)
    {
        transform.position += direction;
    }
}
