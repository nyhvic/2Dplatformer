using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigidbody2;
    public int nextMove;
    Animator animator;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;

    void Awake()
    {
        rigidbody2 = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        Think();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //Move
        rigidbody2.linearVelocity=new Vector2 (nextMove,rigidbody2.linearVelocityY);

        //PlatformCheck
        Vector2 frontVec = new Vector2(rigidbody2.position.x+nextMove, rigidbody2.position.y);
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector2.down, 1, LayerMask.GetMask("Platform"));
        if(rayHit.collider == null)
        {
            Turn();

        }
    }

    void Think()
    {
        //Set Next Active
        nextMove=Random.Range(-1,2);

        //Animation
        animator.SetInteger("WalkSpeed", nextMove);

        //Flip
        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }

        //Recursive
        float nextthinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextthinkTime);
    }

    void Turn()
    {
        nextMove = nextMove * -1;
        spriteRenderer.flipX = nextMove == 1;
        CancelInvoke();
        Invoke("Think", 5);
    }
    
    public void OnDamaged()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        spriteRenderer.flipY = true;
        capsuleCollider.enabled = false;
        rigidbody2.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        Invoke("DeActive", 2);
        
    }
    void DeActive()
    {
        gameObject.SetActive(false);   
    }
}
