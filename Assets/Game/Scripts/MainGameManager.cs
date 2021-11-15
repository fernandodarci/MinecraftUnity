using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class MainGameManager : MonoBehaviour
{
    [Header("BasicData")]
    public string WorldName;
    public int Seed;

    [Header("Procedural Components")]
    public Canvas GameCanvas;
    public PlayerController playerBase;
    public ProceduralSector sectorBase;
    public ProceduralWorld worldBase;
    public Transform Target;
    //public RectTransform t;
    public static SolidBlock[] BlockType => new SolidBlock[] 
    { 
        new SolidBlock(), 
        new Bedrock(), 
        new Stone(), 
        new Dirt(), 
        new Grass(), 
        new Sand()
    };

    [Header("Biomes")]
    public Biome[] Biomes;

    [Header("Material Collection")]
    public Material OpaqueMaterial;
    public Material BiomeMaterial;

    [Header("Test")]
    public TMP_Text marker;
       
    public static MainGameManager GM;

    private Vector3Int playerPosition;
    private Vector3Int playerPreviousPosition;
    private PlayerController mainPlayer;
    private ProceduralWorld proceduralWorld;
    public ProceduralWorld World => proceduralWorld;
    

    public void Awake()
    {
        if (GM != this) GM = this;
        DontDestroyOnLoad(GM);
    }

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        proceduralWorld = Instantiate(worldBase);
        proceduralWorld.InitializeWorld();
       
        mainPlayer = Instantiate(playerBase, transform);
        mainPlayer.transform.position = 
          new Vector3(GameData.SectorSize / 2, GameData.SectorHeight + 10, GameData.SectorSize / 2);
        playerPosition = new Vector3Int(mainPlayer.transform.position.x.ToInt(),0,mainPlayer.transform.position.z.ToInt());
        playerPreviousPosition = playerPosition;
        Target.gameObject.SetActive(false);
        mainPlayer.Target = Target;
        
    }

    private void Update()
    {
        //t.RotateUIElement(angle);
        
        playerPosition = new Vector3Int(mainPlayer.transform.position.x.ToInt(), 0, mainPlayer.transform.position.z.ToInt());
        if (playerPosition != playerPreviousPosition)
        {
            playerPreviousPosition = playerPosition;
            proceduralWorld.PlayerPosition = playerPosition;
            proceduralWorld.UpdateWorld();
           
        }
    }
}