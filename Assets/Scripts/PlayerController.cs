using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform viewPoint;
    public float mouseSensitivity= 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;

    public bool invertLook;

    public float moveSpeed = 5f, runSpeed = 8f;
    private float activeSpeed;
    private Vector3 moveDir, movement;

    public CharacterController charController;

    private Camera cam;

    public float jumpForce = 12f, gravityMod = 3.5f;

    public Transform groundCheck;
    private bool isGrounded;
    public LayerMask whatIsGround;

    public GameObject bulletImpact;
    //public float timeBetweemShoots;
    private float shotCounter;
    public float muzzleDisplay;
    private float muzzleCounter;

    public float maxHeat = 10f, /*heatPerShot = 1f,*/ coolRate = 4f, overheatedCoolRate = 5f;
    private float heatCounter;
    private bool overHeated;

    public Gun[] allGuns;
    private int selectedGun;
    void Start()
    {
        //To hide and lock the mouse in the center of the screen
       Cursor.lockState = CursorLockMode.Locked;
       //to start the CharacterController component
       charController = GetComponent<CharacterController>();
       //To start the camera
       cam = Camera.main;
       //Set the max value to the Temp Slider
       UIController.instance.weaponTempSlider.maxValue = maxHeat;

       SwitchWeapon();

       //SpawnPoint
       Transform newTrans = SpawnManager.instance.GetSpawnPoint();
       transform.position = newTrans.position;
       transform.rotation = newTrans.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //To make the player "X" rotation
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        //To make the player Vertical view rotation
        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

        if(invertLook){
            viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }else
        {
            viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }

        //Check if the player is running and change the speed
        if(Input.GetKey(KeyCode.LeftShift)){
            activeSpeed = runSpeed;
        }else{
            activeSpeed = moveSpeed;
        }

        //PlayerMovement
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        //To make the movement keep in the same direction of the view point and fix the speed bug
        float velY = movement.y;
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeSpeed;
        movement.y = velY;

        if(charController.isGrounded){
            movement.y = 0f;
        }

        //Make the jump
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, 0.25f, whatIsGround);

        if(Input.GetButtonDown("Jump") && isGrounded){
            movement.y = jumpForce;
        }

        //Adding gravity
        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

        charController.Move(movement * Time.deltaTime);

        //Desactivating the muzzle effect
        if(allGuns[selectedGun].muzzleFlash.activeInHierarchy){
            muzzleCounter -= Time.deltaTime;

            if(muzzleCounter <= 0f){
                allGuns[selectedGun].muzzleFlash.SetActive(false);
            }
        }

        //Calling the shoot function
        if(!overHeated){
            if(Input.GetMouseButtonDown(0)){
                Shoot();
            }
            //Making the spray shoot
            if(Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic){
                shotCounter -= Time.deltaTime;
                if(shotCounter <= 0){
                    Shoot();
                }
            }

            heatCounter -= coolRate * Time.deltaTime;
        }else{
            heatCounter -= overheatedCoolRate * Time.deltaTime;
            if(heatCounter <= 0){
                overHeated = false;

                UIController.instance.overheatedMessage.gameObject.SetActive(false);
            }
        }

        if(heatCounter < 0){
            heatCounter = 0f;
        }

        //Switching weapons with the scroll
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f){
            selectedGun++;

            if(selectedGun >= allGuns.Length){
                selectedGun = 0;
            }

            SwitchWeapon();
        }else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f){
            selectedGun--;

            if(selectedGun < 0){
                selectedGun = allGuns.Length - 1;
            }
            SwitchWeapon();
        }

        //Switch the weapon with keyboard keys
        for(int i=0; i < allGuns.Length; i++){
            if(Input.GetKeyDown((i+1).ToString())){
                selectedGun = i;
                SwitchWeapon();
            }
        }

        //Setting the heatcounter value to the slider
        UIController.instance.weaponTempSlider.value = heatCounter;

        //To control the cursor
        if(Input.GetKeyDown(KeyCode.Escape)){
            Cursor.lockState = CursorLockMode.None;
        }else if(Cursor.lockState == CursorLockMode.None){
            if(Input.GetMouseButtonDown(0)){
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    //Function to shoot
    private void Shoot(){
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit)){
            GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
            Debug.Log("We hit " + hit.collider.gameObject.name);

            Destroy(bulletImpactObject, 10f);
        }

        shotCounter = allGuns[selectedGun].timeBetweenShots;

        //Overheated system
        heatCounter += allGuns[selectedGun].heatPerShot;
        if(heatCounter >= maxHeat){
            heatCounter = maxHeat;
            overHeated = true;
            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }

        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplay;

    }

    void LateUpdate()
    {
       //To make the camera take the same position and rotation of viewpoint
       cam.transform.position = viewPoint.position;
       cam.transform.rotation = viewPoint.rotation; 
    }

    //Function to switch weapons
    void SwitchWeapon(){
        foreach(Gun gun in allGuns){
            gun.gameObject.SetActive(false);
        }

        allGuns[selectedGun].gameObject.SetActive(true);
        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }
}
