using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Define los puntos por los que se moverá el enemigo
    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    // Velocidad a la que se mueve el enemigo
    public float speed;
    // Define si el movimiento es cíclico
    public bool cyclic;
    // Tiempo de espera en cada waypoint
    public float waitTime;
    // Suavizado del movimiento
    [Range(0, 2)]
    public float easeAmount;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;

    void Start()
    {
        // Inicializa los waypoints globales
        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    void Update()
    {
        // Calcula el movimiento del enemigo
        Vector3 velocity = CalculateMovement();
        transform.Translate(velocity); // Mueve al enemigo
    }

    // Función para suavizar el movimiento
    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    // Calcula el movimiento del enemigo entre los waypoints
    Vector3 CalculateMovement()
    {
        // Si es tiempo de esperar antes de moverse
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        // Determina el siguiente waypoint
        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);

        // Calcula el porcentaje completado entre los waypoints
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        // Calcula la nueva posición
        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        // Si llega al siguiente waypoint
        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            // Si el movimiento no es cíclico, invierte la dirección al final
            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints); // Invierte los waypoints
                }
            }

            // Establece el tiempo de espera antes del próximo movimiento
            nextMoveTime = Time.time + waitTime;
        }

        // Retorna el movimiento del enemigo
        return newPos - transform.position;
    }

    // Dibuja gizmos para visualizar los waypoints
    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
