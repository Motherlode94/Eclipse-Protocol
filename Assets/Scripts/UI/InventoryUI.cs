using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Références des Prefabs")]
    public GameObject inventorySlotPrefab;

    [Header("Références UI")]
    public Transform gridParent;
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;
    public TextMeshProUGUI emptyInventoryMessage;

    private InventoryManager inventoryManager;
    private GameObject selectedSlot;

    private void Start()
    {
        // Cherche le InventoryManager dans la scène
        inventoryManager = FindObjectOfType<InventoryManager>();

        if (!ValidateReferences())
        {
            Debug.LogError("[InventoryUI] Initialisation impossible : références manquantes.");
            enabled = false;
            return;
        }

        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryUpdated += UpdateUI;
        }
        else
        {
            Debug.LogError("[InventoryUI] InventoryManager introuvable dans la scène !");
            enabled = false;
            return;
        }

        UpdateUI(); // Mise à jour initiale de l'UI
    }

    private bool ValidateReferences()
    {
        if (inventorySlotPrefab == null)
        {
            Debug.LogError("[InventoryUI] Le prefab 'inventorySlotPrefab' n'est pas assigné !");
            return false;
        }

        if (gridParent == null)
        {
            Debug.LogError("[InventoryUI] Le parent 'gridParent' n'est pas assigné !");
            return false;
        }

        // Vérifie que le prefab contient les GameObjects nécessaires
        var tempSlot = Instantiate(inventorySlotPrefab);
        bool isValid = ValidateSlotPrefab(tempSlot);
        Destroy(tempSlot);

        return isValid;
    }

    private bool ValidateSlotPrefab(GameObject slotPrefab)
    {
        bool isValid = true;

        // Vérifie la présence d'un composant Button
        if (!slotPrefab.TryGetComponent<Button>(out _))
        {
            Debug.LogError($"[InventoryUI] Le prefab '{slotPrefab.name}' doit avoir un composant Button à sa racine !");
            isValid = false;
        }

        // Vérifie la présence de 'slotImage' et son composant Image
        var slotImageGO = slotPrefab.transform.Find("slotImage");
        if (slotImageGO == null || !slotImageGO.TryGetComponent<Image>(out _))
        {
            Debug.LogError($"[InventoryUI] Le prefab '{slotPrefab.name}' doit avoir un enfant 'slotImage' avec un composant Image !");
            isValid = false;
        }

        // Vérifie la présence de 'slotTxt' et son composant TextMeshProUGUI
        var slotTxtGO = slotPrefab.transform.Find("slotTxt");
        if (slotTxtGO == null || !slotTxtGO.TryGetComponent<TextMeshProUGUI>(out _))
        {
            Debug.LogError($"[InventoryUI] Le prefab '{slotPrefab.name}' doit avoir un enfant 'slotTxt' avec un composant TextMeshProUGUI !");
            isValid = false;
        }

        return isValid;
    }

    public void UpdateUI()
    {
        if (!enabled || inventoryManager == null || gridParent == null || inventorySlotPrefab == null)
        {
            return;
        }

        ClearGrid();

        if (inventoryManager.inventorySlots.Count == 0)
        {
            HandleEmptyInventory();
            return;
        }

        CreateInventorySlots();
    }

    private void ClearGrid()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void HandleEmptyInventory()
    {
        if (emptyInventoryMessage != null)
        {
            emptyInventoryMessage.gameObject.SetActive(true);
        }
        ClearItemDetails();
    }

    private void CreateInventorySlots()
    {
        if (emptyInventoryMessage != null)
        {
            emptyInventoryMessage.gameObject.SetActive(false);
        }

        foreach (var slot in inventoryManager.inventorySlots)
        {
            if (slot.item == null)
            {
                Debug.LogWarning("[InventoryUI] Un slot contient un item null, il sera ignoré.");
                continue;
            }

            GameObject slotGO = Instantiate(inventorySlotPrefab, gridParent);
            if (!ConfigureSlot(slotGO, slot))
            {
                Debug.LogError("[InventoryUI] Configuration du slot échouée. Vérifiez le prefab.");
            }
        }
    }

    private bool ConfigureSlot(GameObject slotGO, InventorySlot slot)
    {
        bool isConfigured = true;

        // Configure le bouton
        if (slotGO.TryGetComponent<Button>(out Button slotButton))
        {
            slotButton.onClick.AddListener(() => ShowItemDetails(slot.item));
        }
        else
        {
            Debug.LogError($"[InventoryUI] Le prefab '{slotGO.name}' est introuvable ou mal configuré avec un composant Button !");
            isConfigured = false;
        }

        // Configure l'image du slot
        var slotImageGO = slotGO.transform.Find("slotImage");
        if (slotImageGO != null && slotImageGO.TryGetComponent<Image>(out Image slotImage))
        {
            slotImage.sprite = slot.item.icon;
        }
        else
        {
            Debug.LogError($"[InventoryUI] Le prefab '{slotGO.name}' est introuvable ou mal configuré avec un composant Image !");
            isConfigured = false;
        }

        // Configure le texte du slot
        var slotTxtGO = slotGO.transform.Find("slotTxt");
        if (slotTxtGO != null && slotTxtGO.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI slotText))
        {
            slotText.text = slot.amount > 1 ? slot.amount.ToString() : "";
        }
        else
        {
            Debug.LogError($"[InventoryUI] Le prefab '{slotGO.name}' est introuvable ou mal configuré avec un composant TextMeshProUGUI !");
            isConfigured = false;
        }

        return isConfigured;
    }

    public void ShowItemDetails(InventoryItem item)
    {
        if (item == null)
        {
            ClearItemDetails();
            return;
        }

        UpdateItemDetails(item);
        UpdateSlotSelection();
    }

    private void UpdateItemDetails(InventoryItem item)
    {
        if (itemIcon != null) itemIcon.sprite = item.icon;
        if (itemName != null) itemName.text = item.itemName;
        if (itemDescription != null) itemDescription.text = item.description;
    }

    private void UpdateSlotSelection()
    {
        // Réinitialise la couleur du slot précédemment sélectionné
        if (selectedSlot != null && selectedSlot.TryGetComponent<Image>(out Image previousImage))
        {
            previousImage.color = Color.white;
        }

        // Met en surbrillance le slot actuellement sélectionné
        var currentSelected = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject;
        if (currentSelected != null && currentSelected.TryGetComponent<Image>(out Image currentImage))
        {
            currentImage.color = Color.yellow;
            selectedSlot = currentSelected;
        }
    }

    public void ClearItemDetails()
    {
        if (itemIcon != null) itemIcon.sprite = null;
        if (itemName != null) itemName.text = "";
        if (itemDescription != null) itemDescription.text = "";
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryUpdated -= UpdateUI;
        }
    }
}
