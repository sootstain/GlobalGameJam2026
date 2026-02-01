using Cinemachine;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using System.Collections;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;
		public bool lockMovement = false;
		
		public bool isInteracting = false;
		public bool lockCamera = false;

		[Header("Camera Lock")]
		[SerializeField] private float cameraLockSlerpSpeed = 12f;
		private Transform _cameraLockTarget;

		// cinemachine
		private float _cinemachineTargetPitch;
		public CinemachineVirtualCamera cineMachineCamera;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;
		
		// force pan variables
		public bool isForcePan = false;
		public bool isZoomIn = false;
		private Vector3 forcePanToPosition;
		public float forcePanMovementTimeMs = 3;
		private float panLerpLevel = 0;
		private float panLerpTimeStart;
		private float panLerpTimeEnd;

		public float maxZoomFOV = 40;
		private float currZoomFOV;
		private float minZoomFOV = 0;

		private float zoomLerpLevel = 0;
		private float zoomTimeMs = 1;

		private float zoomLerpTimeEnd;
		private float zoomLerpTimeStart;
		public int dollyZoomSpeed = 5;

		private Vector3 movePlayerBackDirection;

		public CanvasGroup canvasGroup;

		public bool readyForLevelChange = false;

	
#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;
		
		private Transform objTransform;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
        {
            if(!lockMovement){
                JumpAndGravity();
                GroundedCheck();
                Move();
            }
            else if(isForcePan){
                if(forcePanToPosition == null){
                    Debug.LogError("Attempted to force pan without location, aborting...");
                }
                else{
                    Vector3 direction = forcePanToPosition - CinemachineCameraTarget.transform.position;
                    Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
                    panLerpLevel = (Time.time-panLerpTimeStart)/(panLerpTimeEnd-panLerpTimeStart);
                    CinemachineCameraTarget.transform.rotation = Quaternion.Lerp(CinemachineCameraTarget.transform.rotation, toRotation, panLerpLevel);
                    if(panLerpLevel >= 0.1f){
                        movePlayerBackDirection = Vector3.Normalize(direction);
                        CinemachineCameraTarget.transform.rotation = toRotation;
                        Debug.DrawRay(CinemachineCameraTarget.transform.position,movePlayerBackDirection,Color.red,50);
                        isForcePan = false;
                        isZoomIn = true;
                        zoomLerpTimeStart = Time.time;
                        zoomLerpTimeEnd = zoomLerpTimeStart + zoomTimeMs;
                    }
                }
            }
            else if(isZoomIn){
                zoomLerpLevel = (Time.time-zoomLerpTimeStart)/(zoomLerpTimeEnd-zoomLerpTimeStart);
                float currFOV = Mathf.Lerp(maxZoomFOV,minZoomFOV,zoomLerpLevel);
                objTransform.position-=movePlayerBackDirection*Time.deltaTime*dollyZoomSpeed;
                cineMachineCamera.m_Lens.FieldOfView = currFOV;
                canvasGroup.alpha = zoomLerpLevel;
                if(currFOV <= minZoomFOV+(minZoomFOV*0.1f)){
                    isZoomIn = false;
                    isForcePan = false;
                    isZoomIn = false;
                    lockCamera = false;
                    lockMovement = false;
                    readyForLevelChange = true;
                }
            }
        }

		private void LateUpdate()
		{
			if (_cameraLockTarget != null)
			{
				Vector3 dir = _cameraLockTarget.position - CinemachineCameraTarget.transform.position;
				if (dir.sqrMagnitude > 0.0001f)
				{
					Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
					CinemachineCameraTarget.transform.rotation =
						Quaternion.Slerp(CinemachineCameraTarget.transform.rotation, targetRot, Time.deltaTime * cameraLockSlerpSpeed);
				}
				return;
			}

			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			if (lockCamera)
				return;

			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			if (_controller == null || !_controller.enabled)
				return;

			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}
		
		public void forcePan(GameObject target){
			forcePanToPosition = target.transform.position;
			isForcePan = true;
			panLerpTimeStart = Time.time;
			panLerpTimeEnd = panLerpTimeStart + forcePanMovementTimeMs;

		}

		public void LockCameraTo(Transform target)
		{
			if (target == null)
			{
				Debug.LogWarning($"{nameof(FirstPersonController)}.{nameof(LockCameraTo)} called with null target.");
				return;
			}

			_cameraLockTarget = target;
			lockCamera = true;
			lockMovement = true;
		}

		public void UnlockCamera()
		{
			_cameraLockTarget = null;
			lockCamera = false;
			lockMovement = false;
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}