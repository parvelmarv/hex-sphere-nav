using UnityEngine;
using _Scripts.Systems.HexGridBuildSystem;

public class PlanetGridSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildController controller; // The input source
    [SerializeField] private PlanetGraph planetGraph;
    [SerializeField] private Camera mainCam;

    [Header("Visuals")]
    public GameObject selectionVisualPrefab; 
    
    [Header("Settings")]
    public float moveCooldown = 0.2f;
    
    private GameObject _cursorInstance; 
    private PlanetTile _currentTile;
    private float _nextMoveTime;
    private Vector3 _debugInputDir; 
    
    public PlanetGraph PlanetGraph => planetGraph;

    private void Start()
    {
        // 1. Resolve Dependencies if not assigned in Inspector
        if (controller == null) controller = FindObjectOfType<BuildController>();
        if (mainCam == null) mainCam = Camera.main;
        
        // 2. Init Start Position
        if (planetGraph != null && planetGraph.tiles.Count > 0)
        {
            SetTile(planetGraph.tiles[0]);
        }
    }

    private void Update()
    {
        // 1. Safety Checks
        if (controller == null || _currentTile == null || planetGraph == null) return;

        // 2. Update Visuals (Every frame, because planet might move)
        UpdateCursorVisual();

        // 3. READ INPUT FROM CONTROLLER
        Vector2 input = controller.Move;

        // 4. RUN LOGIC
        // Check magnitude to prevent drift, check cooldown to prevent skipping tiles too fast
        if (input.magnitude > 0.25f && Time.time >= _nextMoveTime)
        {
            MoveSelection(input.x, input.y);
        }
    }

    private void UpdateCursorVisual()
    {
        if (_cursorInstance != null && _currentTile != null)
        {
            _cursorInstance.transform.position = planetGraph.transform.TransformPoint(_currentTile.LocalCenter);
            _cursorInstance.transform.up = planetGraph.transform.TransformDirection(_currentTile.LocalNormal);
        }
    }
    
    public void ShowMarker()
    {
        if (_cursorInstance == null && selectionVisualPrefab != null)
        {
            _cursorInstance = Instantiate(selectionVisualPrefab);
            UpdateCursorVisual(); // Snap to position immediately
        }
    }

    public void HideMarker()
    {
        if (_cursorInstance != null)
        {
            Destroy(_cursorInstance);
            _cursorInstance = null;
        }
    }

    private void MoveSelection(float x, float y)
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        // --- 1. Calculate Direction on Surface ---
        // We translate "Stick Input" into "Direction on the planet surface relative to the camera"
        
        Vector3 camRight = mainCam.transform.right;
        Vector3 worldNormal = planetGraph.transform.TransformDirection(_currentTile.LocalNormal);
        Vector3 worldCenter = planetGraph.transform.TransformPoint(_currentTile.LocalCenter);

        // Project camera right onto the tile's plane so "Right" follows the curve
        Vector3 surfaceRight = Vector3.ProjectOnPlane(camRight, worldNormal).normalized;
        
        // Calculate "Forward" on the surface (Cross product of Right and Up)
        Vector3 surfaceForward = Vector3.Cross(surfaceRight, worldNormal);

        // Combine inputs to get the desired direction vector
        Vector3 inputDir = (surfaceRight * x + surfaceForward * y).normalized;
        _debugInputDir = inputDir;

        // --- 2. Find Best Neighbor ---
        PlanetTile bestNeighbor = null;
        float bestScore = -1f; // Dot product ranges from -1 to 1

        foreach (var neighbor in _currentTile.Neighbors)
        {
            // Direction from current tile center to neighbor center (IN WORLD SPACE)
            Vector3 neighborWorldCenter = planetGraph.transform.TransformPoint(neighbor.LocalCenter);
            Vector3 dirToNeighbor = (neighborWorldCenter - worldCenter).normalized;
            
            // Compare our input direction with the neighbor's direction
            float score = Vector3.Dot(inputDir, dirToNeighbor);

            if (score > bestScore)
            {
                bestScore = score;
                bestNeighbor = neighbor;
            }
        }

        // --- 3. Apply Move ---
        // Threshold of 0.1f ensures we don't move if the input is perpendicular/ambiguous
        if (bestNeighbor != null && bestScore > 0.1f)
        {
            SetTile(bestNeighbor);
            _nextMoveTime = Time.time + moveCooldown;
        }
    }

    private void SetTile(PlanetTile tile)
    {
        _currentTile = tile;
        UpdateCursorVisual();
    }
    
    // Public getter so the BuildSystem knows where we are
    public PlanetTile GetSelectedTile() => _currentTile;

    // Optional: Visual Debugging to see where the code "thinks" you are pointing
    private void OnDrawGizmos()
    {
        if (_currentTile == null || planetGraph == null) return;

        Vector3 worldCenter = planetGraph.transform.TransformPoint(_currentTile.LocalCenter);

        if (_debugInputDir != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(worldCenter, _debugInputDir * 5f);
        }

        Gizmos.color = Color.green;
        foreach (var n in _currentTile.Neighbors)
        {
            Vector3 neighborCenter = planetGraph.transform.TransformPoint(n.LocalCenter);
            Gizmos.DrawLine(worldCenter, neighborCenter);
        }
    }
}
