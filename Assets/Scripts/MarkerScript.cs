using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MarkerScript : MonoBehaviour
{
    public List<RectTransform> markers;
    public Camera cam;
    public Canvas canvas;

    public int index;

    private void Start()
    {
        index = transform.root.GetComponent<PlayerInput>().playerIndex;
    }

    void LateUpdate()
    {
        if (GameModeManager.Instance.players[index].tagAbility)
        {
            var players = GameManager.Instance.players;

            if (players.Count <= 1)
                return;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            int markerIndex = 0;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == transform.root.gameObject || GameModeManager.Instance.players[i].tagAbility)
                    continue;

                if (markerIndex >= markers.Count)
                    break;

                Vector3 viewportPos = cam.WorldToViewportPoint(
                    players[i].transform.position + Vector3.up * 2.5f
                );

                if (viewportPos.z <= 0f)
                {
                    markers[markerIndex].gameObject.SetActive(false);
                    markerIndex++;
                    continue;
                }

                markers[markerIndex].anchoredPosition = new Vector2(
                    (viewportPos.x - 0.5f) * canvasRect.sizeDelta.x,
                    (viewportPos.y - 0.5f) * canvasRect.sizeDelta.y
                );

                markers[markerIndex].gameObject.SetActive(true);
                markerIndex++;
            }

            // cleanup
            for (int i = markerIndex; i < markers.Count; i++)
            {
                markers[i].gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (var marker in markers)
            {
                marker.gameObject.SetActive(false);
            }
        }

    }
}
