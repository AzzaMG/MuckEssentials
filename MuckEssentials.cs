using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AzzaMods;
using HarmonyLib;
using UnityEngine;

namespace MuckEssentials
{
    public class MuckEssentials : MonoBehaviour
    {
        // Unlock everything
        private static string actionUnlockAllItemsAndStations = "Unlock All Items and Stations";

        // God Mode
        private static string optionNoDamage = "No Damage";
        private static bool enabledNoDamage = false;

        // Unlimited HP
        private static string optionFullHp = "Full HP";
        private static bool enabledFullHP = false;

        // Unlimited Stamina
        private static string optionUnlimitedStamina = "Full Stamina";
        private static bool enabledUnlimitedStamina = false;

        // Unlimited Hunger
        private static string optionFullHunger = "Full Hunger";
        private static bool enabledFullHunger = false;

        // Revive all players
        private static string actionReviveAllPlayers = "Revive All Players";

        // Revive specific player
        private static string optionPlayerToRevive = "Player To Revive";
        private static string actionReviveSpecificPlayer = "Revive Specific Player";

        // Movement Speed
        private static string optionMaxRunSpeed = "Max Run Speed";
        private static float maxRunSpeed = 13f;
        private static string optionMaxWalkSpeed = "Max Walk Speed";
        private static float maxWalkSpeed = 6.5f;

        // Override Coins
        private static string optionOverrideCoins = "Override Total Coins";
        private static bool enabledOverrideCoins = false;
        private static int overrideCoinsAmount = 0;

        // Enable Always Craft
        private static string optionCanAlwaysCraft = "Can Always Craft";
        private static bool enableAlwaysCraft = false;

        // Free Boat Upgrades
        private static string optionFreeBoatUpgrades = "Free Boat Upgrades";

        // Adding items to inventory
        private static string optionGiveItemWhichItem = "Item To Give";
        private static string optionItemGiveCount = "Item Give Count";
        private static string actionGiveItem = "Give Item";

        // Adding PowerUps
        private static string optionGivePowerUpWhichPowerUp = "Powerup To Give";
        private static string optionPowerUpGiveGiveCount = "Powerup Give Count";
        private static string actionGivePowerUp = "Give Powerup";

        // Infinite Jump
        private static string optionInfiniteJumps = "Infinite Jumps";
        private static bool enabledInfiniteJumps = false;

        private static int minGiveCount = 1;
        private static int maxGiveCount = 1024;

        // Skip tutorial
        private static string optionSkipTutorial = "Skip Tutorial";

        // Kill all mobs
        private static string optionKillAllMobs = "Kill All Mobs";
        private static string optionPreventMobSpawn = "Prevent Mob Spawning";

