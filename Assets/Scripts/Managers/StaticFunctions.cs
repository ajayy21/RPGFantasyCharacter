﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGController
{
    public static class StaticFunctions
    {

        public static void DeepCopyWeapon(Weapon from, Weapon to) {
            to.itemIcon = from.itemIcon;
            to.oh_idle = from.oh_idle;
            to.th_idle = from.th_idle;
            to.oneHandedActions = new List<Action>();
            
            //Copy one handed actions
            for (int i = 0; i < from.oneHandedActions.Count; i++)
            {
                Action oneHandedAction = new Action();
                DeepCopyActionToAction(oneHandedAction, from.oneHandedActions[i]);
                to.oneHandedActions.Add(oneHandedAction);
            }

            to.twoHandedActions = new List<Action>();
            //Copy two handed actions
            for (int i = 0; i < from.twoHandedActions.Count; i++)
            {
                Action twoHandedAction = new Action();
                DeepCopyActionToAction(twoHandedAction, from.twoHandedActions[i]);
                to.twoHandedActions.Add(twoHandedAction);
            }

            to.parryMultiplier = from.parryMultiplier;
            to.backstabMultiplier = from.backstabMultiplier;
            to.LeftHandMirror = from.LeftHandMirror;
            to.weaponModelPrefab = from.weaponModelPrefab;

            to.left_model_pos = from.left_model_pos;
            to.left_model_eulerRot = from.left_model_eulerRot;
            to.left_model_scale = from.left_model_scale;

            to.right_model_pos = from.right_model_pos;
            to.right_model_eulerRot = from.right_model_eulerRot;
            to.right_model_scale = from.right_model_scale;
            to.weaponStats = from.weaponStats;

            DeepCopyWeaponStats(from.weaponStats, to.weaponStats);

        }

        public static void DeepCopySpell(Spell from, Spell to) {

            to.itemName = from.itemName;
            to.itemIcon = from.itemIcon;
            to.itemDescription = from.itemDescription;

            to.spellType = from.spellType;
            to.spellClass = from.spellClass;
            to.projectile = from.projectile;
            to.particle_prefab = from.particle_prefab;
            to.spellEffect = from.spellEffect;

            to.actions = new List<SpellAction>();
            for (int i = 0; i < from.actions.Count; i++)
            {
                SpellAction spellAction = new SpellAction();
                DeepCopySpellAction(spellAction, from.actions[i]);
                to.actions.Add(spellAction);
            }
        }

        public static void DeepCopySpellAction(SpellAction to, SpellAction from) {
            to.actInput = from.actInput;
            to.targetAnim = from.targetAnim;
            to.throwAnim = from.throwAnim;
            to.castTime = from.castTime;
        }

        public static void DeepCopyActionToAction(Action action, Action weaponAct) {

            action.input = weaponAct.input;
            action.defaultTargetAnim = weaponAct.defaultTargetAnim;
            action.spellClass = weaponAct.spellClass;
            action.actionType = weaponAct.actionType;
            action.canParry = weaponAct.canParry;
            action.canBeParried = weaponAct.canBeParried;
            action.chageSpeed = weaponAct.chageSpeed;
            action.animSpeed = weaponAct.animSpeed;
            action.canBackStab = weaponAct.canBackStab;
            action.overrideDamageAnimation = weaponAct.overrideDamageAnimation;
            action.damageAnim = weaponAct.damageAnim;
            action.staminaCost = weaponAct.staminaCost;
            action.manaCost = weaponAct.manaCost;

            DeepCopyStepsList(weaponAct, action);
        }

        public static void DeepCopyStepsList(Action from, Action to) {
            to.actionSteps = new List<ActionSteps>();

            for (int i = 0; i < from.actionSteps.Count; i++)
            {
                ActionSteps step = new ActionSteps();
                DeepCopyActionSteps(from.actionSteps[i], step);
                to.actionSteps.Add(step);
            }
        }

        public static void DeepCopyAction(Weapon weapon, ActionInput actionInput, ActionInput assign, List<Action> actionList,bool isLeftHand = false)
        {
            Action action = GetAction(assign, actionList);
            Action weaponAct = weapon.GetAction(weapon.oneHandedActions, actionInput);

            //If the weapon has no action, skip
            if (weaponAct == null)
            {
                //Debug.Log("No weapon action found!");
                return;
            }

            DeepCopyStepsList(weaponAct, action);
            action.defaultTargetAnim = weaponAct.defaultTargetAnim;
            action.spellClass = weaponAct.spellClass;
            action.actionType = weaponAct.actionType;
            action.canParry = weaponAct.canParry;
            action.canBeParried = weaponAct.canBeParried;
            action.chageSpeed = weaponAct.chageSpeed;
            action.animSpeed = weaponAct.animSpeed;
            action.canBackStab = weaponAct.canBackStab;
            action.overrideDamageAnimation = weaponAct.overrideDamageAnimation;
            action.damageAnim = weaponAct.damageAnim;
            action.parryMultiplier = weapon.parryMultiplier;
            action.backstabMultiplier = weapon.backstabMultiplier;
            action.staminaCost = weaponAct.staminaCost;
            action.manaCost = weaponAct.manaCost;
            
            if (isLeftHand)
            {
                //Left hand animations are just mirror version of right hand animations
                action.mirror = true;
            }
            
        }

        public static void DeepCopyWeaponStats(WeaponStats weaponStats_From, WeaponStats weaponStats_To)
        {

            weaponStats_To.physicalDamage = weaponStats_From.physicalDamage;
            weaponStats_To.strikeDamage = weaponStats_From.strikeDamage;
            weaponStats_To.thrustDamage = weaponStats_From.thrustDamage;
            weaponStats_To.slashDamage = weaponStats_From.slashDamage;

            weaponStats_To.magicDamage = weaponStats_From.magicDamage;
            weaponStats_To.lightningDamage = weaponStats_From.lightningDamage;
            weaponStats_To.fireDamage = weaponStats_From.fireDamage;
            weaponStats_To.darkDamage = weaponStats_From.darkDamage;

        }

        public static Action GetAction(ActionInput actInput, List<Action> actionSlots)
        {
            for (int i = 0; i < actionSlots.Count; i++)
            {
                if (actionSlots[i].input == actInput)
                {
                    return actionSlots[i];
                }
            }
            
            return null;
        }

        public static void DeepCopyActionSteps(ActionSteps from, ActionSteps to) {
            to.animationBranches = new List<ActionAnimation>();

            for (int i = 0; i < from.animationBranches.Count; i++)
            {
                ActionAnimation a = new ActionAnimation();
                a.input = from.animationBranches[i].input;
                a.targetAnim = from.animationBranches[i].targetAnim;
                to.animationBranches.Add(a);
            }
        }
    }
}