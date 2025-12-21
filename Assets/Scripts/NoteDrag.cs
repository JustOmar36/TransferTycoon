using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class NoteDrag : MonoBehaviour
{
    private Camera cam;
    private bool isDragging = false;
    private float currentZ;           // current distance from camera
    private Collider selfCollider;
    private Vector3 mouseDownPos;
    private float clickThreshold = 5f;
    private NoteUIManager uiManager;

    void Start()
    {
        //TMP_InputField input = GetComponent<TMP_InputField>();
        //input.lineType = TMP_InputField.LineType.MultiLineNewline;

        cam = Camera.main;
        selfCollider = GetComponent<Collider>();
        currentZ = cam.WorldToScreenPoint(transform.position).z;
        GameObject uiManagerObj = GameObject.Find("UIManager");
        if (uiManagerObj != null)
        {
            uiManager = uiManagerObj.GetComponent<NoteUIManager>();
        }
    }

    void OnMouseDown()
    {
        mouseDownPos = Input.mousePosition;
        // UpdateDepthFromRaycast();
    }

    void OnMouseDrag()
    {
        float moveDist = Vector3.Distance(mouseDownPos, Input.mousePosition);
        if (moveDist >= clickThreshold)
        {
            isDragging = true;
        }

        if (isDragging)
        {
            UpdateDepthFromRaycast();
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        float moveDist = Vector3.Distance(mouseDownPos, Input.mousePosition);
        if (moveDist < clickThreshold)
        {
            OnSimpleClick();
        }

        Debug.Log("[OnMouseUp] Dragging stopped.");
    }

    private void OnSimpleClick()
    {
        Debug.Log("Click without drag detected on " + gameObject.name);

        if (uiManager == null)
        {
            return;
        }
        uiManager.OnAddNoteButtonClick();

        string content = GetTempText();
        uiManager.AddTextToNoteInputField(content, this.gameObject);
    }

    private string GetTempText()
    {
        TextMeshPro input = GetComponentInChildren<TextMeshPro>();
        if (input == null)
        {
            return "";
        }
        string content = input.text;
        return content;
    }

    private void UpdateDepthFromRaycast()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 20f, ~0, QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == selfCollider)
                continue;
            Debug.Log(hit.collider.name);
            if (!hit.collider.name.Contains("PC_monitor"))
                continue;

            Vector3 dir = hit.point - ray.origin;
            Debug.DrawRay(ray.origin, dir, Color.cyan, 0.1f);
            // Draw a small cross at the hit point for visualization
            float size = 0.05f; // cross size
            Vector3 p = hit.point;
            Debug.DrawLine(p - Vector3.up * size, p + Vector3.up * size, Color.red, 0.1f);
            Debug.DrawLine(p - Vector3.right * size, p + Vector3.right * size, Color.red, 0.1f);
            Debug.DrawLine(p - Vector3.forward * size, p + Vector3.forward * size, Color.red, 0.1f);

            // Update only depth based on hit distance from camera
            currentZ = hit.distance;
            transform.position = ray.origin + ray.direction * (currentZ - 0.01f);
            Debug.Log($"[Raycast] Depth updated = {currentZ:F2}, Hit {hit.collider.name}");
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        transform.position = ray.origin + ray.direction * 0.5f;
        return;
    }
}
