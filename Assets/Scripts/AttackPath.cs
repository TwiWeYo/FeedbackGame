using System.Collections.Generic;
using UnityEngine;

public class AttackPath : MonoBehaviour
{
    [SerializeField]
    public GameObject attackPathIndicator;
    [SerializeField]
    public GameObject attackTargetIndicator;
    [SerializeField]
    public GameObject attackGhostIndicator;

    private List<GameObject> pathVizualization = new List<GameObject>();

    public List<Vector3Int> Path { get; private set; } = new();

    public void SetPath(List<Vector3Int> path)
    {
        Path = path;
    }

    public void AddPointToPath(GameObject obj)
    {
        pathVizualization.Add(obj);
        obj.SetActive(false);
    }

    public void DestroySelfAndChildren(AttackPath attackPath)
    {
        Destroy(attackPath.gameObject);
        Destroy(attackPath);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ActivatePathHighlight(true);
        }

        else if (Input.GetKeyUp(KeyCode.E))
        {
            ActivatePathHighlight(false);
        }
    }

    private void OnDestroy()
    {
        foreach (var path in pathVizualization)
        {
            Destroy(path);
        }
    }

    private void ActivatePathHighlight(bool active)
    {
        foreach (var path in pathVizualization)
        {
            path.SetActive(active);
        }
    }

}
