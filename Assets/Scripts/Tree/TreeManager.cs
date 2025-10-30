using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TreeManager : MonoBehaviour
{
    public static TreeManager instance;

    private List<CutTree> cutTrees = new List<CutTree>();
    private CropsManager cropsManager;
    private Tilemap groundTilemap;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        cropsManager = FindObjectOfType<CropsManager>();
        if (cropsManager == null)
        {
            Debug.LogError("TreeManager: CropsManager not found in scene. Disabling TreeManager.");
            enabled = false;
            return;
        }

        GameObject groundObject = GameObject.Find("Ground");
        if (groundObject == null)
        {
            Debug.LogError("TreeManager: 'Ground' GameObject not found in scene. Disabling TreeManager.");
            enabled = false;
            return;
        }

        groundTilemap = groundObject.GetComponent<Tilemap>();
        if (groundTilemap == null)
        {
            Debug.LogError("TreeManager: Tilemap component not found on 'Ground' GameObject. Disabling TreeManager.");
            enabled = false;
            return;
        }
    }

    public void RegisterCutTree(GameObject tree)
    {
        tree.SetActive(false);
        cutTrees.Add(new CutTree(tree, Time.time));
    }

    private void Update()
    {
        for (int i = cutTrees.Count - 1; i >= 0; i--)
        {
            if (Time.time - cutTrees[i].cutTime >= 30f)
            {
                if (IsSpotEmpty(cutTrees[i].tree.transform.position))
                {
                    cutTrees[i].tree.SetActive(true);
                    // Reset hit count
                    TreeCuttable treeCuttable = cutTrees[i].tree.GetComponent<TreeCuttable>();
                    if (treeCuttable != null)
                    {
                        treeCuttable.ResetHitCount();
                    }
                    cutTrees.RemoveAt(i);
                }
            }
        }
    }

    private bool IsSpotEmpty(Vector3 position)
    {
        Vector3Int cellPosition = groundTilemap.WorldToCell(position);
        return !cropsManager.crops.ContainsKey(cellPosition);
    }

    private class CutTree
    {
        public GameObject tree;
        public float cutTime;

        public CutTree(GameObject tree, float cutTime)
        {
            this.tree = tree;
            this.cutTime = cutTime;
        }
    }
}
