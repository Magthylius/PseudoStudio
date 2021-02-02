using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] GameObject cameraHolder;
    [SerializeField] float mouseSens, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    [SerializeField] Item[] items;
    int itemIndex;
    int previousItemIndex = -1;

    float verticleLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;
    PhotonView PV;
    PlayerManager playerManager;

    const float maxHealth = 100.0f;
    float curHealth = maxHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        //Passing instantiation data into an instantiation method
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start()
    {
        if(PV.IsMine)
        {
            EquipItem(0);  
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        PlayerLook();
        PlayerMove();
        PlayerJump();

        for (int i = 0; i < items.Length; i++)
        {
            if(Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
            
        }

        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0.0f)
        {
            if(itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            }
            else
                EquipItem(itemIndex + 1);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0.0f)
        {
            if (itemIndex <= 0)
            {
                EquipItem(0);
            }
            else
                EquipItem(itemIndex - 1);
        }

        if(Input.GetMouseButtonDown(0))
        {
            items[itemIndex].Use();
        }

        //Die if you fall
        if(transform.position.y < -10.0f)
        {
            Die();
        }

    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        //!Physics are done in fixedupdate so it's not impacted by FPS.
        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.deltaTime);
    }

    #region Player Default Actions
    /// <summary>
    /// Allow players to look around
    /// </summary>
    void PlayerLook()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSens);
        verticleLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSens;
        verticleLookRotation = Mathf.Clamp(verticleLookRotation, -90.0f, 90.0f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticleLookRotation;
    }

    /// <summary>
    /// Allow players to move
    /// </summary>
    void PlayerMove()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
        
    }

    /// <summary>
    /// Allow player to jump
    /// </summary>
    void PlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }
    #endregion

    void EquipItem(int _index)
    {
        //! Prevent invisible gun
        if (_index == previousItemIndex)
            return;

        //!init
        itemIndex = _index;

        //! Set gameobject active based on itemIndex
        items[itemIndex].itemGameObject.SetActive(true);

        if(previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        //!Check if it is local player
        if (PV.IsMine)
        {
            //! hash is using Photon's hash instead of unity's hash.
            //! Send itemIndex over the network
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    //! This function is called upon receiving information
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //! client already has info, Verify if the target player is the player
        if(!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    public void TakeDamage(float damage)
    {
        //Call the method as string, define who gets this message, the variable to pass
        //This function will run on shooter's computer.
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    //Run on everyone's computer
    //PV.IsMine check makes it so that the function runs only on the victim's computer
    void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
            return;
        Debug.Log("hit" + damage);

        curHealth -= damage;

        if(curHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        playerManager.Die();
    }
}
