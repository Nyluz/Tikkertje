using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class SplitScreenCamera : MonoBehaviour
{
    private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineCamera cinemachineFPSCamera;
    [SerializeField] private CinemachineCamera cinemachineThirdPersonCamera;

    private Camera cam;
    public int index;
    public int totalPlayers;

    public void Setup()
    {
        cam = GetComponent<Camera>();
        index = GetComponentInParent<PlayerInput>().playerIndex;
        cinemachineBrain = GetComponent<CinemachineBrain>();
        totalPlayers = PlayerInput.all.Count;
        cam.depth = index;

        SetupCameraRect();
        SetupCinemachine();
        SetupCullingMask();
    }

    public void SetupCullingMask()
    {
        cam.cullingMask = ~LayerMask.GetMask("Player" + (index + 1));
    }

    private void SetupCinemachine()
    {
        cinemachineBrain.ChannelMask = (OutputChannels)Enum.Parse(typeof(OutputChannels), "Channel0" + (index + 1));
        cinemachineFPSCamera.OutputChannel = (OutputChannels)Enum.Parse(typeof(OutputChannels), "Channel0" + (index + 1));
        cinemachineThirdPersonCamera.OutputChannel = (OutputChannels)Enum.Parse(typeof(OutputChannels), "Channel0" + (index + 1));
    }

    private void SetupCameraRect()
    {
        if (totalPlayers == 1)
        {
            cam.rect = new Rect(0, 0, 1, 1);
        }
        else if (totalPlayers == 2)
        {
            cam.rect = new Rect(0, index == 0 ? 0.5f : 0f, 1, 0.5f);
        }
        else if (totalPlayers == 3)
        {
            cam.rect = new Rect(
                index == 0 ? 0 : (index == 1 ? 0.5f : 0),
                index < 2 ? 0.5f : 0,
                index < 2 ? 0.5f : 1,
                0.5f
                );
        }
        else
        {
            cam.rect = new Rect((index % 2) * 0.5f, (index < 2) ? 0.5f : 0f, 0.5f, 0.5f);
        }
    }
}
