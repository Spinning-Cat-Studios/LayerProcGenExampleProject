class_name FreeLookCameraSamples extends Camera3D

# Modifier keys' speed multiplier
const SHIFT_MULTIPLIER = 10
const ALT_MULTIPLIER = 1.0 / SHIFT_MULTIPLIER
const CHECKPOINT_DIST_DELTA_THRESHOLD = 50.0

@export_range(0.0, 1.0) var sensitivity: float = 0.25

# Mouse state
var _mouse_position = Vector2(0.0, 0.0)
var _total_pitch = 0.0

# Movement state
var _direction = Vector3(0.0, 0.0, 0.0)
var _velocity = Vector3(0.0, 0.0, 0.0)
var _acceleration = 30
var _deceleration = -10
var _vel_multiplier = 25

# Keyboard state
var _w = false
var _s = false
var _a = false
var _d = false
var _q = false
var _e = false
var _shift = false
var _alt = false

var speed_multi = 7

var checkpoint_position: Vector3 = Vector3.ZERO

func _ready():
	checkpoint_position = global_transform.origin

func _input(event):
	# Receives mouse motion
	if event is InputEventMouseMotion:
		_mouse_position = event.relative

	# Receives mouse button input
	if event is InputEventMouseButton:
		match event.button_index:
			MOUSE_BUTTON_RIGHT: # Only allows rotation if right click down
				Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED if event.pressed else Input.MOUSE_MODE_VISIBLE)
			MOUSE_BUTTON_WHEEL_UP: # Increases max velocity
				_vel_multiplier = clamp(_vel_multiplier * 1.1, 0.2, 200)
			MOUSE_BUTTON_WHEEL_DOWN: # Decreases max velocity
				_vel_multiplier = clamp(_vel_multiplier / 1.1, 0.2, 200)

	# Receives key input
	if event is InputEventKey:
		match event.keycode:
			KEY_W:
				_w = event.pressed
			KEY_S:
				_s = event.pressed
			KEY_A:
				_a = event.pressed
			KEY_D:
				_d = event.pressed
			KEY_Q:
				_q = event.pressed
			KEY_E:
				_e = event.pressed
			KEY_SHIFT:
				_shift = event.pressed
			KEY_ALT:
				_alt = event.pressed

# Updates mouselook and movement every frame
func _process(delta):
	_update_lazy_evaluation_checkpoint()
	_update_mouselook()
	_update_movement(delta)

func compute_dist_from_checkpoint_in_xz() -> float:
	# Computes the distance from the checkpoint in the XZ plane
	var checkpoint_xz = Vector2(checkpoint_position.x, checkpoint_position.z)
	var current_xz = Vector2(global_transform.origin.x, global_transform.origin.z)
	return checkpoint_xz.distance_to(current_xz)

func _update_lazy_evaluation_checkpoint():
	var dist_from_checkpoint = compute_dist_from_checkpoint_in_xz()
	if dist_from_checkpoint > CHECKPOINT_DIST_DELTA_THRESHOLD:
		# Send a signal via SignalBus to indicate that nodes should be reconstructed
		#
		# There is some finesse required here, we don't want to reconstruct all nodes every time 
		# this condition is met, just those that are on the boundary, i.e. remove nodes that are 
		# too far away, and add nodes that have come into range.
		#
		# This can probably be done by querying sqlite for nodes that are within a certain distance
		# and circumventing the constructors for those nodes, in lieu of focusing only on nodes
		# that are not in the database.
		#
		# Regardless, now of this logic should be implemented here, this is just a signal, the
		# consumer of this signal should handle the logic of reconstructing nodes as indicated above.
		SignalBus.GetInstance().ReconstructNodes.emit(
			checkpoint_position,
			global_transform.origin,
			dist_from_checkpoint
		);
		# Update the checkpoint position to the current position
		checkpoint_position = global_transform.origin
	pass

# Updates camera movement
func _update_movement(delta):
	# Computes desired direction from key states
	_direction = Vector3(
		(_d as float) - (_a as float),
		(_e as float) - (_q as float),
		(_s as float) - (_w as float)
	)

	# Computes the change in velocity due to desired direction and "drag"
	# The "drag" is a constant acceleration on the camera to bring it's velocity to 0
	var offset = _direction.normalized() * _acceleration * _vel_multiplier * delta \
	+ _velocity.normalized() * _deceleration * _vel_multiplier * delta

	# Compute modifiers' speed multiplier
	if _shift: speed_multi *= SHIFT_MULTIPLIER
	if _alt: speed_multi *= ALT_MULTIPLIER

	# Checks if we should bother translating the camera
	if _direction == Vector3.ZERO and offset.length_squared() > _velocity.length_squared():
		# Sets the velocity to 0 to prevent jittering due to imperfect deceleration
		_velocity = Vector3.ZERO
	else:
		# Clamps speed to stay within maximum value (_vel_multiplier)
		_velocity.x = clamp(_velocity.x + offset.x, -_vel_multiplier, _vel_multiplier)
		_velocity.y = clamp(_velocity.y + offset.y, -_vel_multiplier, _vel_multiplier)
		_velocity.z = clamp(_velocity.z + offset.z, -_vel_multiplier, _vel_multiplier)

		translate(_velocity * delta * speed_multi)

# Updates mouse look 
func _update_mouselook():
	# Only rotates mouse if the mouse is captured
	if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		_mouse_position *= sensitivity
		var yaw = _mouse_position.x
		var pitch = _mouse_position.y
		_mouse_position = Vector2(0, 0)

		# Prevents looking up/down too far
		pitch = clamp(pitch, -90 - _total_pitch, 90 - _total_pitch)
		_total_pitch += pitch

		rotate_y(deg_to_rad(-yaw))
		rotate_object_local(Vector3(1,0,0), deg_to_rad(-pitch))

func _on_speed_slider_value_changed(new_speed: float) -> void:
	speed_multi = new_speed
