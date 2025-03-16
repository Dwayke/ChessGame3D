using System.Collections.Generic;
using UnityEngine;

public class ChessCommonTypes : MonoBehaviour
{
}
[System.Serializable]
public class SpawnParameters
{
    public Material blackTileMaterial;
    public Material whiteTileMaterial;
    public float tileSize = 1;
    public Vector3 boardCenter = Vector3.zero;
    public float yOffset = 0;
}
[System.Serializable]
public class DeathParameters
{
    public float deathStartOffsetModifier = 9f;
    public float deathYOffsetModifier = 1.25f;
    public float deathDistanceOffsetModifier = 3;
    public float deathSize = 0.3f;
    public float deathSpacing = 0.3f;
    public readonly List<ChessPiece> deadWhites = new();
    public readonly List<ChessPiece> deadBlacks = new();
}
[System.Serializable]
public class Skins
{
    public EPieceSkin whitePlayerSkin;
    public EPieceSkin blackPlayerSkin;
    public ESkin boardSkin;
}

public enum ESpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion = 3
}
public enum ETurnTime
{
    None = 0,
    ThirtySec = 1,
    SixtySec = 2,
    NinetySec = 3,
    FiveMinutes = 4
}