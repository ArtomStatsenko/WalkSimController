using System.Numerics;
using UnityEngine;
using WalkSimController;
using CoreVector2 = System.Numerics.Vector2;
using CoreVector3 = System.Numerics.Vector3;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerUnityAdapter : MonoBehaviour
{
    [SerializeField]
    private Transform cameraRig = null!;

    [SerializeField]
    private Camera playerCamera = null!;

    private readonly WalkSimController.WalkSimController controller = new(CoreVector3.Zero);
    private CharacterController character = null!;

    private void Awake()
    {
        character = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        var state = controller.Update(Time.deltaTime, GatherInput());

        var velocity = new UnityEngine.Vector3(
            state.BodyVelocity.X,
            Physics.gravity.y,
            state.BodyVelocity.Z
        );

        character.Move(velocity * Time.deltaTime);
        controller.SyncBodyPosition(ToCore(transform.position));

        ApplyCamera(state);
    }

    private static FrameInput GatherInput() =>
        new()
        {
            MoveAxis = new CoreVector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ),
            MouseDelta = new CoreVector2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            ),
        };

    private void ApplyCamera(WalkSimState state)
    {
        transform.rotation = Quaternion.Euler(0f, state.RawCameraYaw, 0f);

        var bodycamYawOffset = DeltaAngleDeg(state.RawCameraYaw, state.CameraRotation.Yaw);
        cameraRig.localRotation = Quaternion.Euler(
            state.CameraRotation.Pitch,
            bodycamYawOffset,
            state.CameraRotation.Roll
        );

        playerCamera.transform.localPosition = new UnityEngine.Vector3(
            state.CameraLocalOffset.X,
            state.CameraLocalOffset.Y,
            state.CameraLocalOffset.Z
        );
    }

    private static CoreVector3 ToCore(UnityEngine.Vector3 v) => new(v.x, v.y, v.z);

    private static float DeltaAngleDeg(float from, float to) => Mathf.DeltaAngle(from, to);
}
