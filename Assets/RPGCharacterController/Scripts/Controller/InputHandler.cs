﻿using RPGController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public class InputHandler : MonoBehaviour
    {
        public float vertical;
        public float horizontal;

        bool b_input;
        bool a_input;
        bool x_input;
        bool y_input;
        bool interact_input;

        //Bumpers and triggers for XBOX controler
        bool rb_input;  //Right bumper
        bool rt_input; //Right trigger
        float rt_axis;

        bool lb_input;  //Left bumper
        bool lt_input; //Left trigger
        float lt_axis;

        //Pad axises
        float d_y;
        float d_x;
        bool d_up;
        bool d_down;
        bool d_right;
        bool d_left;

        bool previously_d_up;
        bool previously_d_down;
        bool previously_d_right;
        bool previously_d_left;

        bool leftAxis_down;
        bool rightAxis_down;

        float b_timer;
        float rt_timer;
        float lt_timer;

        float sprintDelay = 0.3f;
        StateManager states;
        CameraManager cameraManager;
        UIManager UIManager;
        InventoryUI inventoryUI;
        DialogueManager dialogManager;

        bool isGestureOpen;
        float delta;

        public static InputHandler Instance;

        private void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            QuickSlot.Instance.Init();

            states = GetComponent<StateManager>();
            states.Init();

            cameraManager = CameraManager.Instance;
            cameraManager.Init(states);

            UIManager = UIManager.Instance;
            inventoryUI = InventoryUI.Instance;
            inventoryUI.Init(states.inventoryManager);

            dialogManager = DialogueManager.Instance;

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            GetInput();
            HandleUI();
            UpdateStates();

            //Update the state manager
            states.FixedTick(Time.deltaTime);

            //Update the camera manager
            cameraManager.Tick(delta);

        }

        bool preferItem;
        private void Update()
        {
            delta = Time.deltaTime;
            states.Tick(delta);

            //Debug.Log("isInvMenu: " + isInvMenu);
            //Input delay must be FIXED!
            if (inventoryUI.isMenu)
            {
                UIManager.CloseAnnounceType();

                //Update inventory UI
                inventoryUI.Tick();
            }
            else
            {
                if (!dialogManager.dialogueActive)
                {
                    if (states.pickableItemManager.itemCandidate != null || states.pickableItemManager.worldInterCandidate != null)
                    {
                        if (states.pickableItemManager.itemCandidate && states.pickableItemManager.worldInterCandidate)
                        {
                            if (preferItem)
                            {
                                PickUpItem();
                            }
                            else
                            {
                                InteractInWorld();
                            }
                        }
                        else
                        {
                            if (states.pickableItemManager.itemCandidate && !states.pickableItemManager.worldInterCandidate)
                            {
                                PickUpItem();
                            }

                            if (states.pickableItemManager.worldInterCandidate && states.pickableItemManager.itemCandidate == null)
                            {
                                InteractInWorld();
                            }
                        }
                    }
                    else
                    {
                        UIManager.CloseAnnounceType();
                        if (interact_input)
                        {
                            UIManager.CloseCards();
                            interact_input = false;
                        }
                    }
                }
                else
                {
                    UIManager.CloseAnnounceType();
                }
            }

            dialogManager.Tick(interact_input);
            //Update character stats (health, mana, stamina etc.)
            states.MonitorStats();

            ResetInputAndStates();

            //Update the UI manager
            UIManager.Tick(states.characterStats, delta);

        }

        void PickUpItem()
        {

            UIManager.OpenAnnounceType(UIManager.UIActionType.pickup);

            if (interact_input)
            {
                Vector3 targetDir = states.pickableItemManager.itemCandidate.transform.position - transform.position;
                states.SnapToRotation(targetDir);
                states.pickableItemManager.PickCandidate();

                states.PlayAnimation(StaticStrings.animState_PickUp);
                interact_input = false;
            }
        }

        void InteractInWorld()
        {
            UIManager.OpenAnnounceType(states.pickableItemManager.worldInterCandidate.actionType);

            if (interact_input)
            {
                Debug.Log("World interaction!");
                states.InteractLogic();
                interact_input = false;
            }

        }

        void GetInput()
        {
            //Get input from buttons and axises
            vertical = Input.GetAxis(StaticStrings.Input_Vertical);
            horizontal = Input.GetAxis(StaticStrings.Input_Horizontal);

            b_input = Input.GetButton(StaticStrings.B);
            a_input = Input.GetButton(StaticStrings.A);
            y_input = Input.GetButtonUp(StaticStrings.Y);
            x_input = Input.GetButton(StaticStrings.X);
            interact_input = Input.GetButtonUp(StaticStrings.Interact);

            rt_input = Input.GetButton(StaticStrings.RT);
            rt_axis = Input.GetAxis(StaticStrings.RT);

            //Even if you're not pressing button, but there is movement return true
            if (rt_axis != 0)
            {
                rt_input = true;
            }

            lt_input = Input.GetButton(StaticStrings.LT);
            lt_axis = Input.GetAxis(StaticStrings.LT);
            if (lt_axis != 0)
            {
                lt_input = true;
            }

            rb_input = Input.GetButton(StaticStrings.RB);
            lb_input = Input.GetButton(StaticStrings.LB);

            leftAxis_down = Input.GetButtonUp(StaticStrings.L) || Input.GetKeyUp(KeyCode.Space);
            rightAxis_down = Input.GetButtonUp(StaticStrings.R) || Input.GetKeyUp(KeyCode.T);

            if (b_input)
            {
                b_timer += delta;
            }

            d_x = Input.GetAxis(StaticStrings.Pad_X);
            d_y = Input.GetAxis(StaticStrings.Pad_Y);

            //In keyboard item swtich is assigned to between 1-4
            d_up = Input.GetKeyUp(KeyCode.Alpha1) || d_y > 0;
            d_down = Input.GetKeyUp(KeyCode.Alpha2) || d_y < 0;
            d_left = Input.GetKeyUp(KeyCode.Alpha3) || d_x < 0;
            d_right = Input.GetKeyUp(KeyCode.Alpha4) || d_x > 0;

            bool gesturesMenu = Input.GetButtonUp(StaticStrings.GestureSelect);
            if (gesturesMenu)
            {
                isGestureOpen = !isGestureOpen;
            }

            bool menu = Input.GetButtonUp(StaticStrings.Start);
            //Debug.Log("Menu: " + menu);
            if (menu)
            {
                inventoryUI.isMenu = !inventoryUI.isMenu;

                if (inventoryUI.isMenu)
                {
                    isGestureOpen = false;
                    inventoryUI.OpenUI();
                }
                else
                {
                    inventoryUI.CloseUI();
                }
            }

        }

        void HandleUI()
        {
            UIManager.gesturesManager.HandleGestures(isGestureOpen);

            if (isGestureOpen)
            {
                currentUIState = UIState.gestures;
            }
            else
            {
                currentUIState = UIState.game;
            }

            if (inventoryUI.isMenu)
            {
                currentUIState = UIState.inventory;
            }

            switch (currentUIState)
            {
                case UIState.game:
                    HandleQuickSlotChanges();
                    break;
                case UIState.gestures:
                    HandleGesturesUI();
                    break;
                case UIState.inventory:
                    break;
                default:
                    break;
            }

        }

        UIState currentUIState;
        enum UIState
        {
            game, gestures, inventory
        }

        void HandleGesturesUI()
        {
            //Switch left hand weapon
            if (d_left)
            {
                if (!previously_d_left)
                {
                    UIManager.gesturesManager.SelectGesture(false);
                    previously_d_left = true;
                }
            }

            //Switch right hand weapon
            if (d_right)
            {
                if (!previously_d_right)
                {
                    UIManager.gesturesManager.SelectGesture(true);
                    previously_d_right = true;
                }
            }

            if (!d_left)
            {
                previously_d_left = false;
            }
            if (!d_right)
            {
                previously_d_right = false;
            }

            if (rb_input)
            {
                isGestureOpen = false;
                states.isUsingItem = true;

                if (UIManager.gesturesManager.closeWeapons)
                {
                    states.closeWeapons = true;
                }
                states.PlayAnimation(UIManager.gesturesManager.currentGestureAnim, false);
            }

        }

        void UpdateStates()
        {
            states.horizontal = horizontal;
            states.vertical = vertical;

            Vector3 vertical_Movement = vertical * cameraManager.transform.forward;
            Vector3 horizontal_Movement = horizontal * cameraManager.transform.right;

            states.moveDirection = (vertical_Movement + horizontal_Movement).normalized;

            //Smooth transition between slow and fast movement animations
            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            states.moveAmount = Mathf.Clamp01(m);

            //It he inventory UI is opened, dont register player control inputs
            //(when inventory ui is opened, character can't move)
            //if (inventoryUI.isMenu)
            //{
            //    return;
            //}

            if (states.isRunning)
            {
                if (leftAxis_down)
                {
                    //Jump
                    states.Jump();
                }
            }

            if (x_input)
            {
                b_input = false;
            }

            if (b_input && b_timer > sprintDelay)
            {
                states.isRunning = (states.moveAmount > 0) && (states.characterStats.currentStamina > 0);
            }

            if (b_input == false && b_timer > 0 && b_timer < sprintDelay)
            {
                states.rollInput = true;
            }

            //Update input states
            states.itemInput = x_input;
            states.rt = rt_input;
            states.lt = lt_input;
            states.rb = rb_input;
            states.lb = lb_input;

            if (y_input)
            {
                if (states.pickableItemManager.itemCandidate && states.pickableItemManager.worldInterCandidate)
                {
                    preferItem = !preferItem;
                }
                else
                {
                    states.isTwoHanded = !states.isTwoHanded;
                    states.HandleTwoHanded();
                }
            }

            //Check locked on target's status
            if (states.lockOnTarget != null)
            {
                //If the enemy is dead, then stop locking the camera
                if (states.lockOnTarget.eStates.isDead)
                {
                    Debug.Log("Locked on target DIED!");
                    states.lockOn = false;
                    states.lockOnTarget = null;
                    states.lockOnTransform = null;
                    cameraManager.lockOn = false;
                    cameraManager.lockOnTarget = null;
                }
            }

            if (rightAxis_down)
            {
                states.lockOn = !states.lockOn;

                states.lockOnTarget = EnemyManager.Instance.GetEnemy(transform.position);
                //If there is no target transform to lock on, set status to false
                if (states.lockOnTarget == null)
                {
                    states.lockOn = false;
                }

                cameraManager.lockOnTarget = states.lockOnTarget;
                states.lockOnTransform = states.lockOnTarget.GetTarget();
                cameraManager.lockOnTransform = states.lockOnTransform;
                cameraManager.lockOn = states.lockOn;
            }
        }

        void HandleQuickSlotChanges()
        {

            if (states.isSpellCasting || states.isUsingItem)
            {
                return;
            }

            //Switch spell 
            if (d_up)
            {
                if (!previously_d_up)
                {
                    previously_d_up = true;
                    states.inventoryManager.ChangeToNextSpell();
                }
            }

            //Switch consumable
            if (d_down)
            {
                if (!previously_d_down)
                {
                    previously_d_down = true;
                    states.inventoryManager.ChangeToNextConsumable();
                }
            }


            if (!d_up)
            {
                previously_d_up = false;
            }
            if (!d_down)
            {
                previously_d_down = false;
            }

            //You cant change weapon while character's moving or has two handed weapon
            if (!states.onEmpty)
            {
                return;
            }

            if (states.isTwoHanded)
            {
                return;
            }

            //Switch left hand weapon
            if (d_left)
            {
                if (!previously_d_left)
                {
                    states.inventoryManager.ChangeToNextWeapon(true);
                    previously_d_left = true;
                }
            }

            //Switch right hand weapon
            if (d_right)
            {
                if (!previously_d_right)
                {
                    states.inventoryManager.ChangeToNextWeapon(false);
                    previously_d_right = true;
                }
            }

            if (!d_left)
            {
                previously_d_left = false;
            }
            if (!d_right)
            {
                previously_d_right = false;
            }
        }

        void ResetInputAndStates()
        {
            //Reset the inputs for next frame
            if (interact_input)
            {
                interact_input = false;
            }

            if (b_input == false)
            {
                b_timer = 0;
            }

            if (states.rollInput)
            {
                states.rollInput = false;
            }

            if (states.isRunning)
            {
                states.isRunning = false;
            }
        }
    }
}