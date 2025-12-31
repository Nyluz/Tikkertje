using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimTargetCollector : MonoBehaviour
{
    public List<Transform> targets = new List<Transform>();
    private PlayerInput playerInput;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GameModeManager.Instance.players[playerInput.playerIndex].tagAbility)
            return;

        // Pick your own rule: tag, layer, component, etc.
        if (other.name == "Player(Clone)" && other.gameObject != gameObject)
        {
            var t = other.transform;
            if (!targets.Contains(t)) targets.Add(t);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Player(Clone)")
            targets.Remove(other.transform);
    }
}