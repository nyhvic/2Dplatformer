using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerMove : MonoBehaviour
{
    public GameManager Manager;
    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigidbody2;
    SpriteRenderer spriteRenderer;
    Animator animator;
    CapsuleCollider2D capsuleCollider;
    public int jumpCount=0;
    public float jumpstartTime;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioCoin;
    public AudioClip audioJumpItem;
    public AudioClip audioPotion;
    public AudioClip audioSave;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public AudioSource audioSource;

    void Awake()
    {
        rigidbody2 = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource=GetComponent<AudioSource>();
    }

    void Update()
    {
        //Jump
        if (Input.GetButtonDown("Jump")&&jumpCount<2)
        {
            PlaySound("Jump");
            jumpCount++;
            if (rigidbody2.linearVelocityY < 0 ) {
                if (!animator.GetBool("Is_Jumping")) { jumpCount++; }
            }
            rigidbody2.linearVelocity = new Vector2(rigidbody2.linearVelocityX, 0);
            animator.SetBool("Is_Jumping", true);
            rigidbody2.AddForce(Vector2.up * jumpPower*0.65f, ForceMode2D.Impulse); 
            jumpstartTime=Time.time;
        }

        if (Input.GetButton("Jump")&& animator.GetBool("Is_Jumping") && Time.time - jumpstartTime <= 0.1f)
        {
            if (jumpCount == 1){
                rigidbody2.AddForce(Vector2.up * Time.deltaTime * 5 * jumpPower, ForceMode2D.Impulse);
            }
            else { rigidbody2.AddForce(Vector2.up * Time.deltaTime * 3 * jumpPower, ForceMode2D.Impulse); }
        }


        //Stop
        if (!Input.GetButton("Horizontal"))
        {
            rigidbody2.linearVelocity = new Vector2(0, rigidbody2.linearVelocity.y);
        }

        //Direction
        //if (Input.GetButton("Horizontal"))
        //    spriteRenderer.flipX = Input.GetAxisRaw("Horizontal")==-1;
        if (rigidbody2.linearVelocityX != 0) spriteRenderer.flipX = rigidbody2.linearVelocityX < 0 ? true : false;

        //Animation
        if (Mathf.Abs(rigidbody2.linearVelocity.x) < 0.3f)
        {
            animator.SetBool("Is_Run", false);
        }
        else { animator.SetBool("Is_Run", true); }
    }

    void FixedUpdate()
    {
        //Move by control
        float h = Input.GetAxisRaw("Horizontal");
        rigidbody2.AddForce(Vector2.right * h, ForceMode2D.Impulse);
        //Max Speed
        if (rigidbody2.linearVelocityX > maxSpeed) rigidbody2.linearVelocity = new Vector2(maxSpeed, rigidbody2.linearVelocityY);
        else if (rigidbody2.linearVelocityX < maxSpeed * (-1)) rigidbody2.linearVelocity = new Vector2(maxSpeed * (-1), rigidbody2.linearVelocityY);

        //Landing
        if (rigidbody2.linearVelocityY < 0)
        {
            RaycastHit2D rayHit = Physics2D.Raycast(rigidbody2.position, Vector2.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                {
                    animator.SetBool("Is_Jumping", false);
                    jumpCount = 0;
                }
            }
        }
    }

    //Collision
     void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            //attack
            if (rigidbody2.linearVelocityY < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            //damaged
            else
            {
                OnDamaged(collision.transform.position);
            }
        }
    }

    void OnAttack(Transform enemy)
    {
        PlaySound("Attack");
        rigidbody2.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        EnemyMove enemyMove=enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
        Manager.stagePoint += 5;
    }

    void OnDamaged(Vector2 targetPos)
    {
        PlaySound("Damaged");
        Manager.HealthDown();
        gameObject.layer = 11;
        spriteRenderer.color = new Color(1, 1, 1,0.4f);
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigidbody2.AddForce(new Vector2(dirc,1)*7, ForceMode2D.Impulse);
        animator.SetTrigger("Damaged");
        Invoke("OffDamaged", 0.7f);
    }

    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            PlaySound("Coin");
            collision.gameObject.SetActive(false);
            Manager.stagePoint += 10;
        }
        else if (collision.gameObject.tag == "Finish")
        {
            PlaySound("Finish");
            Manager.NextStage();
        }
        else if(collision.gameObject.tag == "Potion")
        {
            PlaySound("Potion");
            collision.gameObject.SetActive(false);
            Manager.HealthUp();
        }
        else if (collision.gameObject.tag == "Jump")
        {
            PlaySound("JumpItem");
            collision.gameObject.SetActive(false);
            jumpCount--; 
            StartCoroutine(jumpActive(collision));
        }
        else if (collision.gameObject.tag == "Save")
        {
            PlaySound("Save");
            collision.GetComponent<SpriteRenderer>().enabled = false;
            Manager.repos=transform.position;
            StartCoroutine(SaveSeen(collision));
        }
    }

    IEnumerator jumpActive(Collider2D collision)
    {
        yield return new WaitForSeconds(2f);
        collision.gameObject.SetActive(true);
    }

    IEnumerator SaveSeen(Collider2D collision)
    {
        yield return new WaitForSeconds(2f);
        collision.GetComponent<SpriteRenderer>().enabled=true;
    }

    public void OnDie()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        spriteRenderer.flipY = true;
        capsuleCollider.enabled = false;
        rigidbody2.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        Invoke("erase",0.3f);
        Manager.died = true;
    }

    public void erase()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0);
    }

    public void Live()
    {
        spriteRenderer.color = new Color(1, 1, 1,1);
        spriteRenderer.flipY = false;
        capsuleCollider.enabled = true;
        gameObject.SetActive(true);
    }

    public void VelocityZero()
    {
        rigidbody2.linearVelocity = Vector2.zero;
    }

    void PlaySound(string soundName)
    {
        switch (soundName)
        {
            case "Jump":
                audioSource.clip = audioJump;
                break;
            case "Attack":
                audioSource.clip = audioAttack;
                break;
            case "Damaged":
                audioSource.clip = audioDamaged;
                break;
            case "Coin":
                audioSource.clip = audioCoin;
                break;
            case "JumpItem":
                audioSource.clip = audioJumpItem;
                break;
            case "Potion":
                audioSource.clip = audioPotion;
                break;
            case "Save":
                audioSource.clip = audioSave;
                break;
            case "Die":
                audioSource.clip = audioDie;
                break;
            case "Finish":
                audioSource.clip = audioFinish;
                break;
        }
        audioSource.Play();
}
}