        void OnModLoaded()
        {
            // Unlock items
            Options.RegisterAction(actionUnlockAllItemsAndStations, "Unlock All Items and Stations");
            Options.SetDescription(actionUnlockAllItemsAndStations, "Press this to unlock all items and stations.");
            Options.AddPersistence(actionUnlockAllItemsAndStations);

            // Revive Button
            Options.RegisterAction(actionReviveAllPlayers, "Revive All Players");
            Options.SetDescription(actionReviveAllPlayers, "Attempts to revive all players on the server.");
            Options.AddPersistence(actionReviveAllPlayers);

            // Revive player list
            Options.RegisterDropdown(optionPlayerToRevive);
            Options.SetDescription(optionPlayerToRevive, "Select a player to revive.");
            Options.AddPersistence(optionPlayerToRevive);

            // Revive specific player
            Options.RegisterAction(actionReviveSpecificPlayer, "Revive Specific Player");
            Options.SetDescription(actionReviveSpecificPlayer, "Revive the specified player.");
            Options.AddPersistence(actionReviveSpecificPlayer);

            // Kill All Mobs
            Options.RegisterAction(optionKillAllMobs, "Kill All Mobs");
            Options.SetDescription(optionKillAllMobs, "Press this to kill every mob that is currently on the server. This works best if you're the host.");
            Options.AddPersistence(optionKillAllMobs);

            // Prevent mob spawning
            Options.RegisterBool(optionPreventMobSpawn, false);
            Options.SetDescription(optionPreventMobSpawn, "Prevents hostile mobs/monsters from spawning. This won't kill mobs that have already spawned.");
            Options.AddPersistence(optionPreventMobSpawn);
            Patching.Prefix(typeof(MobSpawner).GetMethod("ServerSpawnNewMob", Patching.AnyMethod), this.GetType().GetMethod("PrefixBlockMobSpawning", Patching.AnyMethod));
            Patching.Prefix(typeof(MobSpawner).GetMethod("SpawnMob", Patching.AnyMethod), this.GetType().GetMethod("PrefixBlockMobSpawning", Patching.AnyMethod));

            // No Damage
            Options.RegisterBool(optionNoDamage, false);
            Options.SetDescription(optionNoDamage, "Prevents any players from taking damage.");
            Options.AddPersistence(optionNoDamage);
            Patching.Prefix(typeof(PlayerStatus).GetMethod("Damage", Patching.AnyMethod), typeof(MuckEssentials).GetMethod("PrefixNoDamage", Patching.AnyMethod));
            Patching.Prefix(typeof(PlayerStatus).GetMethod("DealDamage", Patching.AnyMethod), typeof(MuckEssentials).GetMethod("PrefixNoDamage", Patching.AnyMethod));
            Patching.Prefix(typeof(PlayerStatus).GetMethod("PlayerDied", Patching.AnyMethod), typeof(MuckEssentials).GetMethod("PrefixNoDamage", Patching.AnyMethod));

            // Full HP
            Options.RegisterBool(optionFullHp, false);
            Options.SetDescription(optionFullHp, "Sets your HP to match your full HP.");
            Options.AddPersistence(optionFullHp);
            Patching.Postfix(typeof(PlayerStatus).GetMethod("Healing", Patching.AnyMethod), typeof(MuckEssentials).GetMethod("PostfixHealing", Patching.AnyMethod));

            // Unlimited Stamina
            Options.RegisterBool(optionUnlimitedStamina, false);
            Options.SetDescription(optionUnlimitedStamina, "Gives you unlimited stamina.");
            Options.AddPersistence(optionUnlimitedStamina);
            Patching.Postfix(typeof(PlayerStatus).GetMethod("Stamina", Patching.AnyMethod), typeof(MuckEssentials).GetMethod("PostfixStamina", Patching.AnyMethod));

            // Full Hunger
            Options.RegisterBool(optionFullHunger, false);
            Options.SetDescription(optionFullHunger, "Ensure your hunger bar remains full.");
            Options.AddPersistence(optionFullHunger);
            Patching.Postfix(typeof(PlayerStatus).GetMethod("Hunger", Patching.AnyMethod), typeof(MuckEssentials).GetMethod("PostfixHunger", Patching.AnyMethod));

            // Max Run Speed
            Options.RegisterFloat(optionMaxRunSpeed, maxRunSpeed);
            Options.SetDescription(optionMaxRunSpeed, "The max run speed you can achieve. The default is 13.");
            Options.AddToggle(optionMaxRunSpeed);
            Options.AddPersistence(optionMaxRunSpeed);

            // Max Walk Speed
            Options.RegisterFloat(optionMaxWalkSpeed, maxWalkSpeed);
            Options.SetDescription(optionMaxWalkSpeed, "The max walk speed you can achieve. The default is 6.5.");
            Options.AddToggle(optionMaxWalkSpeed);
            Options.AddPersistence(optionMaxWalkSpeed);

            // Infinite Jumps
            Options.RegisterBool(optionInfiniteJumps, enabledInfiniteJumps);
            Options.SetDescription(optionInfiniteJumps, "Give you an endless number of jumps.");
            Options.AddPersistence(optionInfiniteJumps);
            Patching.Prefix(typeof(PowerupInventory).GetMethod("GetExtraJumps", Patching.AnyMethod), typeof(MuckEssentials).GetMethod("PrefixGetExtraJumps", Patching.AnyMethod));

            // Override Coins
            Options.RegisterInt(optionOverrideCoins, overrideCoinsAmount);
            Options.SetDescription(optionOverrideCoins, "Override the total number of coins you have. You need to enable the toggle for this to apply.");
            Options.AddToggle(optionOverrideCoins);
            Options.SetMinValue(optionOverrideCoins, 0);
            Options.SetMaxValue(optionOverrideCoins, int.MaxValue);
            Options.AddPersistence(optionOverrideCoins);
            Patching.Prefix(typeof(InventoryUI).GetMethod("GetMoney", Patching.AnyMethod), typeof(MuckEssentials).GetMethod("PrefixGetMoney", Patching.AnyMethod));

            // Always craft
            Options.RegisterBool(optionCanAlwaysCraft, false);
            Options.SetDescription(optionCanAlwaysCraft, "Allows you to craft things even if you don't have enough items to craft it. This will still use items if you do have them.");
            Options.AddPersistence(optionCanAlwaysCraft);
            Patching.Prefix(typeof(InventoryUI).GetMethod("IsCraftable", Patching.AnyMethod), typeof(MuckEssentials).GetMethod("PrefixIsCraftable", Patching.AnyMethod));

            // Free Boat Upgrades
            Options.RegisterBool(optionFreeBoatUpgrades, false);
            Options.SetDescription(optionFreeBoatUpgrades, "This will allow you to repair the boat even if you don't have enough resources.");
            Options.AddPersistence(optionFreeBoatUpgrades);
            Patching.Prefix(typeof(InventoryUI).GetMethod("CanRepair", Patching.AnyMethod), this.GetType().GetMethod("PrefixCanRepair", Patching.AnyMethod));
            Patching.Prefix(typeof(InventoryUI).GetMethod("Repair", Patching.AnyMethod), this.GetType().GetMethod("PrefixCanRepair", Patching.AnyMethod));

            // Give Items
            Options.RegisterDropdown(optionGiveItemWhichItem, GetPossibleInventoryItems(), "");
            Options.SetDescription(optionGiveItemWhichItem, "The item to give when giving an item.");
            Options.AddPersistence(optionGiveItemWhichItem);
            Options.RegisterInt(optionItemGiveCount, 1);
            Options.SetDescription(optionItemGiveCount, "The number of a given item to give.");
            Options.SetMinValue(optionItemGiveCount, minGiveCount);
            Options.SetMaxValue(optionItemGiveCount, maxGiveCount);
            Options.AddPersistence(optionItemGiveCount);
            Options.RegisterAction(actionGiveItem, "Give Item");
            Options.SetDescription(actionGiveItem, "Give you an item based on the item selected from the dropdown menu.");
            Options.AddPersistence(actionGiveItem);

            // Give PowerUps
            Options.RegisterDropdown(optionGivePowerUpWhichPowerUp, GetPossiblePowerUps(), "");
            Options.SetDescription(optionGivePowerUpWhichPowerUp, "The powerup to give when giving a powerup.");
            Options.AddPersistence(optionGivePowerUpWhichPowerUp);
            Options.RegisterInt(optionPowerUpGiveGiveCount, 1);
            Options.SetDescription(optionPowerUpGiveGiveCount, "The number of a given powerup to give.");
            Options.SetMinValue(optionPowerUpGiveGiveCount, minGiveCount);
            Options.SetMaxValue(optionPowerUpGiveGiveCount, maxGiveCount);
            Options.AddPersistence(optionPowerUpGiveGiveCount);
            Options.RegisterAction(actionGivePowerUp, "Give Powerup");
            Options.SetDescription(actionGivePowerUp, "Give you an powerup based on the powerup selected from the dropdown menu.");
            Options.AddPersistence(actionGivePowerUp);

            // Skip Tutorial
            Options.RegisterBool(optionSkipTutorial, false);
            Options.SetDescription(optionSkipTutorial, "Will prevent the tutorial from coming up, or close the tutorial if it has already started.");
            Options.AddPersistence(optionSkipTutorial);

            // Start management of movement speed
            StartCoroutine(this.SlowUpdate());
        }

