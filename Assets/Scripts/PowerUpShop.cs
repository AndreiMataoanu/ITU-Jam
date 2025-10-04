using System.Collections;
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
    [SerializeField] private float disappearTime = 1.0f;

    private Transform _highlight;
    private Transform _selection;
    private RaycastHit _raycastHit;
    private bool _hasSelected;
    private MoneyManagement _moneyManagement;
    private InventoryManagement _inventoryManagement;
    
    private void Awake()
    {
        _moneyManagement = GetComponent<MoneyManagement>();
        _inventoryManagement = GetComponent<InventoryManagement>();
        
        if (powerUpPrefabs == null || powerUpPrefabs.Count() < powerUpCount)
            Debug.Log("Not enough power up prefabs added!");
    }

    void Update()
    {
        HighlightPowerUp();
        SelectPowerUp();
    }

    public void SpawnPowerUps()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Count() < powerUpCount) return;
        
        for (int i = 0; i < powerUpCount; i++)
        {
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            Vector3 prefabPosition = transform.position + Vector3.up * (i * spaceOffset);
            Instantiate(powerUpPrefabs[randomIndex], prefabPosition, Quaternion.identity, transform);
        }
    }

    public void DestroyPowerUps()
    {
        for (int i = 0; i < powerUpCount - 1; i++)
        {
            GameObject powerUp = gameObject.transform.GetChild(i).gameObject;
            Destroy(powerUp);
        }
    }

    private void HighlightPowerUp()
    {
        if (_inventoryManagement.inInventory || _hasSelected) return;
        
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
        if (_inventoryManagement.inInventory) return;
        
        if (!_hasSelected && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_highlight)
            {
                _selection = _raycastHit.transform;
                _selection.gameObject.GetComponent<Outline>().enabled = false;
                _highlight = null;
                _hasSelected = true;

                BuySelectedPowerUp();
                // TODO: After buying, change camera direction
            }
        }
    }

    private void BuySelectedPowerUp()
    {
        var selectionInfo = _selection.gameObject.GetComponent<PowerUpInfo>();
                
        if (_inventoryManagement.AddItem(_selection.gameObject))
            _moneyManagement.LoseAmount(selectionInfo.price);
    }
}
