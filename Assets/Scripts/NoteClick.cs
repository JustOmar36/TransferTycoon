using UnityEngine;

public class NoteClick : MonoBehaviour
{
    private bool isZoomed = false;         // Whether the note is currently zoomed in
    private Vector3 originalPosition;      // Stores the note's original position
    private Quaternion originalRotation;   // Stores the note's original rotation
    private Vector3 originalScale;         // Stores the note's original scale

    void OnMouseDown()
    {
        // If the note is NOT zoomed in, zoom it to the center of the screen
        if (!isZoomed)
        {
            // Save the note’s original transform data
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalScale = transform.localScale;

            // Move the note to the center of the screen (in front of the camera)
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;

            // Make the note face the camera
            transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);

            // Scale up the note (2x larger)
            transform.localScale = originalScale * 2f;

            isZoomed = true; // Mark as zoomed in
        }
        else
        {
            // If it's already zoomed in, return it to its original state
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            transform.localScale = originalScale;

            isZoomed = false; // Mark as not zoomed
        }
    }
}
