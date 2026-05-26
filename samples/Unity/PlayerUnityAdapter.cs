using UnityEngine;
using WalkSimController;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerUnityAdapter : MonoBehaviour
{
    [SerializeField]
    private Transform cameraRig = null!;

    [SerializeField]
    private Camera playerCamera = null!;

    private readonly WalkSimController.WalkSimController _controller =
        new(System.Numerics.Vector3.Zero);
    private CharacterController _character = null!;

    private void Awake()
    {
        _character = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        var state = _controller.Update(Time.deltaTime, GatherInput());

        var velocity = new Vector3(state.BodyVelocity.X, Physics.gravity.y, state.BodyVelocity.Z);

        _character.Move(velocity * Time.deltaTime);
        _controller.SyncBodyPosition(ToCore(transform.position));

        ApplyCamera(state);
    }

    private static FrameInput GatherInput()
    {
        return new FrameInput
        {
            MoveAxis = new System.Numerics.Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ),
            MouseDelta = new System.Numerics.Vector2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            ),
        };
    }

    private void ApplyCamera(WalkSimState state)
    {
        transform.rotation = Quaternion.Euler(0f, state.RawCameraYaw, 0f);

        var bodycamYawOffset = DeltaAngleDeg(state.RawCameraYaw, state.CameraRotation.Yaw);
        cameraRig.localRotation = Quaternion.Euler(
            state.CameraRotation.Pitch,
            bodycamYawOffset,
            state.CameraRotation.Roll
        );

        playerCamera.transform.localPosition = new Vector3(
            state.CameraLocalOffset.X,
            state.CameraLocalOffset.Y,
            state.CameraLocalOffset.Z
        );
    }

    private static System.Numerics.Vector3 ToCore(Vector3 v) => new(v.x, v.y, v.z);

    private static float DeltaAngleDeg(float from, float to) => Mathf.DeltaAngle(from, to);
}
