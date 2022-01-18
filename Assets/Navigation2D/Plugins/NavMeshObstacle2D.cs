// Navigation2D Script (c) noobtuts.com
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class NavMeshObstacle2D : MonoBehaviour
{
    // NavMeshObstacle properties
    public NavMeshObstacleShape shape = NavMeshObstacleShape.Box;
    public Vector2 center;
    public Vector2 size = Vector2.one;
    public bool carve = false; // experimental and hard to debug in 2D

    public GameObject go;

    // the projection
    NavMeshObstacle obstacle;

    public NetworkIdentity identity;

    public bool setted = false;

    public bool isPermanentObject = false;

    public GameObject parent;

    public void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        if (!go)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "NAVIGATION2D_OBSTACLE" + name;
            parent = gameObject;
            go.transform.tag = "Obstacle";
            go.transform.position = NavMeshUtils2D.ProjectTo3D(transform.position);
            go.transform.rotation = Quaternion.Euler(NavMeshUtils2D.RotationTo3D(transform.eulerAngles));
            obstacle = go.AddComponent<NavMeshObstacle>();
            go.AddComponent<NavMeshObjstaclePlacer>();
            go.GetComponent<NavMeshObjstaclePlacer>().placer = gameObject;

            // disable mesh and collider (no collider for now)
            Destroy(obstacle.GetComponent<Collider>());
            Destroy(obstacle.GetComponent<MeshRenderer>());

            // copy properties to projection all the time
            // (in case they are modified after creating it)
            obstacle.center = NavMeshUtils2D.ProjectTo3D(center);
            obstacle.size = new Vector3(size.x, 1, size.y);
            obstacle.carving = true;

            // scale and rotate to match scaled/rotated sprites center properly
            obstacle.transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.y);
            obstacle.transform.rotation = Quaternion.Euler(NavMeshUtils2D.RotationTo3D(transform.eulerAngles));

            //project position to 3d
            obstacle.transform.position = NavMeshUtils2D.ProjectTo3D(transform.position);

            go.transform.position = NavMeshUtils2D.ProjectTo3D(transform.position);
            go.transform.rotation = Quaternion.Euler(NavMeshUtils2D.RotationTo3D(transform.eulerAngles));

            //// copy properties to projection all the time
            //// (in case they are modified after creating it)
            obstacle.center = NavMeshUtils2D.ProjectTo3D(center);
            obstacle.size = new Vector3(size.x, 1, size.y);
            obstacle.carving = carve;

            //// scale and rotate to match scaled/rotated sprites center properly
            obstacle.transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.y);
            obstacle.transform.rotation = Quaternion.Euler(NavMeshUtils2D.RotationTo3D(transform.eulerAngles));

            ////project position to 3d
            obstacle.transform.position = NavMeshUtils2D.ProjectTo3D(transform.position);

            if (!identity) identity = GetComponent<NetworkIdentity>();
        }
    }

    public void OnDestroy()
    {
        //destroy projection if not destroyed yet
        if (obstacle) Destroy(obstacle.gameObject);
        if (go) Destroy(go.gameObject);
    }

    public void OnEnable()
    {
        if (obstacle) obstacle.enabled = true;
        if (go) go.SetActive(true);
    }

    public void OnDisable()
    {
        if (obstacle) obstacle.enabled = false;
        if (go) go.SetActive(false);
    }

    // radius gizmo (gizmos.matrix for correct rotation)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawWireCube(center, size);
    }

    // validation
    void OnValidate()
    {
        // force shape to box for now because we would need a separate Editor
        // GUI script to switch between size and radius otherwise
        shape = NavMeshObstacleShape.Box;
    }

    // NavMeshAgent proxies ////////////////////////////////////////////////////
    public Vector2 velocity
    {
        get { return NavMeshUtils2D.ProjectTo2D(obstacle.velocity); }
        // set: is a bad idea
    }
}
