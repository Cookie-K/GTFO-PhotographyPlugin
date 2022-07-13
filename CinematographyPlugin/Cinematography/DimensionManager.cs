using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CinematographyPlugin.UI.UiInput;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography;

public class DimensionManager : MonoBehaviour
{
    public static Action<Vector3> OnDimensionWarp;
    
    private readonly Dictionary<eDimensionIndex, Vector3> _prevPositionByIndex = new ();
    private readonly Dictionary<eDimensionIndex, LG_Dimension> _dimensionByIndex = new ();
    private readonly List<eDimensionIndex> _dimensionIndices = new ();
    private int _currentIndex;

    private void Start()
    {
        var dimensions = FindObjectsOfType<LG_DimensionRoot>();
        _dimensionIndices.AddRange(dimensions.Select(d => d.LinkedDimensionIndex));

        foreach (var dimensionRoot in dimensions)
        {
            var dimension = dimensionRoot.GetComponentInChildren<LG_Dimension>();
            var playerSpawnPoint = dimension.GetPlayerSpawnPoint(PlayerManager.GetLocalPlayerAgent());
            _prevPositionByIndex[dimensionRoot.LinkedDimensionIndex] = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            _dimensionByIndex[dimensionRoot.LinkedDimensionIndex] = dimension;
        }
    }

    private void Update()
    {
        if (CinemaCamManager.Current.FreeCamEnabled() && KeyBindInputManager.GetDimensionWarp())
        {
            GotoDimension();
        }
    }

    private void GotoDimension()
    {
        var player = PlayerManager.GetLocalPlayerAgent();
        var index = _dimensionIndices[_currentIndex];
        var target = _prevPositionByIndex[index];
        
        _prevPositionByIndex[player.DimensionIndex] = CinemaCamManager.Current.GetOriginalPlayerPosition();
        
        if (!player.TryWarpTo(index, target, player.Forward, true))
        {
            var spawnPoint = _dimensionByIndex[index].GetPlayerSpawnPoint(PlayerManager.GetLocalPlayerAgent());
            target = spawnPoint.position;
            player.TryWarpTo(index, target, spawnPoint.forward, true);
        }
        
        _currentIndex++;
        if (_dimensionIndices.Count == _currentIndex)
        {
            _currentIndex = 0;
        }

        CinemaCamManager.Current.UpdatePlayerPositionAndShield(player, target, true);
        OnDimensionWarp.Invoke(target);
    }
}