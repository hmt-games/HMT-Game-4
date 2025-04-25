using System;
using System.Collections;
using System.Collections.Generic;
using HMT.Puppetry;
using UnityEngine;
using UnityEngine.UI;

public class PlantSelectionUIManager : MonoBehaviour
{
    private Animator _animator;

    public static PlantSelectionUIManager Instance;

    [SerializeField] private GameObject plantSlotParent;
    [SerializeField] private GameObject plantSlotPrefab;

    private Action<PlantBehavior, PuppetBehavior> _onSelectedCallback;

    private void Awake()
    {
        if (Instance) Destroy(this);
        else Instance = this;

        _animator = GetComponent<Animator>();
    }

    public void ShowSelection(List<PlantBehavior> plants, PuppetBehavior bot, Action<PlantBehavior, PuppetBehavior>onSelectedCallback)
    {
        _onSelectedCallback = onSelectedCallback;

        for (int i = plantSlotParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(plantSlotParent.transform.GetChild(i).gameObject);
        }

        foreach (PlantBehavior plant in plants)
        {
            GameObject nSlot = Instantiate(plantSlotPrefab, plantSlotParent.transform);
            Image plantImage = nSlot.transform.GetChild(0).GetComponent<Image>();
            Button selectBtn = nSlot.transform.GetChild(1).GetComponent<Button>();

            plantImage.sprite = plant.spriteRenderer.sprite;
            ScaleToFit(plantImage);
            selectBtn.onClick.AddListener(delegate { PlantSelected(plant, bot); });
        }
        
        Show();
    }

    public void PlantSelected(PlantBehavior plant, PuppetBehavior bot)
    {
        _onSelectedCallback(plant, bot);
        _onSelectedCallback = null;
        Hide();
    }

    public void Cancel()
    {
        if (_onSelectedCallback != null)
            _onSelectedCallback(null, null);
        Hide();
    }

    public void Show()
    {
        _animator.SetTrigger("show");
    }

    public void Hide()
    {
        _animator.SetTrigger("hide");
    }
    
    private void ScaleToFit(Image image)
    {
        RectTransform rectTransform = image.rectTransform;

        if (image.sprite == null) return;

        float spriteWidth = image.sprite.rect.width;
        float spriteHeight = image.sprite.rect.height;

        float rectWidth = rectTransform.rect.width;
        float rectHeight = rectTransform.rect.height;

        float scale = Mathf.Min(rectWidth / spriteWidth, rectHeight / spriteHeight);

        // Apply scale to the Image transform
        image.preserveAspect = true;
        image.SetNativeSize();
        image.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }
}