        void OnModUnloaded()
        {
            // Undo everything, your mod is being unloaded

            // Options are automatically unregistered
            // Patches via the AzzaMods patching system are automatically unhooked
        }

        void OnOptionChanged(string optionName)
        {
            // No Damage
            if(optionName == optionNoDamage)
            {
                enabledNoDamage = Options.GetBool(optionName);
            }

            // Full HP
            if(optionName == optionFullHp)
            {
                enabledFullHP = Options.GetBool(optionName);
            }

            // Unlimited Stamina
            if(optionName == optionUnlimitedStamina)
            {
                enabledUnlimitedStamina = Options.GetBool(optionName);
            }

            // Full Hunger
            if(optionName == optionFullHunger)
            {
                enabledFullHunger = Options.GetBool(optionName);
            }

            // Max run speed
            if (optionName == optionMaxRunSpeed)
            {
                maxRunSpeed = Options.GetFloat(optionName);
                ApplyMaxRunAndWalkSpeeds();
            }

            // Max walk speed
            if (optionName == optionMaxWalkSpeed)
            {
                maxWalkSpeed = Options.GetFloat(optionName);
                ApplyMaxRunAndWalkSpeeds();
            }

            // Override Coin Count
            if(optionName == optionOverrideCoins)
            {
                overrideCoinsAmount = Options.GetInt(optionName);
                enabledOverrideCoins = Options.GetToggleState(optionName);
            }

            // Always craft
            if(optionName == optionCanAlwaysCraft)
            {
                enableAlwaysCraft = Options.GetBool(optionName);
            }

            // Inifite jumps
            if(optionName == optionInfiniteJumps)
            {
                enabledInfiniteJumps = Options.GetBool(optionName);
            }

            // Skip tutorial
            if(optionName == optionSkipTutorial)
            {
                // Is it enabled?
                if(Options.GetBool(optionName))
                {
                    // Kill any gameobjects related to it
                    foreach(Tutorial tutorial in FindObjectsOfType<Tutorial>())
                    {
                        try
                        {
                            Traverse traverse = new Traverse(tutorial);
                            traverse.Field<TutorialTaskUI>("currentTaskUi").Value.StartFade();
                        }
                        catch
                        {
                            // do nothing
                        }

                        Destroy(tutorial.gameObject);
                    }

                    // Disble it
                    CurrentSettings.Instance.tutorial = false;
                }
            }
        }

