using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LandminePlacer;

public class LandminePlacer : PhysicsProp
{
    private readonly NetworkVariable<int> _maxLandmines = new ();
    private readonly NetworkVariable<bool> _infiniteLandmines = new ();
    private readonly NetworkVariable<int> _landminesPlaced = new ();
    
    public override void OnNetworkSpawn()
    {
        base.Start();
        if (!IsHost) return;
        _maxLandmines.Value = Config.MaxLandmines?.Value ?? 3;
        _infiniteLandmines.Value = Config.InfiniteLandmines?.Value ?? false;
        UpdateGrabTooltipClientRpc(_landminesPlaced.Value, _maxLandmines.Value, _infiniteLandmines.Value);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        if (IsOwner)
        {
            TryPlaceLandmine(buttonDown);
        }
    }

    public override void EquipItem()
    {
        UpdateItemTooltip(_landminesPlaced.Value, _maxLandmines.Value, _infiniteLandmines.Value);
        base.EquipItem();
    }

    public override void DiscardItem()
    {
        UpdateGrabTooltipServerRpc();
        base.DiscardItem();
    }

    public void TryPlaceLandmine(bool buttonDown)
    {
        if (buttonDown == false) return;
        if (IsPlayer())
        {
            if (CanPlaceLandmine())
            {
                PlaceLandMine(playerHeldBy.transform);
                LandminePlacedServerRpc();
            }
            else
            {
                DestroyObjectInHandClientRpc();
            }
        }
        else
        {
            PlaceLandMine(parentObject.transform);
            LandminePlacedServerRpc();
        }
    }

    private void PlaceLandMine(Transform userTransform)
    {
        var userPosition = userTransform.position;
        var pos = userPosition + userTransform.forward * 1.5f;
        PlaceLandmineAtServerRpc(pos);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void PlaceLandmineAtServerRpc(Vector3 position)
    {
        var instantiatedLandmine = Instantiate(Plugin.Landmine, position, Quaternion.identity);
        instantiatedLandmine.SetActive(true);
            
        var networkObject = instantiatedLandmine.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Plugin.Log.LogError("NetworkObject is missing on the instantiated landmine.");
            return;
        }
        networkObject.Spawn();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void LandminePlacedServerRpc()
    {
        if (_infiniteLandmines.Value) return;
        _landminesPlaced.Value ++;
        if (_landminesPlaced.Value >= _maxLandmines.Value)
        {
            DestroyObjectInHandClientRpc();
        }
        else
        {
            Plugin.Log.LogInfo( $"Landmines placed count {_landminesPlaced} !");
        }
        UpdateItemTooltip(_landminesPlaced.Value, _maxLandmines.Value, _infiniteLandmines.Value);
    }

    [ClientRpc]
    private void DestroyObjectInHandClientRpc()
    {
        DestroyObjectInHand(playerHeldBy);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdateGrabTooltipServerRpc()
    {
        UpdateGrabTooltipClientRpc(_landminesPlaced.Value, _maxLandmines.Value, _infiniteLandmines.Value);
    }
    
    [ClientRpc]
    public void UpdateGrabTooltipClientRpc(int landminesPlaced, int maxLandmines, bool infiniteLandmines)
    {
        if (!infiniteLandmines)
        {
            //write the number of remaining landmines
            customGrabTooltip = $"{maxLandmines - landminesPlaced} Landmines remaining";
        }
        else
        {
            customGrabTooltip = "Infinite Landmines";
        }
    }
    
    public void UpdateItemTooltip(int landminesPlaced, int maxLandmines, bool infiniteLandmines = false)
    {
        var player = GameNetworkManager.Instance.localPlayerController;
        if (player == null) return;
        if (player.playerActions?.m_Movement_Use == null || itemProperties == null)
        {
            Plugin.Log.LogError("Cannot update ItemTooltip, One of the required objects is null.");
            return;
        }

        const string placeLandmineTooltip = "Press {0} to place a landmine";
        var landmineRemainingTooltip = !_infiniteLandmines.Value ? $"{_maxLandmines.Value - _landminesPlaced.Value} Landmines remaining" : null;

        var tooltips = new List<string>();
        if (landmineRemainingTooltip != null)
        {
            tooltips.Add(landmineRemainingTooltip);
        }
        
        var useKey = player.playerActions.m_Movement_Use.GetBindingDisplayString(0);
        tooltips.Add(string.Format(placeLandmineTooltip,$"[{useKey}]"));

        itemProperties.toolTips = tooltips.ToArray();
    }

    private bool IsPlayer()
    {
        return playerHeldBy != null && !isHeldByEnemy;
    }
    
    private bool CanPlaceLandmine()
    {
        return _landminesPlaced.Value < _maxLandmines.Value || _infiniteLandmines.Value;
    }
    
    public void InitProperties(PhysicsProp prop)
    {
        // Initialise PlaceableLandmine properties with PhysicsProp ones
        grabbable = prop.grabbable;
        isHeld = prop.isHeld;
        isHeldByEnemy = prop.isHeldByEnemy;
        deactivated = prop.deactivated;
        parentObject = prop.parentObject;
        targetFloorPosition = prop.targetFloorPosition;
        startFallingPosition = prop.startFallingPosition;
        floorYRot = prop.floorYRot;
        fallTime = prop.fallTime;
        hasHitGround = prop.hasHitGround;
        scrapValue = prop.scrapValue;
        itemUsedUp = prop.itemUsedUp;
        playerHeldBy = prop.playerHeldBy;
        isPocketed = prop.isPocketed;
        isBeingUsed = prop.isBeingUsed;
        isInElevator = prop.isInElevator;
        isInShipRoom = prop.isInShipRoom;
        isInFactory = prop.isInFactory;
        useCooldown = prop.useCooldown;
        currentUseCooldown = prop.currentUseCooldown;
        itemProperties = prop.itemProperties;
        insertedBattery = prop.insertedBattery;
        customGrabTooltip = prop.customGrabTooltip;
        propBody = prop.propBody;
        propColliders = prop.propColliders;
        originalScale = prop.originalScale;
        wasOwnerLastFrame = prop.wasOwnerLastFrame;
        mainObjectRenderer = prop.mainObjectRenderer;
        isSendingItemRPC = prop.isSendingItemRPC;
        scrapPersistedThroughRounds = prop.scrapPersistedThroughRounds;
        heldByPlayerOnServer = prop.heldByPlayerOnServer;
        radarIcon = prop.radarIcon;
        reachedFloorTarget = prop.reachedFloorTarget;
        grabbableToEnemies = prop.grabbableToEnemies;
        hasBeenHeld = prop.hasBeenHeld;
    }
    
    public override string __getTypeName() => nameof(LandminePlacer);
}