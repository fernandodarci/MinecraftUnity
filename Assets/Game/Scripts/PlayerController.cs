using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    public bool isGrounded;
    public bool isSprinting;

    public Camera GameCamera;
    public Transform Target;
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f;
    public float boundsTolerance = 0.1f;

    private CharacterController character;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private float lastheight = 0;
    public float checkIncrement = 0.1f;
    public float reach = 16f;
    private SectorCoord targetSector;

    private PlayerControls _playerControl;
   

    private void OnEnable()
    {
        _playerControl = new PlayerControls();
        _playerControl.Enable();
        
        _playerControl.Player.Move.performed += (ctx) => 
        { 
            horizontal = ctx.ReadValue<Vector2>().x; 
            vertical = ctx.ReadValue<Vector2>().y; 
        };

        _playerControl.Player.Move.canceled += (_) => { horizontal = 0; vertical = 0; };
        
        _playerControl.Player.Look.performed += (ctx) =>
        {
            mouseHorizontal = ctx.ReadValue<Vector2>().x;
            mouseVertical = ctx.ReadValue<Vector2>().y;
        };

        _playerControl.Player.Look.canceled += (_) => { mouseHorizontal = 0; mouseVertical = 0; };
        
        _playerControl.Player.Sprint.performed += (_) => isSprinting = true;
        _playerControl.Player.Sprint.canceled += (_) => isSprinting = false;
        _playerControl.Player.Jump.performed += (_) => Jump();
        _playerControl.Player.LeftClick.performed += PlaceBlock;
      
    }

    private void PlaceBlock(InputAction.CallbackContext ctx)
    {
        Debug.Log("Pressed Left Button");
        if (Target.gameObject.activeSelf == true && targetSector.block.y > 0)
            MainGameManager.GM.World.SetBlockInWorld(targetSector, 0);
        
    }

    private void OnDisable()
    {
        _playerControl.Disable();
        _playerControl.Dispose();
    }

    void Start()
    {
        character = GetComponent<CharacterController>();
    }

    private void Jump()
    {
        if (isGrounded)
        {
            verticalMomentum = jumpForce;
            isGrounded = false;
        }
    }

  
    void Update()
    {
        if (lastheight == transform.position.y) isGrounded = true;
        else if (!isGrounded) lastheight = transform.position.y;
        CalculateVelocity();

        mouseVertical = Mathf.Clamp(mouseVertical, -70f, 70f);

        transform.Rotate(Vector3.up * mouseHorizontal);
        GameCamera.transform.Rotate(Vector3.right * -mouseVertical);
        var cameraRay = GameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        placeCursorBlocks();
        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit, GameData.SectorSize))
        {
            //Debug.Log(hit.collider.name);
            ProceduralSector target = hit.collider.GetComponent<ProceduralSector>();
            if (target != null)
            {
                Vector3 hitp = hit.point;
               
                Target.gameObject.SetActive(true);
               // Target.position = hitp.ToVector3Int() + new Vector3(0.5f, -0.5f, 0.5f);
                Debug.DrawLine(GameCamera.transform.position, hit.point, Color.yellow);
            }
           
            
        }
        else Target.gameObject.SetActive(false);


        character.Move(velocity);
           
    }

    private void placeCursorBlocks()
    {

        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = GameCamera.transform.position + (GameCamera.transform.forward * step);
            targetSector = new SectorCoord().FromAbsolute(pos.ToVector3Int());

            byte b = MainGameManager.GM.World.GetBlockInWorld(targetSector);

            if (b != 0)
            {
                MainGameManager.GM.marker.text = pos.ToString() + "<>" + pos.ToVector3Int().ToString()
                                          + "\n" + targetSector.ToString() + 
                                          "\n" + MainGameManager.BlockType[b].Name + " selected";
                Target.transform.position = new Vector3(pos.x.ToInt() + 0.5f, pos.y.ToInt() + 0.5f, pos.z.ToInt() + 0.5f);
                //placeBlock.position = lastPos;

                Target.gameObject.SetActive(true);
                //placeBlock.gameObject.SetActive(true);

                return;

            }

            lastPos = pos.ToVector3Int();

            step += checkIncrement;

        }

        Target.gameObject.SetActive(false);
   
    }


    private void CalculateVelocity()
    {

        // Affect vertical momentum with gravity.
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if we're sprinting, use the sprint multiplier.
        velocity = ((transform.forward * vertical) + (transform.right * horizontal))
            * Time.fixedDeltaTime * (isSprinting ? sprintSpeed : walkSpeed);

        // Apply vertical momentum (falling/jumping).
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;  
    }




  
}