        void OnSceneChanged(string oldScene, string newScene)
        {
            // Update the list of items that is available
            Options.SetDropdownOptions(optionGiveItemWhichItem, GetPossibleInventoryItems());
            Options.SetDropdownOptions(optionGivePowerUpWhichPowerUp, GetPossiblePowerUps());
        }

        private List<string> GetPlayerNames()
        {
            List<string> players = new List<string>();

            try
            {
                foreach(KeyValuePair<int, PlayerManager> pair in GameManager.players)
                {
                    players.Add(pair.Value.username);
                }
            }
            catch
            {
                // do nothing
            }

            return players;
        }

        private List<string> cachedPlayers = new List<string>();

        // Run once every second or so
        private IEnumerator SlowUpdate()
        {
            while(true)
            {
                // Wait a second
                yield return new WaitForSeconds(1f);

                // Apply it
                ApplyMaxRunAndWalkSpeeds();

                // Update player list
                List<string> newPlayerList = GetPlayerNames();
                if(cachedPlayers.Count != newPlayerList.Count)
                {
                    cachedPlayers = newPlayerList;
                    Options.SetDropdownOptions(optionPlayerToRevive, newPlayerList);
                }
            }
        }

        private static void ApplyMaxRunAndWalkSpeeds()
        {
            FieldInfo fieldMaxRunSpeed = typeof(PlayerMovement).GetField("maxRunSpeed", Patching.AnyMethod);
            FieldInfo fieldMaxWalkSpeed = typeof(PlayerMovement).GetField("maxWalkSpeed", Patching.AnyMethod);

            foreach (PlayerMovement plyMovement in FindObjectsOfType<PlayerMovement>())
            {
                if(Options.GetToggleState(optionMaxRunSpeed))
                {
                    fieldMaxRunSpeed.SetValue(plyMovement, maxRunSpeed);
                }

                if (Options.GetToggleState(optionMaxWalkSpeed))
                {
                    fieldMaxWalkSpeed.SetValue(plyMovement, maxWalkSpeed);
                }
            }
        }

        private static List<string> GetPossibleInventoryItems()
        {
            List<string> possibleItems = new List<string>();

            try
            {
                foreach (KeyValuePair<int, InventoryItem> pair in ItemManager.Instance.allItems)
                {
                    possibleItems.Add(pair.Value.name);
                }
            }
            catch
            {
                // do nothing
            }

            possibleItems.Sort();

            return possibleItems;
        }

