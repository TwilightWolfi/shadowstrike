using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour {
    [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
    float speed = 9;

    [SerializeField, Tooltip("Acceleration while grounded.")]
    float walkAcceleration = 75;

    [SerializeField, Tooltip("Acceleration while airborne.")]
    float airAcceleration = 30;

    [SerializeField, Tooltip("Deceleration absent of attempted movement.")]
    float groundDeceleration = 70;

    [SerializeField, Tooltip("Maximum jump height, ignoring gravity.")]
    float jumpHeight = 4;
	
    bool grounded;

    private BoxCollider2D boxCollider;

    private Vector2 velocity;

    private void Awake() {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void SetVelocity(Vector2 velocity) {
        this.velocity = velocity;
    }

	private void Update() {
		
		grounded = false;
		
		float moveInput = Input.GetAxisRaw("Horizontal");
        velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, walkAcceleration * Time.deltaTime);
        
        transform.Translate(velocity * Time.deltaTime);
        
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size * transform.localScale, 0);
        foreach (Collider2D hit in hits) {
			if (hit != boxCollider) {
				ColliderDistance2D colliderDistance = hit.Distance(boxCollider);
				
				if (colliderDistance.distance <= 0) {
					transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
					Vector3 changeInPos = colliderDistance.pointA - colliderDistance.pointB;
					Vector3 dir = changeInPos.normalized;
					if (dir.y < 0) {
						grounded = true;
					}
					dir.x = 1 - Mathf.Abs(dir.x);
					dir.y = 1 - Mathf.Abs(dir.y);
					velocity.x *= dir.x;
					velocity.y *= dir.y;
				}
			}
        }
		
		if (Input.GetButtonDown("Jump") || (Input.GetButtonDown("Vertical") && Input.GetAxis("Vertical") > 0)) {
                print("Jumped");
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
            }
		
        if (!grounded) {
            velocity.y += Physics2D.gravity.y * Time.deltaTime;
        }
    }
}
