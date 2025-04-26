using UnityEngine;
using UnityEngine.UI;

namespace GamePortfolio.UI.Components {
    /// <summary>
    /// UI component for a connection line between skill nodes
    /// </summary>
    public class ConnectionLine : MonoBehaviour {
        [SerializeField]
        private RectTransform lineTransform;
        [SerializeField]
        private Image lineImage;

        public RectTransform FromNode { get; private set; }
        public RectTransform ToNode { get; private set; }

        private void Awake() {
            if (lineTransform == null)
                lineTransform = GetComponent<RectTransform>();

            if (lineImage == null)
                lineImage = GetComponent<Image>();
        }

        /// <summary>
        /// Set the connection points
        /// </summary>
        public void SetPoints(RectTransform from, RectTransform to) {
            FromNode = from;
            ToNode = to;

            UpdateLine();
        }

        /// <summary>
        /// Set line width
        /// </summary>
        public void SetWidth(float width) {
            if (lineTransform != null) {
                lineTransform.sizeDelta = new Vector2(lineTransform.sizeDelta.x, width);
            }
        }

        /// <summary>
        /// Set line color
        /// </summary>
        public void SetColor(Color color) {
            if (lineImage != null) {
                lineImage.color = color;
            }
        }

        /// <summary>
        /// Update the line position and rotation
        /// </summary>
        private void UpdateLine() {
            if (FromNode == null || ToNode == null || lineTransform == null)
                return;

            // Get positions in canvas space
            Vector3 fromPos = FromNode.position;
            Vector3 toPos = ToNode.position;

            // Calculate direction and length
            Vector3 direction = toPos - fromPos;
            float distance = direction.magnitude;

            // Position line at midpoint
            lineTransform.position = fromPos + direction * 0.5f;

            // Set length
            lineTransform.sizeDelta = new Vector2(distance, lineTransform.sizeDelta.y);

            // Set rotation based on direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            lineTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void LateUpdate() {
            // Update line position if nodes move
            if (FromNode != null && ToNode != null) {
                UpdateLine();
            }
        }
    }
}