        private static List<string> GetPossiblePowerUps()
        {
            List<string> possiblePowerUps = new List<string>();

            try
            {
                foreach (KeyValuePair<int, Powerup> pair in ItemManager.Instance.allPowerups)
                {
                    possiblePowerUps.Add(pair.Value.name);
                }
            }
            catch
            {
                // do nothing
            }

            possiblePowerUps.Sort();

            return possiblePowerUps;
        }

        // When an action happens
        void OnAction(string actionName, string actionType)
        {
            // Unlock everything
            if (actionName == actionUnlockAllItemsAndStations)
            {
                if(UiEvents.Instance != null)
                {
                    // Grab the private unlock method
                    MethodInfo methodUnlockItemSoft = typeof(UiEvents).GetMethod("UnlockItemSoft", Patching.AnyMethod);

                    // Cycle every item
                    try
                    {
                        foreach (KeyValuePair<int, InventoryItem> pair in ItemManager.Instance.allItems)
                        {
                            // Unlock stations, hard and soft items
                            UiEvents.Instance.CheckProcessedItem(pair.Value.id);
                            UiEvents.Instance.StationUnlock(pair.Value.id);
                            methodUnlockItemSoft.Invoke(UiEvents.Instance, new object[] { pair.Value.id });
                        }
                    }
                    catch
                    {
                        // do nothing
                    }
                }
            }

            // Revive all players
            if(actionName == actionReviveAllPlayers)
            {
                // Loop over each client in the server
                foreach(KeyValuePair<int, Client> pair in Server.clients)
                {
                    // Grab the client
                    Client thisClient = pair.Value;

                    // Grab player
                    Player ply = thisClient.player;
                    if (ply != null)
                    {
                        // Are they dead?
                        if (ply.dead)
                        {
                            // Buff to at least 100 hp
                            if (ply.currentHp < 100)
                            {
                                ply.currentHp = 100;
                            }

                            // No longer dead
                            ply.dead = false;

                            // Send message to say that the player was revived
                            //ServerSend.RevivePlayer(-1, ply.id, true, -1);
                            ClientSend.RevivePlayer(ply.id, -1, true);
                        }
                    }
                }

                // Ensure the game over screen isnt showing
                try
                {
                    GameManager.instance.gameoverUi.SetActive(false);
                }
                catch
                {
                    // do nothing
                }

                // Fix cursor issues
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Try to start game
                try
                {
                    GameManager.instance.StartGame();
                }
                catch
                {

                }
            }

            // Give Item
            if (actionName == actionGiveItem)
            {
                // Grab the name of the item we need to give
                string itemToGive = Options.GetString(optionGiveItemWhichItem);

                // Are the controllers available?
                if (InventoryUI.Instance != null)
                {
                    // Find an instance of the item we want to give
                    InventoryItem toGive = null;

                    try
                    {
                        foreach (KeyValuePair<int, InventoryItem> pair in ItemManager.Instance.allItems)
                        {
                            if (pair.Value.name == itemToGive)
                            {
                                toGive = pair.Value;
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // do nothing
                    }

                    // Did we find an item to give?
                    if (toGive != null)
                    {
                        // How many to give?
                        int numberToGive = (int)Mathf.Clamp(Options.GetInt(optionItemGiveCount), minGiveCount, maxGiveCount);

                        for(int i=0; i< numberToGive; ++i)
                        {
                            InventoryUI.Instance.AddItemToInventory(toGive);
                        }
                    }
                }
            }

            // Give Powerup
            if (actionName == actionGivePowerUp)
            {
                // Grab the name of the item we need to give
                string powerUpToGive = Options.GetString(optionGivePowerUpWhichPowerUp);

                // Are the controllers available?
                if (PowerupInventory.Instance != null)
                {
                    // Find an instance of the item we want to give
                    Powerup toGive = null;
                    try
                    {
                        foreach (KeyValuePair<int, Powerup> pair in ItemManager.Instance.allPowerups)
                        {
                            if (pair.Value.name == powerUpToGive)
                            {
                                toGive = pair.Value;
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // do nothing
                    }

                    // Did we find an item to give?
                    if (toGive != null)
                    {
                        // How many to give?
                        int numberToGive = (int)Mathf.Clamp(Options.GetInt(optionPowerUpGiveGiveCount), minGiveCount, maxGiveCount);

                        string itemToGiveName = toGive.name;
                        int itemToGiveId = toGive.id;

                        // This will always generate an exception cause hte itemId is null, but, it will still give the powerup
                        for (int i = 0; i < numberToGive; ++i)
                        {
                            try
                            {
                                PowerupInventory.Instance.AddPowerup(itemToGiveName, itemToGiveId, -1);
                            }
                            catch
                            {
                                // do nothing
                            }
                        }
                        
                    }
                }
            }

            // Kill All Mobs
            if(actionName == optionKillAllMobs)
            {
                foreach (MobServer monster in FindObjectsOfType<MobServer>())
                {
                    try
                    {
                        Destroy(monster.gameObject);
                    }
                    catch
                    {
                        // do nothing
                    }
                }

                foreach (Mob monster in FindObjectsOfType<Mob>())
                {
                    try
                    {
                        Destroy(monster.gameObject);
                    }
                    catch
                    {
                        // do nothing
                    }
                }
            }

            // Revive specific player
            if(actionName == actionReviveSpecificPlayer)
            {
                string nameToMatch = Options.GetString(optionPlayerToRevive);

                try
                {
                    foreach (KeyValuePair<int, PlayerManager> pair in GameManager.players)
                    {
                        if(pair.Value.username == nameToMatch)
                        {
                            ClientSend.RevivePlayer(pair.Value.id, -1, true);
                        }
                    }
                }
                catch
                {
                    // do nothing
                }

                // Ensure the game over screen isnt showing
                try
                {
                    GameManager.instance.gameoverUi.SetActive(false);
                }
                catch
                {
                    // do nothing
                }

                // Fix cursor issues
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Try to start game
                try
                {
                    GameManager.instance.StartGame();
                }
                catch
                {

                }
            }
        }

        // No Damage
        private static bool PrefixNoDamage()
        {
            // If enabled, don't run original function
            if(enabledNoDamage)
            {
                return false;
            }

            // Do run original function
            return true;
        }

        private static void PostfixStamina(PlayerStatus __instance)
        {
            // Is our option enabled?
            if(enabledUnlimitedStamina)
            {
                // Set the current stamina to the max stamina
                __instance.stamina = __instance.maxStamina;
            }
        }

        private static void PostfixHunger(PlayerStatus __instance)
        {
            // Is our option enabled?
            if(enabledFullHunger)
            {
                // Set current hunger to max hunger
                __instance.hunger = __instance.maxHunger;
            }
        }

        private static void PostfixHealing(PlayerStatus __instance)
        {
            // Option enabled?
            if(enabledFullHP)
            {
                // Set hp to max hp
                __instance.hp = __instance.maxHp;
            }
        }

        private static bool PrefixGetMoney(ref int __result)
        {
            // Is the option enabled
            if(enabledOverrideCoins)
            {
                // Update the override amount
                __result = overrideCoinsAmount;

                // Don't run original method
                return false;
            }

            // Run original Method
            return true;
        }

        private static bool PrefixIsCraftable(ref bool __result)
        {
            // Option enabled?
            if(enableAlwaysCraft)
            {
                // Set the answer to yes
                __result = true;

                // Don't run original method
                return false;
            }

            // Do run original method
            return true;
        }

        private static bool PrefixGetExtraJumps(ref int __result)
        {
            // Option enabled?
            if(enabledInfiniteJumps)
            {
                // Set ma jumps to ma
                __result = int.MaxValue;

                // Don't run original method
                return false;
            }

            // Run original method
            return true;
        }

        // Boat repair
        private static bool PrefixCanRepair(ref bool __result)
        {
            // Free boat repair enabled?
            if(Options.GetBool(optionFreeBoatUpgrades))
            {
                // Result is true
                __result = true;

                // Don't run original method
                return false;
            }

            // Run original method
            return true;
        }

        // Prevent mob spawning
        private static bool PrefixBlockMobSpawning()
        {
            // Is mob spawning disbled?
            if(Options.GetBool(optionPreventMobSpawn))
            {
                // Don't run the mob spawn code
                return false;
            }

            // Run original method
            return true;
        }
    }
}
