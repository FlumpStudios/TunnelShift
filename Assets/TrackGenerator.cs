using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    public Vector3 IntialPosition;
    public Vector3 OuterInitialPosition;
    public GameObject LevelUpObject;
    
    private int LevelUpObjectSpawnInterval = 1;

    public GameObject[] OuterTrackPieces;
    public List<TrackPieces> TrackPieces = new List<TrackPieces>();
    public int pieceSize = 250;
    public int TrackPiecesToGenerateUpFront;
    private int currentCount;
    private int SpawnCountSinceLastLevelUp = 0;
    private int trackPiecesSpawned = 0;     

    private TrackPieces LastSpawnedPiece = null;

    void Awake()
    {
        for (int i = 0; i < TrackPiecesToGenerateUpFront; i++)
        {
            SpawnTrackPart();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TrackSpawnTrigger"))
        {
            SpawnTrackPart();
            trackPiecesSpawned++;
            if (trackPiecesSpawned > 5)
            { 
                //Destroy(GameObject.FindGameObjectsWithTag("TrackParent").FirstOrDefault());
            }
        }
    }

    private void SpawnTrackPart()
    {
        TrackPieces nextPiece = GetNextStaticMeshPieceToGenerate();

        var newLocal = new Vector3(IntialPosition.x, IntialPosition.y, IntialPosition.z + currentCount);


        SpawnCountSinceLastLevelUp++;
        if (SpawnCountSinceLastLevelUp > LevelUpObjectSpawnInterval)
        {
            Vector3 spawnLocale = new(-1.60f, 12.104f, newLocal.z);
            Instantiate(LevelUpObject, spawnLocale, Quaternion.Euler(90, 90, 90));

            SpawnCountSinceLastLevelUp = 0;

            // Further through the game, spawn less often
            LevelUpObjectSpawnInterval += 2;            
        }
        
        if (!IsOkToSpawn(nextPiece))
        {
            nextPiece = GetSafeTrackPiece;
        }        

        Instantiate(nextPiece.StaticMesh, newLocal, Quaternion.identity);
        LastSpawnedPiece = nextPiece;

        var newOuterLocal = new Vector3(OuterInitialPosition.x, OuterInitialPosition.y, OuterInitialPosition.z + currentCount);

        foreach (var OuterTrackPiece in OuterTrackPieces)
        {
            Instantiate(OuterTrackPiece, newOuterLocal, Quaternion.identity);
        }
        currentCount += pieceSize;
    }

    private bool IsOkToSpawn(in TrackPieces piece)
    {
        if (LevelManager.GetCurrentLevel() <= piece.LowestLevelCanSpawn)
        {
            return false;
        }

        if (LastSpawnedPiece == null || !piece.PreviousTrackPieceExclusion.Any()) { return true; }

        foreach (var x in piece.PreviousTrackPieceExclusion)
        {
            if (string.Equals(LastSpawnedPiece.StaticMesh.name, x.name, StringComparison.OrdinalIgnoreCase)
                || string.Equals(string.Concat(LastSpawnedPiece.StaticMesh.name,"(clone)"), x.name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;        
    }

    int oldStage = 0;
    private TrackPieces GetNextStaticMeshPieceToGenerate()
    {

        return TrackPieces.Where(x => x.Stage == LevelManager.GetCurrentStage()).ToList()[UnityEngine.Random.Range(0, TrackPieces.Where(x => x.Stage == LevelManager.GetCurrentStage()).Count())];
    }


// Get piece that have no exlusions
private TrackPieces GetSafeTrackPiece =>
        TrackPieces[0];
    // TrackPieces[UnityEngine.Random.Range(0, 0)];
}

[Serializable]
public class TrackPieces
{
    public GameObject StaticMesh;
    public List<GameObject> PreviousTrackPieceExclusion;
    public int LowestLevelCanSpawn = 0;
    public int Stage = 0;
}
