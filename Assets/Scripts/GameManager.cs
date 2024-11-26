using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int Health;
    public PlayerMove PlayerMove;
    public Image[] UIHealth;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage;
    public GameObject[] stages;
    public GameObject Retry;
    public bool Win=false;
    public Vector3 repos=new Vector3(0,0,-1);
    public bool saved=false;
    public bool died = false;

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
        if (Input.GetKeyDown("r"))
        {
            if (Win)
            {
                stageIndex = 0;
                Health = 3;
                for (int i = 0; i < 3; i++)
                {
                    UIHealth[i].color = new Color(1, 1, 1, 1);
                }
                SceneManager.LoadScene(0);
            }
            else if (died)
            {
                died = false;
                Health = 3;
                for (int i = 0; i < 3; i++)
                {
                    UIHealth[i].color = new Color(1, 1, 1, 1);
                }
            }
            PlayerReposition();
        }
    }

    public void NextStage()
    {
        repos=new Vector3(0,0,-1);
        if (stageIndex < stages.Length-1)
        {
            stages[stageIndex].SetActive(false);
            stageIndex++;
            stages[stageIndex].SetActive(true);
            PlayerReposition();
            UIStage.text="Stage"+(stageIndex+1).ToString();
        }
        else//clear
        {
            Time.timeScale = 0;
            Win= true;
            Retry.SetActive(true);
        }
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        if(Health > 1)
        {
            Health--;
            UIHealth[Health].color = new Color(1, 1, 1, 0.3f);
        }
        else
        {
            UIHealth[0].color = new Color(1, 1, 1, 0.3f);
            PlayerMove.OnDie();
            Retry.SetActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Health = 0;
            for(int i = 0; i < 3; i++)
            {
                UIHealth[i].color = new Color(1, 1, 1, 0.3f);
            }
            PlayerMove.OnDie();
            Retry.SetActive(true);
        }
    }

    public void PlayerReposition()
    {
        Win=false;
        PlayerMove.transform.position = repos;
        PlayerMove.VelocityZero();
        Time.timeScale = 1;
        PlayerMove.Live();
        Retry.SetActive(false);
        PlayerMove.jumpCount = 0;
        PlayerMove.GetComponent<Animator>().SetBool("Is_Jumping", false);
    }

    public void HealthUp()
    {
        if (Health != 3)
        {
            Health++;
            UIHealth[Health-1].color = new Color(1, 1, 1, 1);
        }
    }

}
