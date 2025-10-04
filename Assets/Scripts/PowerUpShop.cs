using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PowerUpShop : MonoBehaviour
{
    [Header("Spawning Power Ups")]
    [SerializeField] private float spaceOffset = 3.0f;
    [SerializeField] private int powerUpCount = 3;
    [SerializeField] private GameObject[] powerUpPrefabs;

    [Header("Power Up Selection")]
    [SerializeField] private Color outlineColor = new Color(0.4f, 0.0f, 0.7f);
    [SerializeField] private float outlineWidth = 5.0f;

    private Transform _highlight;
    private Transform _selection;
    private RaycastHit _raycastHit;
    private bool _hasSelected;
    private MoneyManagement _moneyManagement;
    
    private void Awake()
    {
        _moneyManagement = GetComponent<MoneyManagement>();
        if (powerUpPrefabs == null || powerUpPrefabs.Count() < powerUpCount)
            Debug.Log("Not enough power up prefabs added!");
    }

    void Start()
    {
        SpawnPowerUp();
    }

    void Update()
    {
        HighlightPowerUp();
        SelectPowerUp();
    }

    private void SpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Count() < powerUpCount) return;
        
        for (int i = 0; i < powerUpCount; i++)
        {
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            Vector3 prefabPosition = transform.position + Vector3.up * i * spaceOffset;
            Instantiate(powerUpPrefabs[randomIndex], prefabPosition, Quaternion.identity, transform);
        }
    }

    private void HighlightPowerUp()
    {
        if (_hasSelected) return;
        
        if (_highlight)
        {
            _highlight.gameObject.GetComponent<Outline>().enabled = false;
            _highlight = null;
        }
        
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out _raycastHit))
        {
            _highlight = _raycastHit.transform;
            if (_highlight.CompareTag($"Selectable") && _highlight != _selection)
            {
                var outline = _highlight.gameObject.GetComponent<Outline>();
                if (outline) outline.enabled = true;
                else
                {
                    outline = _highlight.gameObject.AddComponent<Outline>();
                    outline.enabled = true;
                    
                    outline = _highlight.gameObject.GetComponent<Outline>();
                    outline.OutlineColor = outlineColor;
                    outline.OutlineWidth = outlineWidth;
                }
            }
            else _highlight = null;
        }
    }

    private void SelectPowerUp()
    {
        if (!_hasSelected && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_highlight)
            {
                _selection = _raycastHit.transform;
                _selection.gameObject.GetComponent<Outline>().enabled = true;
                _highlight = null;
                _hasSelected = true;

                var selectionInfo = _selection.gameObject.GetComponent<PowerUpInfo>();
                selectionInfo.isSelected = true;
                _moneyManagement.LoseAmount(selectionInfo.price);
            }
        }
    }
    
}
